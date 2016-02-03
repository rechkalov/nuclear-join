using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    public class Replacer
    {
        public Expression<Func<IEnumerable<TParam>, IEnumerable<TResult>>> Convert<TParam, TResult>(
            Expression<Func<IQueryable<TParam>, IQueryable<TResult>>> expression)
        {
            var param = Expression.Parameter(typeof (IEnumerable<TParam>));

            var visitor = new ReplacingVisitor(
                node => node == expression.Parameters.Single() ? param : node,
                new MethodReplasingRegistry());

            var newBody = visitor.Visit(expression.Body);

            return Expression.Lambda<Func<IEnumerable<TParam>, IEnumerable<TResult>>>(newBody, param);
        }

        public Expression Convert(Expression oldDataSource, Expression newDataSource, Expression oldBody)
        {
            var visitor = new ReplacingVisitor(node => node == oldDataSource ? newDataSource : node, new MethodReplasingRegistry());
            return visitor.Visit(oldBody);
        }

        public Expression Convert(Func<Expression, Expression> dataSourceReplacer, Expression oldBody)
        {
            var visitor = new ReplacingVisitor(dataSourceReplacer, new MethodReplasingRegistry());
            return visitor.Visit(oldBody);
        }
    }
}