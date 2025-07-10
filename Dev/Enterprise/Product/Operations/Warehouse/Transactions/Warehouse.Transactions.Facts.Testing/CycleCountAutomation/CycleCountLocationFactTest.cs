using System;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class CycleCountLocationFactTest
	{
		[Test]
		public void ImplementsInputFactWithUserDefinedProperties()
		{
			Assert.That(typeof(InputFactWithUserDefinedProperties).IsAssignableFrom(typeof(CycleCountLocationFact)));
		}

		[Test]
		public void Constructor_Throws()
		{
			var location = Mock.Of<ICycleCountLocationCoreFact>();
			var client = Mock.Of<IOrganisationFact>();
			var product = Mock.Of<ICycleCountProductFact>();
			Assert.Throws<ArgumentNullException>(() => new CycleCountLocationFact(null, client, product, 0m));
		}

		[Test]
		public void WrappedLocation()
		{
			var location = Mock.Of<ICycleCountLocationCoreFact>();
			var locationFact = GetCycleCountLocationFact(location: location);
			Assert.That(locationFact.WrappedLocation.Fact, Is.EqualTo(location));
		}

		[Test]
		public void Client()
		{
			var client = Mock.Of<IOrganisationFact>();
			var locationFact = GetCycleCountLocationFact(client: client);
			Assert.That(locationFact.Client.Fact, Is.EqualTo(client));
		}

		[Test]
		public void Client_Null()
		{
			var client = Mock.Of<IOrganisationFact>();
			var locationFact = GetCycleCountLocationFact();
			Assert.That(locationFact.Client.Fact, Is.Null);
		}

		[Test]
		public void Product()
		{
			var product = Mock.Of<ICycleCountProductFact>();
			var locationFact = GetCycleCountLocationFact(product: product);
			Assert.That(locationFact.Product.Fact, Is.EqualTo(product));
		}

		[Test]
		public void Product_Null()
		{
			var product = Mock.Of<ICycleCountProductFact>();
			var locationFact = GetCycleCountLocationFact();
			Assert.That(locationFact.Product.Fact, Is.Null);
		}

		[Test]
		public void StockOnHandForThisProduct()
		{
			var locationFact = GetCycleCountLocationFact(stockOnHandForThisProduct: 3.14m);
			Assert.That(locationFact.StockOnHandForThisProduct, Is.EqualTo(3.14m));
		}

		[Test]
		public void LocationPK()
		{
			var pk = Guid.NewGuid();

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.PK).Returns(pk);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			Assert.That(locationFact.LocationPK, Is.EqualTo(pk));

			location.Verify(l => l.PK, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void PK()
		{
			var pk = Guid.NewGuid();

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.PK).Returns(pk);

			var locationFact1 = GetCycleCountLocationFact(location: location.Object);
			var locationFact2 = GetCycleCountLocationFact(location: location.Object);
			Assert.That(locationFact1.PK, Is.Not.EqualTo(pk));
			Assert.That(locationFact2.PK, Is.Not.EqualTo(pk));
			Assert.That(locationFact1.PK, Is.Not.EqualTo(locationFact2.PK));

			location.VerifyNoOtherCalls();
		}

		[Test]
		public void LocationTypeCode()
		{
			const string typeCode = "RNO";

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.LocationTypeCode).Returns(typeCode);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			Assert.That(locationFact.LocationTypeCode, Is.EqualTo(typeCode));

			location.Verify(l => l.LocationTypeCode, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void LocationClass()
		{
			const string locationClass = "RNO";

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.LocationClass).Returns(locationClass);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.LocationClass, Is.EqualTo(locationClass));
			location.Verify(l => l.LocationClass, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void AreaName()
		{
			const string areaName = "A";

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.AreaName).Returns(areaName);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.AreaName, Is.EqualTo(areaName));
			location.Verify(l => l.AreaName, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void RowName()
		{
			const string rowName = "A";

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.RowName).Returns(rowName);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.RowName, Is.EqualTo(rowName));
			location.Verify(l => l.RowName, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void LocationStatus()
		{
			const string status = "NOR";

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.LocationStatus).Returns(status);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.LocationStatus, Is.EqualTo(status));
			location.Verify(l => l.LocationStatus, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void Column()
		{
			const int column = 1;

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.Column).Returns(column);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.Column, Is.EqualTo(column));
			location.Verify(l => l.Column, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void Level()
		{
			const int level = 1;

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.Level).Returns(level);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.Level, Is.EqualTo(level));
			location.Verify(l => l.Level, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void Tray()
		{
			const int tray = 1;

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.Tray).Returns(tray);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.Tray, Is.EqualTo(tray));
			location.Verify(l => l.Tray, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void CycleCountPathSequence()
		{
			const int sequence = 4;

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.CycleCountPathSequence).Returns(sequence);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.CycleCountPathSequence, Is.EqualTo(sequence));
			location.Verify(l => l.CycleCountPathSequence, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void RowPathSequence()
		{
			const int sequence = 2;

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.RowPathSequence).Returns(sequence);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.RowPathSequence, Is.EqualTo(sequence));
			location.Verify(l => l.RowPathSequence, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void PickMethod()
		{
			const string pickMethod = "CAR";

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.PickMethod).Returns(pickMethod);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.PickMethod, Is.EqualTo(pickMethod));
			location.Verify(l => l.PickMethod, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void LocationString()
		{
			const string loc = "A-A-4";

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.LocationString).Returns(loc);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.LocationString, Is.EqualTo(loc));
			location.Verify(l => l.LocationString, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void LocationStringSortIndex()
		{
			const int index = 3;

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.LocationStringSortIndex).Returns(index);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.LocationStringSortIndex, Is.EqualTo(index));
			location.Verify(l => l.LocationStringSortIndex, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void InventoryLastChangedDate()
		{
			var date = DateTime.Today.AddDays(-2);

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.InventoryLastChangedDate).Returns(date);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.InventoryLastChangedDate, Is.EqualTo(date));
			location.Verify(l => l.InventoryLastChangedDate, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void CycleCountLastPerformedDate()
		{
			var date = DateTime.Today.AddDays(-4);

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.CycleCountLastPerformedDate).Returns(date);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.CycleCountLastPerformedDate, Is.EqualTo(date));
			location.Verify(l => l.CycleCountLastPerformedDate, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[TestCase(false)]
		[TestCase(true)]
		public void CycleCountTaskExists_Getter(bool value)
		{
			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.CycleCountTaskExists).Returns(value);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.CycleCountTaskExists, Is.EqualTo(value));
			location.Verify(l => l.CycleCountTaskExists, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[TestCase(false)]
		[TestCase(true)]
		public void CycleCountTaskExists_Setter(bool value)
		{
			var location = new Mock<ICycleCountLocationCoreFact>();
			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			locationFact.CycleCountTaskExists = value;
			location.VerifySet(l => l.CycleCountTaskExists = value, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[Test]
		public void StockOnHand()
		{
			const decimal stockOnHand = 9.81m;

			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.StockOnHand).Returns(stockOnHand);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.StockOnHand, Is.EqualTo(stockOnHand));
			location.Verify(l => l.StockOnHand, Times.Once);
			location.VerifyNoOtherCalls();
		}

		[TestCase(false)]
		[TestCase(true)]
		public void HasCommittedStock_Getter(bool value)
		{
			var location = new Mock<ICycleCountLocationCoreFact>();
			location.Setup(l => l.HasCommittedStock).Returns(value);

			var locationFact = GetCycleCountLocationFact(location: location.Object);
			location.VerifyNoOtherCalls();

			Assert.That(locationFact.HasCommittedStock, Is.EqualTo(value));
			location.Verify(l => l.HasCommittedStock, Times.Once);
			location.VerifyNoOtherCalls();
		}

		static CycleCountLocationFact GetCycleCountLocationFact(
			ICycleCountLocationCoreFact location = null,
			IOrganisationFact client = null,
			ICycleCountProductFact product = null,
			decimal stockOnHandForThisProduct = 0m)
		{
			return
				new CycleCountLocationFact(
					location ?? Mock.Of<ICycleCountLocationCoreFact>(),
					client,
					product,
					stockOnHandForThisProduct);
		}
	}
}
