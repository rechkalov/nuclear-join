using System.Linq;

namespace NuClear.Utils.Join
{
    public interface IQueryOptimizer<T1, T2>
    {
        bool TryOptimize(IQueryable<T1> left, IQueryable<T2> right, out IQueryable<T1> leftOptimized, out IQueryable<T2> rightOptimized);
    }
}