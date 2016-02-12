using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    public static class Factory
    {
        public static IQueryable<TOut> MemoryJoin<T1, T2, TKey, TOut>(
            this IQueryable<T1> outer,
            Expression<Func<T1, TKey>> outerKeySelector,
            IQueryable<T2> inner,
            Expression<Func<T2, TKey>> innerKeySelector,
            Expression<Func<T1, T2, TOut>> resultSelector)
            where TKey : IComparable<TKey>
        {
            var optimizer = new SimpleQueryOptimizer<T1, T2, TKey>(outerKeySelector, innerKeySelector);
            var joiner = new Joiner<T1, T2, TKey, TOut>(outerKeySelector.Compile(), innerKeySelector.Compile(), resultSelector.Compile(), Comparer<TKey>.Default);
            var provider = new MemoryJoinProvider<T1, T2, TOut>(outer, inner, optimizer, joiner);
            return new MemoryJoinQueryable<TOut>(provider);
        }

        /// <summary>
        /// С возможностью указать сравнение ключа. Следует учитывать, что сравнение должно совпадать с тем, что реализовано на уровне хранилища, иначе гарантированы проблемы.
        /// </summary>
        public static IQueryable<TOut> MemoryJoin<T1, T2, TKey, TOut>(
            this IQueryable<T1> outer,
            Expression<Func<T1, TKey>> outerKeySelector,
            IQueryable<T2> inner,
            Expression<Func<T2, TKey>> innerKeySelector,
            Expression<Func<T1, T2, TOut>> resultSelector,
            IComparer<TKey> comparer)
        {
            var optimizer = new SimpleQueryOptimizer<T1, T2, TKey>(outerKeySelector, innerKeySelector);
            var joiner = new Joiner<T1, T2, TKey, TOut>(outerKeySelector.Compile(), innerKeySelector.Compile(), resultSelector.Compile(), comparer);
            var provider = new MemoryJoinProvider<T1, T2, TOut>(outer, inner, optimizer, joiner);
            return new MemoryJoinQueryable<TOut>(provider);
        }

        /// <summary>
        /// С возможностью указать сравнение ключа. Следует учитывать, что сравнение должно совпадать с тем, что реализовано на уровне хранилища, иначе гарантированы проблемы.
        /// </summary>
        public static IQueryable<TOut> MemoryJoin<T1, T2, TKey, TOut>(
            this IQueryable<T1> outer,
            Expression<Func<T1, TKey>> outerKeySelector,
            IQueryable<T2> inner,
            Expression<Func<T2, TKey>> innerKeySelector,
            Expression<Func<T1, T2, TOut>> resultSelector,
            Comparison<TKey> comparison)
        {
            var optimizer = new SimpleQueryOptimizer<T1, T2, TKey>(outerKeySelector, innerKeySelector);
            var joiner = new Joiner<T1, T2, TKey, TOut>(outerKeySelector.Compile(), innerKeySelector.Compile(), resultSelector.Compile(), Comparer<TKey>.Create(comparison));
            var provider = new MemoryJoinProvider<T1, T2, TOut>(outer, inner, optimizer, joiner);
            return new MemoryJoinQueryable<TOut>(provider);
        }
    }
}
