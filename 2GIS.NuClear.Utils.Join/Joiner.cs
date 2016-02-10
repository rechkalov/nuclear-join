using System;
using System.Collections.Generic;

namespace NuClear.Utils.Join
{
    public sealed class Joiner<T1, T2, TKey, TOut> : IJoiner<T1, T2, TOut>
    {
        private readonly Func<T1, TKey> _leftKeyFunction;
        private readonly Func<T2, TKey> _rightKeyFunction;
        private readonly IComparer<TKey> _keyComparer;
        private readonly Func<T1, T2, TOut> _join;

        public Joiner(Func<T1, TKey> leftKeyFunction, Func<T2, TKey> rightKeyFunction, IComparer<TKey> keyComparer, Func<T1, T2, TOut> join)
        {
            _leftKeyFunction = leftKeyFunction;
            _rightKeyFunction = rightKeyFunction;
            _keyComparer = keyComparer;
            _join = join;
        }

        public IEnumerable<TOut> Join(IEnumerator<T1> left, IEnumerator<T2> right)
        {
            var hasNext = left.MoveNext() && right.MoveNext();

            while (hasNext)
            {
                var leftKey = _leftKeyFunction.Invoke(left.Current);
                var rightKey = _rightKeyFunction.Invoke(right.Current);
                var comared = _keyComparer.Compare(leftKey, rightKey);
                if (comared == 0)
                {
                    var leftAccumulator = new List<T1>();
                    var rightAccumulator = new List<T2>();

                    hasNext = Accumulate(leftAccumulator, leftKey, _leftKeyFunction, left) &
                              Accumulate(rightAccumulator, rightKey, _rightKeyFunction, right);

                    foreach (var leftItem in leftAccumulator)
                    {
                        foreach (var rightItem in rightAccumulator)
                        {
                            yield return _join.Invoke(leftItem, rightItem);
                        }
                    }
                }
                else
                {
                    hasNext = comared > 0
                        ? right.MoveNext()
                        : left.MoveNext();
                }
            }
        }

        private bool Accumulate<T>(List<T> accumulator, TKey key, Func<T, TKey> keyExtractor, IEnumerator<T> data)
        {
            do
            {
                accumulator.Add(data.Current);
                if (!data.MoveNext())
                {
                    return false;
                }
            } while (_keyComparer.Compare(key, keyExtractor.Invoke(data.Current)) == 0);

            return true;
        }
    }
}