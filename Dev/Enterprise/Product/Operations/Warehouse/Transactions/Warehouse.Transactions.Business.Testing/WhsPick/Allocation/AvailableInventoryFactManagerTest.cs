using System;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class AvailableInventoryFactManagerTest : WhsTestCaseWithFactory
	{
		public void TestRegister_NullArguments_Throws()
		{
			var manager = new AvailableInventoryFactManager();
			AssertExceptionThrown<ArgumentNullException>(() =>
				manager.Register(null, Mock.Of<IAvailableInventoryFact>()));
			AssertExceptionThrown<ArgumentNullException>(() => manager.Register("TEST", null));
		}

		public void TestUpdate_NullArgument_Throws()
		{
			var manager = new AvailableInventoryFactManager();
			AssertExceptionThrown<ArgumentNullException>(() => manager.Update(null, Guid.NewGuid(), 1.0m));
		}

		public void TestUpdate_UnregisteredArgument()
		{
			var manager = new AvailableInventoryFactManager();
			AssertExceptionThrown<ArgumentException>(() => manager.Update("LOL", Guid.NewGuid(), 1.0m));
		}

		public void TestHasKeyRegistered()
		{
			var manager = new AvailableInventoryFactManager();
			AssertEquals("Should not be registered.", false, manager.HasKeyRegistered("ABC"));
			AssertEquals("Should not be registered.", false, manager.HasKeyRegistered(string.Empty));

			var pk1 = Guid.NewGuid();
			var line1 = new Mock<IAvailableInventoryFact>();
			line1.Setup(l => l.PK).Returns(pk1);
			manager.Register("ABC", line1.Object);
			AssertEquals("Should be registered.", true, manager.HasKeyRegistered("ABC"));
			AssertEquals("Should not be registered.", false, manager.HasKeyRegistered(string.Empty));
			AssertEquals("Should not be registered.", false, manager.HasKeyRegistered("DEF"));

			var pk2 = Guid.NewGuid();
			var line2 = new Mock<IAvailableInventoryFact>();
			line2.Setup(l => l.PK).Returns(pk2);
			line2.Setup(l => l.Quantity).Returns(2m);
			manager.Register("DEF", line2.Object);
			AssertEquals("Should be registered.", true, manager.HasKeyRegistered("DEF"));
		}

		public void TestUpdate()
		{
			var manager = new AvailableInventoryFactManager();

			var pk1 = Guid.NewGuid();
			var line1 = new Mock<IAvailableInventoryFact>();
			line1.Setup(l => l.PK).Returns(pk1);
			manager.Register("ABC", line1.Object);

			var pk2 = Guid.NewGuid();
			var line2 = new Mock<IAvailableInventoryFact>();
			line2.Setup(l => l.PK).Returns(pk2);
			line2.Setup(l => l.Quantity).Returns(2m);
			manager.Register("ABC", line2.Object);

			var pk3 = Guid.NewGuid();
			var line3 = new Mock<IAvailableInventoryFact>();
			line3.Setup(l => l.PK).Returns(pk3);
			line3.Setup(l => l.Quantity).Returns(3m);
			manager.Register("ABC", line3.Object);

			var pk4 = Guid.NewGuid();
			var line4 = new Mock<IAvailableInventoryFact>();
			line4.Setup(l => l.PK).Returns(pk4);
			manager.Register("DEF", line4.Object);

			var pk5 = Guid.NewGuid();
			var line5 = new Mock<IAvailableInventoryFact>();
			line5.Setup(l => l.PK).Returns(pk5);
			manager.Register("XYZ", line5.Object);

			manager.Update("ABC", pk1, 1.23m);
			line2.Verify(l => l.DecreaseQuantity(1.23m), Times.Once);
			line3.Verify(l => l.DecreaseQuantity(1.23m), Times.Once);
			line1.Verify(l => l.DecreaseQuantity(It.IsAny<decimal>()), Times.Never);
			line4.Verify(l => l.DecreaseQuantity(It.IsAny<decimal>()), Times.Never);
			line5.Verify(l => l.DecreaseQuantity(It.IsAny<decimal>()), Times.Never);
			Assert(true);
		}

		public void TestUpdate_SuspendsUpdating()
		{
			var manager = new AvailableInventoryFactManager();

			var pk1 = Guid.NewGuid();
			var line1 = new Mock<IAvailableInventoryFact>();
			line1.Setup(l => l.PK).Returns(pk1);
			manager.Register("ABC", line1.Object);

			var pk2 = Guid.NewGuid();
			var line2 = new Mock<IAvailableInventoryFact>();
			line2.Setup(l => l.PK).Returns(pk2);
			line2.Setup(l => l.Quantity).Callback(() => manager.Update("ABC", pk2, -1.23m)).Returns(2m);
			manager.Register("ABC", line2.Object);

			manager.Update("ABC", pk1, 1.23m);
			line2.Verify(l => l.DecreaseQuantity(1.23m), Times.Once);
			line1.Verify(l => l.DecreaseQuantity(It.IsAny<decimal>()), Times.Never);
			Assert(true);
		}
	}
}
