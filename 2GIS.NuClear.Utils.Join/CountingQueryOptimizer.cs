using System;
using System.Linq;

namespace NuClear.Utils.Join
{
    public sealed class CountingQueryOptimizer<T1, T2> : IQueryOptimizer<T1, T2>
    {
        public bool TryOptimize(IQueryable<T1> left, IQueryable<T2> right, out IQueryable<T1> leftOptimized, out IQueryable<T2> rightOptimized)
        {
            // todo: ��������� ��������, ���� �� ������������ ������� ����� �����������, ���� ���� - ������� ���������� ������� �� ��������.
            throw new NotImplementedException();
        }
    }
}