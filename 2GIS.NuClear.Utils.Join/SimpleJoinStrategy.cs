using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    public sealed class SimpleJoinStrategy<T1, T2, TKey> : IJoinStrategy<T1, T2>
    {
        private readonly IQueryable<T1> _left;
        private readonly Expression<Func<T1, TKey>> _leftKeyExpression;
        private readonly IQueryable<T2> _right;
        private readonly Expression<Func<T2, TKey>> _rightKeyExpression;
        private readonly IComparer<TKey> _keyComparer;

        public SimpleJoinStrategy(
            IQueryable<T1> left,
            Expression<Func<T1, TKey>> leftKeyExpression,
            IQueryable<T2> right,
            Expression<Func<T2, TKey>> rightKeyExpression,
            IComparer<TKey> keyComparer)
        {
            _left = left;
            _leftKeyExpression = leftKeyExpression;
            _right = right;
            _rightKeyExpression = rightKeyExpression;
            _keyComparer = keyComparer;
        }

        public IEnumerator<Tuple<T1, T2>> GetEnumerator()
        {
            var leftKeyFunction = _leftKeyExpression.Compile();
            var rightKeyFunction = _rightKeyExpression.Compile();
            var left = _left.OrderBy(_leftKeyExpression).GetEnumerator();
            var right = _right.OrderBy(_rightKeyExpression).GetEnumerator();

            Func<bool> next = () => left.MoveNext() && right.MoveNext();

            while (next.Invoke())
            {
                var leftKey = leftKeyFunction.Invoke(left.Current);
                var rightKey = rightKeyFunction.Invoke(right.Current);
                var comared = _keyComparer.Compare(leftKey, rightKey);
                if (comared == 0)
                {
                    var leftAccumulator = new List<T1>();
                    var rightAccumulator = new List<T2>();

                    next = Accumulate(leftAccumulator, leftKey, leftKeyFunction, left) &
                           Accumulate(rightAccumulator, rightKey, rightKeyFunction, right)
                        ? (Func<bool>) (() => true)
                        : (Func<bool>) (() => false);

                    foreach (var leftItem in leftAccumulator)
                    {
                        foreach (var rightItem in rightAccumulator)
                        {
                            yield return Tuple.Create(leftItem, rightItem);
                        }
                    }
                }
                else
                {
                    next = comared > 0
                        ? (Func<bool>)(() => right.MoveNext())
                        : (Func<bool>)(() => left.MoveNext());
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}