using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NuClear.Utils.Join
{
    public sealed class Replacer
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

        private sealed class ReplacingVisitor : ExpressionVisitor
        {
            private readonly Func<Expression, Expression> _dataSourceReplacer;
            private readonly MethodReplasingRegistry _registry;

            public ReplacingVisitor(Func<Expression, Expression> dataSourceReplacer, MethodReplasingRegistry registry)
            {
                _registry = registry;
                _dataSourceReplacer = dataSourceReplacer;
            }

            public override Expression Visit(Expression node)
            {
                return node != null
                    ? base.Visit(_dataSourceReplacer.Invoke(node))
                    : null;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Quote)
                {
                    return node.Operand;
                }

                return base.VisitUnary(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                try
                {
                    MethodInfo newMethod;
                    return _registry.TryReplaceMethod(node.Method, out newMethod)
                        ? Expression.Call(Visit(node.Object), newMethod, VisitAndConvert(node.Arguments, nameof(VisitMethodCall)))
                        : base.VisitMethodCall(node);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"unsupported method call: {node.Method}", ex);
                }
            }
        }
    }
}