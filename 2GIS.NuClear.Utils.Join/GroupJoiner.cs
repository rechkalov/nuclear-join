using System;
using System.Collections.Generic;
using System.Linq;

namespace NuClear.Utils.Join
{
    public sealed class GroupJoiner<T1, T2, TKey, TOut> : IJoiner<T1, T2, TOut>
    {
        private readonly Func<T1, TKey> _outerKeySelector;
        private readonly Func<T2, TKey> _innerKeySelector;
        private readonly Func<T1, IEnumerable<T2>, TOut> _resultSelector;
        private readonly IComparer<TKey> _keyComparer;

        public GroupJoiner(Func<T1, TKey> outerKeySelector, Func<T2, TKey> innerKeySelector, Func<T1, IEnumerable<T2>, TOut> resultSelector, IComparer<TKey> keyComparer)
        {
            _outerKeySelector = outerKeySelector;
            _innerKeySelector = innerKeySelector;
            _resultSelector = resultSelector;
            _keyComparer = keyComparer;
        }

        public IEnumerable<TOut> Join(IEnumerator<T1> outer, IEnumerator<T2> inner)
        {
            var hasNextOuter = outer.MoveNext();
            var hasNextInner = inner.MoveNext();

            while (hasNextOuter)
            {
                if (!hasNextInner)
                {
                    yield return _resultSelector.Invoke(outer.Current, Enumerable.Empty<T2>());
                    hasNextOuter = outer.MoveNext();
                    continue;
                }

                var outerKey = _outerKeySelector.Invoke(outer.Current);
                var innerKey = _innerKeySelector.Invoke(inner.Current);
                var comared = _keyComparer.Compare(outerKey, innerKey);
                if (comared == 0)
                {
                    var outerAccumulator = new List<T1>();
                    var innerAccumulator = new List<T2>();

                    hasNextOuter = Accumulate(outerAccumulator, outerKey, _outerKeySelector, outer);
                    hasNextInner = Accumulate(innerAccumulator, innerKey, _innerKeySelector, inner);

                    foreach (var outerItem in outerAccumulator)
                    {
                        yield return _resultSelector.Invoke(outerItem, innerAccumulator);
                    }
                }
                else
                {
                    if (comared > 0)
                    {
                        hasNextInner = inner.MoveNext();
                    }
                    else
                    {
                        yield return _resultSelector.Invoke(outer.Current, Enumerable.Empty<T2>());
                        hasNextOuter = outer.MoveNext();
                    }
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