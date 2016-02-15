using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NuClear.Utils.Join
{
    public class GroupJoinTest
    {
        [TestCaseSource(nameof(DataSources))]
        public void TestJoinOnly(IEnumerable<Foo> foos, IEnumerable<Foo> bars)
        {
            var actual = Factory.MemoryGroupJoin(foos.AsQueryable(), bars.AsQueryable(), f => f.Id, b => b.Id, (f, b) => new { Id = f.Id, BarCount = b.Count() });
            var expected = foos.GroupJoin(bars, f => f.Id, i => i.Id, (f, b) => new { Id = f.Id, BarCount = b.Count() });

            Assert.That(actual.ToArray(), Is.EquivalentTo(expected));
        }

        private IEnumerable<TestCaseData> DataSources()
        {
            yield return new TestCaseData(
                new[] { new Foo(2, "foo-1"), new Foo(3, "foo-2"), new Foo(4, "foo-2") },
                new[] { new Foo(1, "bar-1"), new Foo(3, "bar-2"), new Foo(5, "bar-2") });

            yield return new TestCaseData(
                new[] { new Foo(1, "bar-1"), new Foo(3, "bar-2"), new Foo(5, "bar-2") },
                new[] { new Foo(2, "foo-1"), new Foo(3, "foo-2"), new Foo(4, "foo-2") });

            yield return new TestCaseData(
                new[] { new Foo(1, "bar-1"), new Foo(2, "bar-2"), new Foo(3, "bar-2") },
                new[] { new Foo(3, "foo-1"), new Foo(4, "foo-2"), new Foo(5, "foo-2") });

            yield return new TestCaseData(
                new[] { new Foo(3, "foo-1"), new Foo(4, "foo-2"), new Foo(5, "foo-2") },
                new[] { new Foo(1, "bar-1"), new Foo(2, "bar-2"), new Foo(3, "bar-2") });

            yield return new TestCaseData(
                new[] { new Foo(1, "bar-1"), new Foo(3, "bar-2"), new Foo(5, "bar-2") },
                new Foo[0]);

            yield return new TestCaseData(
                new Foo[0],
                new[] { new Foo(1, "bar-1"), new Foo(3, "bar-2"), new Foo(5, "bar-2") });
        }

        public class Foo
        {
            public Foo(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
