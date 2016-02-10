using System.Collections.Generic;

namespace NuClear.Utils.Join
{
    public interface IJoiner<in T1, in T2, out TJoinResult>
    {
        /// <summary>
        /// Объединяет две последовательности, основываясь на предположении, что они уже отсортированы по ключу слияния.
        /// </summary>
        IEnumerable<TJoinResult> Join(IEnumerator<T1> left, IEnumerator<T2> right);
    }
}