using System;
using System.Collections.Generic;

namespace NuClear.Utils.Join
{
    public sealed class Joiner<T1, T2, TKey, TOut> : IJoiner<T1, T2, TOut>
    {
        private readonly Func<T1, TKey> _outerKeySelector;
        private readonly Func<T2, TKey> _innerKeySelector;
        private readonly Func<T1, T2, TOut> _resultSelector;
        private readonly IComparer<TKey> _keyComparer;

        public Joiner(Func<T1, TKey> outerKeySelector, Func<T2, TKey> innerKeySelector, Func<T1, T2, TOut> resultSelector, IComparer<TKey> keyComparer)
        {
            _outerKeySelector = outerKeySelector;
            _innerKeySelector = innerKeySelector;
            _resultSelector = resultSelector;
            _keyComparer = keyComparer;
        }

        public IEnumerable<TOut> Join(IEnumerator<T1> outer, IEnumerator<T2> inner)
        {
            var hasNext = outer.MoveNext() && inner.MoveNext();

            while (hasNext)
            {
                var outerKey = _outerKeySelector.Invoke(outer.Current);
                var innerKey = _innerKeySelector.Invoke(inner.Current);
                var comared = _keyComparer.Compare(outerKey, innerKey);
                if (comared == 0)
                {
                    var outerAccumulator = new List<T1>();
                    var innerAccumulator = new List<T2>();

                    hasNext = Accumulate(outerAccumulator, outerKey, _outerKeySelector, outer) &
                              Accumulate(innerAccumulator, innerKey, _innerKeySelector, inner);

                    foreach (var outerItem in outerAccumulator)
                    {
                        foreach (var innerItem in innerAccumulator)
                        {
                            yield return _resultSelector.Invoke(outerItem, innerItem);
                        }
                    }
                }
                else
                {
                    hasNext = comared > 0
                        ? inner.MoveNext()
                        : outer.MoveNext();
                }
            }

            outer.Dispose();
            inner.Dispose();
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