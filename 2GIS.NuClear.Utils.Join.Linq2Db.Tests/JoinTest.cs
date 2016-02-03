using System;
using System.Diagnostics;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.Mapping;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NuClear.Utils.Join
{
    public class JoinTest
    {
        [SetUp]
        public void SetUp()
        {
            DataConnection.TraceSwitch.Level = TraceLevel.Verbose;
            DataConnection.TurnTraceSwitchOn(TraceLevel.Verbose);
            DataConnection.WriteTraceLine = (s1, s2) => Debug.WriteLine(s1, s2);
        }

        [Test]
        public void TestClassicJoin()
        {
            Test(ClassicJoin, Throws.Exception);
        }

        [Test]
        public void TestInmemoryJoin()
        {
            Test(InmemoryJoin, Throws.Nothing);
        }

        private static void Test(Func<IQueryable<Order>, IQueryable<OrderPosition>, IQueryable<long>> func, IResolveConstraint expected)
        {
            var schema1 = new MappingSchema();
            schema1.GetFluentMappingBuilder()
                .Entity<Order>()
                .HasSchemaName("Billing")
                .HasTableName("Orders");

            var schema2 = new MappingSchema();
            schema2.GetFluentMappingBuilder()
                .Entity<OrderPosition>()
                .HasSchemaName("Billing")
                .HasTableName("OrderPositions");

            using (var source1 = new DataConnection("Source1").AddMappingSchema(schema1))
            using (var source2 = new DataConnection("Source2").AddMappingSchema(schema2))
            {
                var orders = source1.GetTable<Order>().Where(x => x.Id < 100);
                var positions = source2.GetTable<OrderPosition>().Where(x => x.Id < 100);
                var join = func.Invoke(orders, positions);

                Assert.That(() => join.ToArray(), expected);
            }
        }

        private static IQueryable<long> ClassicJoin(IQueryable<Order> orders, IQueryable<OrderPosition> positions)
        {
            return orders.Join(positions, o => o.Id, p => p.OrderId, (order, position) => order.Id);
        }

        private static IQueryable<long> InmemoryJoin(IQueryable<Order> orders, IQueryable<OrderPosition> positions)
        {
            return Factory.Create(orders, o => o.Id, positions, p => p.OrderId, (order, position) => order.Id);
        }

        class Order
        {
            public long Id { get; set; }
            public string Number { get; set; }
        }

        class OrderPosition
        {
            public long Id { get; set; }
            public long OrderId { get; set; }
        }
    }
}
