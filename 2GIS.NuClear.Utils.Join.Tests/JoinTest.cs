using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NuClear.Utils.Join
{
    public class JoinTest
    {
        [TestCaseSource(nameof(DataSources))]
        public void TestJoinOnly(IEnumerable<Foo> foos, IEnumerable<Bar> bars, int expected)
        {
            var joined = Factory.MemoryJoin(foos.AsQueryable(), f => f.Id, bars.AsQueryable(), b => b.Id, (f, b) => new { FooName = f.Name, BarName = b.Name });
            Assert.That(joined.ToArray(), Has.Length.EqualTo(expected));
        }

        [TestCaseSource(nameof(DataSources))]
        public void TestJoinWithPostExpression(IEnumerable<Foo> foos, IEnumerable<Bar> bars, int expected)
        {
            var joined = Factory.MemoryJoin(foos.AsQueryable(), f => f.Id, bars.AsQueryable(), b => b.Id, (f, b) => new { f.Id, FooName = f.Name, BarName = b.Name })
                .Where(x => x.FooName != null) // todo: Влияет на сложность слияния, хочется, чтобы выполнялось до материализации.
                .Select(x => x.BarName) // todo: Влияет на объём материализуемых данных
                .ToArray();

            Assert.That(joined.ToArray(), Has.Length.EqualTo(expected));
        }

        private IEnumerable<TestCaseData> DataSources()
        {
            yield return new TestCaseData(
                new Foo[0],
                new Bar[0],
                0);

            yield return new TestCaseData(
                new[] { new Foo(1, "foo-1") },
                new[] { new Bar(1, "bar-1") },
                1);

            yield return new TestCaseData(
                new[] { new Foo(1, "foo-1"), new Foo(1, "foo-2") },
                new[] { new Bar(1, "bar-1") },
                2);

            yield return new TestCaseData(
                new[] { new Foo(1, "foo-1") },
                new[] { new Bar(1, "bar-1"), new Bar(1, "bar-2") },
                2);

            yield return new TestCaseData(
                new[] { new Foo(1, "foo-1"), new Foo(1, "foo-2") },
                new[] { new Bar(1, "bar-1"), new Bar(1, "bar-2") },
                4);

            yield return new TestCaseData(
                new[] { new Foo(1, "foo-1"), new Foo(2, "foo-2"), new Foo(3, "foo-2") },
                new[] { new Bar(3, "bar-1"), new Bar(2, "bar-2"), new Bar(1, "bar-2") },
                3);

            yield return new TestCaseData(
                new[] { new Foo(1, "foo-1"), new Foo(3, "foo-2"), new Foo(5, "foo-2") },
                new[] { new Bar(2, "bar-1"), new Bar(3, "bar-2"), new Bar(4, "bar-2") },
                1);

            yield return new TestCaseData(
                new[] { new Foo(2, "foo-1"), new Foo(3, "foo-2"), new Foo(4, "foo-2") },
                new[] { new Bar(1, "bar-1"), new Bar(3, "bar-2"), new Bar(5, "bar-2") },
                1);
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

        public class Bar
        {
            public Bar(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
