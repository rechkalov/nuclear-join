using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NuClear.Utils.Join
{
    internal sealed class ReplacingVisitor : ExpressionVisitor
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