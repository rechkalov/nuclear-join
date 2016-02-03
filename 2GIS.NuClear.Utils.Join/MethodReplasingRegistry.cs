using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NuClear.Utils.Join
{
    internal sealed class MethodReplasingRegistry
    {
        private readonly IDictionary<MethodInfo, MethodInfo> _replasingTable = new Dictionary<MethodInfo, MethodInfo>
        {
            {
                GetGenericMethodInfo(Queryable.Where, default(IQueryable<object>), default(Expression<Func<object, bool>>)),
                GetGenericMethodInfo(Enumerable.Where, default(IEnumerable<object>), default(Func<object, bool>))
            },
            {
                GetGenericMethodInfo(Queryable.OrderBy, default(IQueryable<object>), default(Expression<Func<object, object>>)),
                GetGenericMethodInfo(Enumerable.OrderBy, default(IEnumerable<object>), default(Func<object, object>))
            },
            {
                GetGenericMethodInfo(Queryable.Select, default(IQueryable<object>), default(Expression<Func<object, object>>)),
                GetGenericMethodInfo(Enumerable.Select, default(IEnumerable<object>), default(Func<object, object>))
            },
        };

        public bool TryReplaceMethod(MethodInfo method, out MethodInfo replacedMethod)
        {
            return TryProcessGenericMethod(method, out replacedMethod) || TryProcessMethod(method, out replacedMethod);
        }

        private bool TryProcessGenericMethod(MethodInfo method, out MethodInfo replacedMethod)
        {
            MethodInfo newGenericMethod;
            if (method.IsGenericMethod && _replasingTable.TryGetValue(method.GetGenericMethodDefinition(), out newGenericMethod))
            {
                replacedMethod = newGenericMethod.MakeGenericMethod(method.GetGenericArguments());
                return true;
            }

            replacedMethod = null;
            return false;
        }

        private bool TryProcessMethod(MethodInfo method, out MethodInfo replacedMethod)
        {
            return _replasingTable.TryGetValue(method, out replacedMethod);
        }

        private static MethodInfo GetGenericMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
        {
            return f.Method.GetGenericMethodDefinition();
        }

        private static MethodInfo GetGenericMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.Method.GetGenericMethodDefinition();
        }

        private static MethodInfo GetGenericMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3)
        {
            return f.Method.GetGenericMethodDefinition();
        }
    }
}