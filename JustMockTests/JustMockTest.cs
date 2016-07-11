using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Telerik.JustMock.Core;
using Telerik.JustMock.Helpers;

namespace JustMockTests
{
    [TestClass]
    public class JustMockTest
    {
        [TestMethod]
        public void OrderTest_Collect()
        {
            // arrange
            var warehouse = Mock.Create<IWarehouse>();
            var order = new Order("Camera", 2);

            bool called = false;
            Mock.Arrange(() => warehouse.HasInventory("Camera", 2)).DoInstead(() => called = true);

            // act
            order.Fill(warehouse);

            // assert
            Assert.IsTrue(called);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void OrderTest_Throws()
        {
            // arrange
            var order = new Order("Camera", 0);
            var warehouse = Mock.Create<IWarehouse>();

            Mock.Arrange(() => warehouse.HasInventory(Arg.IsAny<string>(), Arg.IsAny<int>())).Returns(true);
            Mock.Arrange(() => warehouse.Remove(Arg.IsAny<string>(), Arg.Matches<int>(x => x == 0))).Throws(new InvalidOperationException());

            // act
            order.Fill(warehouse);
        }

        [TestMethod]
        public void WarehouseProperty_Collect()
        {
            // arrange
            var warehouse = Mock.Create<IWarehouse>();
            Mock.Arrange(() => warehouse.Manager).Returns("John");
            string manager = string.Empty;

            // act
            manager = warehouse.Manager;

            // assert
            Assert.AreEqual("John", manager);
        }

        [TestMethod]
        [ExpectedException(typeof(StrictMockException))]
        public void WarehousePropertySet()
        {
            // arrange
            var warehouse = Mock.Create<IWarehouse>(Behavior.Strict);
            Mock.ArrangeSet(() => warehouse.Manager = "John");

            // act
            warehouse.Manager = "Scott";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WarehousePropertySet_Throws()
        {
            // arrange
            var warehouse = Mock.Create<IWarehouse>();
            Mock.ArrangeSet(() => warehouse.Manager = "John").Throws<ArgumentException>();

            // act
            warehouse.Manager = "Scott";

            warehouse.Manager = "John";
        }

        [TestMethod]
        public void RaisingAnEvent()
        {
            // arrange
            var warehouse = Mock.Create<IWarehouse>();
            Mock.Arrange(() => warehouse.Remove(Arg.IsAny<string>(), Arg.IsInRange(int.MinValue, int.MaxValue, RangeKind.Exclusive)))
                .Raises(() => warehouse.ProductRemoved += null, "Camera", 2);

            string productName = string.Empty;
            int quantity = 0;
            warehouse.ProductRemoved += (p, q) =>
            {
                productName = p;
                quantity = q;
            };

            // act
            warehouse.Remove(Arg.AnyString, Arg.AnyInt);

            // assert
            Assert.AreEqual("Camera", productName);
            Assert.AreEqual(2, quantity);
        }
    }

    public class Order
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }

        public Order(string productName, int quantity)
        {
            this.ProductName = productName;
            this.Quantity = quantity;
        }

        public void Fill(IWarehouse warehouse)
        {
            if (warehouse.HasInventory(this.ProductName, this.Quantity))
            {
                warehouse.Remove(this.ProductName, this.Quantity);
            }
        }
    }

    public delegate void ProductRemoveEventHandler(string productName, int quantity);

    public interface IWarehouse
    {
        bool HasInventory(string productName, int quantity);
        void Remove(string productName, int quantity);
        string Manager { get; set; }
        event ProductRemoveEventHandler ProductRemoved;
    }
}
