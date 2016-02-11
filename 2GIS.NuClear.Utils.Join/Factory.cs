using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    public static class Factory
    {
        public static IQueryable<TOut> MemoryJoin<T1, T2, TKey, TOut>(
            this IQueryable<T1> left,
            Expression<Func<T1, TKey>> leftKeySelector,
            IQueryable<T2> right,
            Expression<Func<T2, TKey>> rightKeySelector,
            Expression<Func<T1, T2, TOut>> mergeExpression)
            where TKey : IComparable
        {
            var optimizer = new SimpleQueryOptimizer<T1, T2, TKey>(leftKeySelector, rightKeySelector);
            var joiner = new Joiner<T1, T2, TKey, TOut>(leftKeySelector.Compile(), rightKeySelector.Compile(), Comparer<TKey>.Default, mergeExpression.Compile());
            var provider = new MemoryJoinProvider<T1, T2, TOut>(left, right, optimizer, joiner);
            return new MemoryJoinQueryable<TOut>(provider);
        }

        /// <summary>
        /// С возможностью указать сравнение ключа. Следует учитывать, что сравнение должно совпадать с тем, что реализовано на уровне хранилища, иначе гарантированы проблемы.
        /// </summary>
        public static IQueryable<TOut> MemoryJoin<T1, T2, TKey, TOut>(
            this IQueryable<T1> left,
            Expression<Func<T1, TKey>> leftKeySelector,
            IQueryable<T2> right,
            Expression<Func<T2, TKey>> rightKeySelector,
            Expression<Func<T1, T2, TOut>> mergeExpression,
            IComparer<TKey> comparer)
        {
            var optimizer = new SimpleQueryOptimizer<T1, T2, TKey>(leftKeySelector, rightKeySelector);
            var joiner = new Joiner<T1, T2, TKey, TOut>(leftKeySelector.Compile(), rightKeySelector.Compile(), comparer, mergeExpression.Compile());
            var provider = new MemoryJoinProvider<T1, T2, TOut>(left, right, optimizer, joiner);
            return new MemoryJoinQueryable<TOut>(provider);
        }

        /// <summary>
        /// С возможностью указать сравнение ключа. Следует учитывать, что сравнение должно совпадать с тем, что реализовано на уровне хранилища, иначе гарантированы проблемы.
        /// </summary>
        public static IQueryable<TOut> MemoryJoin<T1, T2, TKey, TOut>(
            this IQueryable<T1> left,
            Expression<Func<T1, TKey>> leftKeySelector,
            IQueryable<T2> right,
            Expression<Func<T2, TKey>> rightKeySelector,
            Expression<Func<T1, T2, TOut>> mergeExpression,
            Comparison<TKey> comparison)
        {
            var optimizer = new SimpleQueryOptimizer<T1, T2, TKey>(leftKeySelector, rightKeySelector);
            var joiner = new Joiner<T1, T2, TKey, TOut>(leftKeySelector.Compile(), rightKeySelector.Compile(), Comparer<TKey>.Create(comparison), mergeExpression.Compile());
            var provider = new MemoryJoinProvider<T1, T2, TOut>(left, right, optimizer, joiner);
            return new MemoryJoinQueryable<TOut>(provider);
        }
    }
}
