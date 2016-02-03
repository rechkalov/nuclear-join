using System;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    class MemoryJoinProvider<T1, T2, TResult> : IQueryProvider
    {
        // todo: материал для дальнейшего совершествования: http://blogs.msdn.com/b/mattwar/archive/2008/11/18/linq-links.aspx

        private readonly IJoinStrategy<T1, T2> _joinStrategy;
        private readonly Expression<Func<T1, T2, TResult>> _joinExpression;

        public MemoryJoinProvider(IJoinStrategy<T1, T2> joinStrategy, Expression<Func<T1, T2, TResult>> joinExpression)
        {
            _joinStrategy = joinStrategy;
            _joinExpression = joinExpression;
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
            var join = _joinExpression.Compile();
            var joinedData = _joinStrategy.Select(tuple => join.Invoke(tuple.Item1, tuple.Item2));

            var result = Expression.Constant(joinedData);
            var newBody = new Replacer().Convert(
                node => node.NodeType == ExpressionType.Constant && ((ConstantExpression)node).Value is ISecretMarker ? result : node,
                expression);
            return Expression.Lambda(newBody).Compile().DynamicInvoke();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}