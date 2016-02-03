using System;
using System.Collections.Generic;

namespace NuClear.Utils.Join
{
    public interface IJoinStrategy<T1, T2> : IEnumerable<Tuple<T1, T2>>
    {
    }
}