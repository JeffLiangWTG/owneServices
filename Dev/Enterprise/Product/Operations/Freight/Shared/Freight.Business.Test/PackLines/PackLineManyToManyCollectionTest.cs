using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PackLineManyToManyCollection))]
	sealed class PackLineManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			return container.PackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CommonShipment>().OuterPackLines.AddNew();
		}

		public void TestPackLineManyToManyCollection()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();

			CommonContainer parentContainer = consol.Containers.AddNew();
			PackLineManyToManyCollection collection = new PackLineManyToManyCollection(parentContainer);

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			collection.Add(packLine1);

			JobContainerPackPivot[] pivots = (JobContainerPackPivot[])Factory.Load(typeof(JobContainerPackPivot), new ZQuery(JobContainerPackPivotSchema.J6_JL, packLine1.PK));
			AssertEquals("Count", 1, pivots.Length);
			AssertEquals("J6_JC", parentContainer.PK, pivots[0].J6_JC);
			AssertEquals("J6_JL", packLine1.PK, pivots[0].J6_JL);

			var retrievedContainer = Factory.Load<CommonContainer>(parentContainer.PK);
			PackLineManyToManyCollection retrievedCollection = new PackLineManyToManyCollection(retrievedContainer);
			retrievedCollection.Load();
			AssertEquals("Count", 1, retrievedCollection.Count);
			AssertEquals("Container", packLine1, retrievedCollection[0]);
		}

		public void TestFilterByShipment()
		{
			CommonShipment shipment1 = CommonShipment.New(Factory);
			PackLine packLine1_1 = shipment1.OuterPackLines.AddNew();
			PackLine packLine1_2 = shipment1.OuterPackLines.AddNew();

			CommonShipment shipment2 = CommonShipment.New(Factory);
			PackLine packLine2 = shipment2.OuterPackLines.AddNew();

			CommonContainer parentContainer = Factory.New<CommonContainer>();
			PackLineManyToManyCollection collection = new PackLineManyToManyCollection(parentContainer);
			AssertEquals("Empty Collection", 0, collection.FilterByShipment(shipment1).Count);

			collection.Add(packLine1_1);
			collection.Add(packLine1_2);
			collection.Add(packLine2);

			AssertEquals("Filter by Shipment2 only has 1 packline.", 1, collection.FilterByShipment(shipment2).Count);
			AssertEquals("Test filtered collection has correct packline", true, collection.FilterByShipment(shipment2).Contains(packLine2.PK));

			AssertEquals("Change filter.", 2, collection.FilterByShipment(shipment1).Count);
		}

		public void TestTypedFind()
		{
			CommonContainer parentContainer = Factory.New<CommonContainer>();
			PackLineManyToManyCollection collection = new PackLineManyToManyCollection(parentContainer);
			PackLine[] collectionResult = collection.Find(new ZQuery());
			AssertNotNull(collectionResult);
		}

		[ExpectNoExceptions]
		public void TestHandleContainerAllocationAndHandleContainerDeallocation()
		{
			var handlerMock = new Mock<IContainerPenaltyCalculateHandler>();

			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container = Factory.New<DummyContainer>();
			container.Handler = handlerMock.Object;
			var collection = new PackLineManyToManyCollection(container);

			var packLine = shipment.OuterPackLines.AddNew();
			collection.Add(packLine);

			handlerMock.Verify(x => x.HandleContainerAllocation(packLine), Moq.Times.Once);
			handlerMock.Verify(x => x.HandleContainerDeallocation(packLine), Moq.Times.Never);

			collection.Remove(packLine);
			handlerMock.Verify(x => x.HandleContainerAllocation(packLine), Moq.Times.Once);
			handlerMock.Verify(x => x.HandleContainerDeallocation(packLine), Moq.Times.Exactly(2));
		}

		class DummyContainer : CommonContainer
		{
			public DummyContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override IContainerPenaltyCalculateHandler[] ContainerPenaltyCalculateHandlers => new IContainerPenaltyCalculateHandler[] { Handler };

			public IContainerPenaltyCalculateHandler Handler { get; set; }
		}
	}
}
