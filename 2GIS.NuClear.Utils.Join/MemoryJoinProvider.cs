using System;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    internal class MemoryJoinProvider<T1, T2, TResult> : IQueryProvider
    {
        // todo: http://blogs.msdn.com/b/mattwar/archive/2008/11/18/linq-links.aspx

        private readonly IQueryable<T1> _outer;
        private readonly IQueryable<T2> _inner;

        private readonly IQueryOptimizer<T1, T2> _queryOptimizer;
        private readonly IJoiner<T1, T2, TResult> _joiner;

        public MemoryJoinProvider(IQueryable<T1> outer, IQueryable<T2> inner, IQueryOptimizer<T1, T2> queryOptimizer, IJoiner<T1, T2, TResult> joiner)
        {
            _outer = outer;
            _inner = inner;
            _queryOptimizer = queryOptimizer;
            _joiner = joiner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return (IQueryable)Activator.CreateInstance(typeof(MemoryJoinQueryable<>).MakeGenericType(expression.Type),
                                                        new object[] { this, expression });
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new MemoryJoinQueryable<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return Expression.Lambda<Func<object>>(ExcuteExpression(expression)).Compile().Invoke();
        }

        public T Execute<T>(Expression expression)
        {
            return Expression.Lambda<Func<T>>(ExcuteExpression(expression)).Compile().Invoke();
        }

        private Expression ExcuteExpression(Expression expression)
        {
            IQueryable<T1> outer;
            IQueryable<T2> inner;
            _queryOptimizer.TryOptimize(_outer, _inner, out outer, out inner);

            var joinedSequence = _joiner.Join(outer.GetEnumerator(), inner.GetEnumerator());

            var result = Expression.Constant(joinedSequence);
            return new Replacer().Convert(
                node => node.NodeType == ExpressionType.Constant && ((ConstantExpression) node).Value is ISecretMarker
                        ? result
                        : node,
                expression);
        }
    }
}