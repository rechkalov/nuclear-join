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

        [Test]
        public void ResourcesShouldBeFreed()
        {
            using (var source1 = CreateOrdersDbConnection())
            using (var source2 = CreateOrderPositionsDbConnection())
            {
                var orders = source1.GetTable<Order>().Where(x => x.Id < 10);
                var positions = source2.GetTable<OrderPosition>().Where(x => 5 < x.OrderId && x.OrderId < 20);
                var join = Factory.MemoryJoin(orders, o => o.Id, positions, p => p.OrderId, (order, position) => order.Id).ToArray();

                Assert.That(() => join.ToArray(), Throws.Nothing);
                Assert.That(() => orders.ToArray(), Throws.Nothing);
                Assert.That(() => positions.ToArray(), Throws.Nothing);
            }
        }

        private static void Test(Func<IQueryable<Order>, IQueryable<OrderPosition>, IQueryable<long>> func, IResolveConstraint expected)
        {
            using (var source1 = CreateOrdersDbConnection())
            using (var source2 = CreateOrderPositionsDbConnection())
            {
                var orders = source1.GetTable<Order>().Where(x => x.Id < 1000);
                var positions = source2.GetTable<OrderPosition>().Where(x => x.OrderId < 2000);
                var join = func.Invoke(orders, positions);

                Assert.That(() => join.ToArray(), expected);
            }
        }

        private static DataConnection CreateOrdersDbConnection()
        {
            var schema = new MappingSchema();
            schema.GetFluentMappingBuilder()
                .Entity<Order>()
                .HasSchemaName("Billing")
                .HasTableName("Orders");

            return new DataConnection("Source1").AddMappingSchema(schema);
        }

        private static DataConnection CreateOrderPositionsDbConnection()
        {
            var schema = new MappingSchema();
            schema.GetFluentMappingBuilder()
                .Entity<OrderPosition>()
                .HasSchemaName("Billing")
                .HasTableName("OrderPositions");

            return new DataConnection("Source2").AddMappingSchema(schema);
        }

        private static IQueryable<long> ClassicJoin(IQueryable<Order> orders, IQueryable<OrderPosition> positions)
        {
            return orders.Join(positions, o => o.Id, p => p.OrderId, (order, position) => order.Id);
        }

        private static IQueryable<long> InmemoryJoin(IQueryable<Order> orders, IQueryable<OrderPosition> positions)
        {
            return Factory.MemoryJoin(orders, o => o.Id, positions, p => p.OrderId, (order, position) => order.Id);
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
