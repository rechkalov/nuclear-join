using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace NuClear.Utils.Join
{
    public class ReplacerTest
    {
        [Test]
        public void TestExplicitExpression()
        {
            Expression<Func<IQueryable<int>, IQueryable<int>>> ex = ints => ints.OrderBy(i => i).Where(i => i > 10).Select(i => i);

            var e2 = new Replacer().Convert(ex);
            var func = e2.Compile();
            var x = func.Invoke(new[] { 1, 5, 10, 15, 25, 20 });

            Assert.That(x, Is.EqualTo(new[] { 15, 20, 25 }));
        }

        [Test]
        public void TestImplicitExpression()
        {
            var q = new[] { 1, 2, 3 }.AsQueryable();
            var ex = q.OrderBy(i => i).Where(i => i > 2).Select(i => i).Expression;
            var newParam = Expression.Constant(new[] { 6, 4, 5, });

            var newBody = new Replacer().Convert(q.Expression, newParam, ex);
            var result = Expression.Lambda(newBody).Compile().DynamicInvoke();

            Assert.That(result, Is.EqualTo(new[] { 4, 5, 6 }));
        }

        [Test]
        public void X()
        {
            var q = new[] { 1, 2, 3 }.AsEnumerable();
            var e = q.GetEnumerator();

            while (e.MoveNext())
                Debug.WriteLine(e.Current);
        }
    }
}
