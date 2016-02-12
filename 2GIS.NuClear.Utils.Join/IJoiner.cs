using System.Collections.Generic;

namespace NuClear.Utils.Join
{
    public interface IJoiner<in T1, in T2, out TJoinResult>
    {
        /// <summary>
        /// ���������� ��� ������������������, ����������� �� �������������, ��� ��� ��� ������������� �� ����� �������.
        /// </summary>
        IEnumerable<TJoinResult> Join(IEnumerator<T1> outer, IEnumerator<T2> inner);
    }
}