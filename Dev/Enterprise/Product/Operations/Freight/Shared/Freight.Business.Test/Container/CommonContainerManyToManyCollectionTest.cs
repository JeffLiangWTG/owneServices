using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonContainerManyToManyCollection))]
	sealed class CommonContainerManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			return new CommonContainerManyToManyCollection(shipment.OuterPackLines.AddNew());
		}

		public void TestRegisterConcurrencyCheck()
		{
			var shipment = Factory.New<CommonShipment>();

			AssertEquals("concurrency check has not yet been registered", false, ContainerPackLineConcurrencyCheck.IsRegistered(Factory));

			var collection = new CommonContainerManyToManyCollection(shipment.OuterPackLines.AddNew());
			collection.Load();

			AssertEquals("concurrency check has been registered", true, ContainerPackLineConcurrencyCheck.IsRegistered(Factory));
		}

		public void TestCollection()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			PackLine line = shipment.OuterPackLines.AddNew();
			line.JL_JS = shipment.PK;

			CommonContainerManyToManyCollection collection = new CommonContainerManyToManyCollection(line);
			AssertSame("ParentPackLine", line, collection.ParentPackLine);

			CommonContainer testContainer1 = Factory.New<CommonContainer>();
			collection.Add(testContainer1);

			AssertEquals("Container 0", testContainer1, collection[0]);

			Factory.Save();

			JobContainerPackPivot[] pivots = (JobContainerPackPivot[])Factory.Load(typeof(JobContainerPackPivot), new ZQuery(JobContainerPackPivotSchema.J6_JL, line.PK));
			AssertEquals("Count", 1, pivots.Length);
			AssertEquals("J6_JC", testContainer1.PK, pivots[0].J6_JC);
			AssertEquals("J6_JL", line.PK, pivots[0].J6_JL);

			CommonContainerManyToManyCollection retrievedCollection = new CommonContainerManyToManyCollection(line);
			retrievedCollection.Load();
			AssertEquals("Count", 1, retrievedCollection.Count);
			AssertEquals("Container", testContainer1, retrievedCollection[0]);
		}

		public void TestAddAndRemove()
		{
			var consolA = Factory.New<CommonConsol>();

			var containerA1 = consolA.Containers.AddNew();
			var containerA2 = consolA.Containers.AddNew();

			var consolB = Factory.New<CommonConsol>();

			var containerB1 = consolB.Containers.AddNew();
			var containerB2 = consolB.Containers.AddNew();

			var shipment = consolA.Shipments.AddNew();
			consolB.Shipments.Add(shipment);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.Containers.Add(containerA1);
			packLine.Containers.Add(containerB1);

			AssertContainsExactElementsInAnyOrder("Expecting container 1 from consol A to contain line.", new[] { packLine }, containerA1.PackLines);
			AssertContainsExactElementsInAnyOrder("Expecting container 1 from consol B to contain line.", new[] { packLine }, containerB1.PackLines);
			AssertContainsExactElementsInAnyOrder("Expecting line to contain container 1 from consol A and container 2 from consol B.", new[] { containerA1, containerB1 }, packLine.Containers);

			packLine.Containers.Remove(containerA1);

			AssertContainsExactElementsInAnyOrder("Not expecting container 1 from consol A to contain line.", System.Array.Empty<PackLine>(), containerA1.PackLines);
			AssertContainsExactElementsInAnyOrder("Expecting container 1 from consol B to contain line.", new[] { packLine }, containerB1.PackLines);
			AssertContainsExactElementsInAnyOrder("Expecting line to contain container 1 from consol B.", new[] { containerB1 }, packLine.Containers);
		}

		public void TestAdd_PackLineDeleted_NoExceptionInErrorReporter()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			packLine.Containers.Add(container1);

			packLine.Delete();
			packLine.Containers.Add(container2);

			AssertEquals("No exception is reported to ErrorReporter.", 0, ErrorReporter.LastExceptionsReported().Count);
		}

		public void TestAddSameContainerTwice()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.Containers.Add(container);
			packLine.Containers.Add(container);

			AssertContainsExactElementsInAnyOrder("collection does not store duplicte continers", new[] { container }, packLine.Containers);
		}

		public void TestUpdatingPackLinesCollectionOnAdd()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();

			using (var callCounter = ContainerPackLineRelationshipHelper.GetNewCallCounter())
			{
				var packLine = shipment.OuterPackLines.AddNew();

				AssertContainsExactElementsInAnyOrder("prerequsite; pack line was packed in the container", new[] { packLine }, container.PackLines);
				AssertContainsExactElementsInAnyOrder("prerequsite; pack line was packed in the container", new[] { container }, packLine.Containers);

				AssertEquals("should only once notify helper that packline was added to container", 1, callCounter.GetTotalCalls("PackLineAddedToContainer"));
			}
		}

		public void TestUpdatingPackLinesCollectionOnRemove()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();

			var packLine = shipment.OuterPackLines.AddNew();

			AssertContainsExactElementsInAnyOrder("prerequsite; pack line was packed in the container", new[] { packLine }, container.PackLines);
			AssertContainsExactElementsInAnyOrder("prerequsite; pack line was packed in the container", new[] { container }, packLine.Containers);

			using (var callCounter = ContainerPackLineRelationshipHelper.GetNewCallCounter())
			{
				packLine.Containers.Remove(container);

				AssertEquals("should only once notify helper that packline was removed to container", 1, callCounter.GetTotalCalls("PackLineRemovedFromContainer"));
			}
		}

		public void TestAddRemovesFromContainerOnTheSameConsol()
		{
			var consolA = Factory.New<CommonConsol>();
			var consolB = Factory.New<CommonConsol>();

			var containerA1 = consolA.Containers.AddNew();
			var containerA2 = consolA.Containers.AddNew();

			var containerB1 = consolB.Containers.AddNew();
			var containerB2 = consolB.Containers.AddNew();

			var shipment = consolA.Shipments.AddNew();
			consolB.Shipments.Add(shipment);

			var packLine = shipment.OuterPackLines.AddNew();

			packLine.Containers.RemoveAll();

			AssertEquals("prerequiste: pack line contains no containers", 0, packLine.Containers.Count);

			packLine.Containers.Add(containerA1);

			AssertContainsExactElementsInAnyOrder("pack lines containers", new[] { containerA1 }, packLine.Containers);
			AssertContainsExactElementsInAnyOrder("pack lines in container A1", new[] { packLine }, containerA1.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container A2", System.Array.Empty<PackLine>(), containerA2.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container B1", System.Array.Empty<PackLine>(), containerB1.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container B2", System.Array.Empty<PackLine>(), containerB2.PackLines);

			packLine.Containers.Add(containerA2);

			AssertContainsExactElementsInAnyOrder("pack lines containers", new[] { containerA2 }, packLine.Containers);
			AssertContainsExactElementsInAnyOrder("pack lines in container A1", System.Array.Empty<PackLine>(), containerA1.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container A2", new[] { packLine }, containerA2.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container B1", System.Array.Empty<PackLine>(), containerB1.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container B2", System.Array.Empty<PackLine>(), containerB2.PackLines);

			packLine.Containers.Add(containerB1);

			AssertContainsExactElementsInAnyOrder("pack lines containers", new[] { containerA2, containerB1 }, packLine.Containers);
			AssertContainsExactElementsInAnyOrder("pack lines in container A1", System.Array.Empty<PackLine>(), containerA1.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container A2", new[] { packLine }, containerA2.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container B1", new[] { packLine }, containerB1.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container B2", System.Array.Empty<PackLine>(), containerB2.PackLines);

			packLine.Containers.Add(containerB2);

			AssertContainsExactElementsInAnyOrder("pack lines containers", new[] { containerA2, containerB2 }, packLine.Containers);
			AssertContainsExactElementsInAnyOrder("pack lines in container A1", System.Array.Empty<PackLine>(), containerA1.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container A2", new[] { packLine }, containerA2.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container B1", System.Array.Empty<PackLine>(), containerB1.PackLines);
			AssertContainsExactElementsInAnyOrder("pack lines in container B2", new[] { packLine }, containerB2.PackLines);
		}

		public void TestUpdateFCLAvailableDateOnAddRemove()
		{
			TestUpdateAvailableStorageDatesOnAddRemove(JobContainerSchema.JC_FCLAvailable, JobDocsAndCartageSchema.JP_FCLAvailable);
		}

		public void TestUpdateFCLStorageDateOnAddRemove()
		{
			TestUpdateAvailableStorageDatesOnAddRemove(JobContainerSchema.JC_ArrivalCTOStorageStartDate, JobDocsAndCartageSchema.JP_FCLStorageCommences);
		}

		public void TestUpdateLCLAvailableDateOnAddRemove()
		{
			TestUpdateAvailableStorageDatesOnAddRemove(JobContainerSchema.JC_LCLAvailable, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		public void TestUpdateLCLStorageDateOnAddRemove()
		{
			TestUpdateAvailableStorageDatesOnAddRemove(JobContainerSchema.JC_LCLStorageCommences, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestUpdateAvailableStorageDatesOnAddRemove(SchemaDateTimeColumn containerDateProperty, SchemaDateTimeColumn docsAndCartageProperty)
		{
			Container[containerDateProperty] = new ZDateTime(2000, 2, 2);
			Container2[containerDateProperty] = new ZDateTime(2000, 1, 1);

			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(Consol, Container);
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(Consol, Container2);

			AssertEquals(docsAndCartageProperty.Name + " initially", new ZDateTime(2000, 1, 1), Shipment.DocsAndCartage[docsAndCartageProperty]);
			packLine2.Containers.Remove(Container2);
			AssertEquals(docsAndCartageProperty.Name + " date after removing pack line", new ZDateTime(2000, 2, 2), Shipment.DocsAndCartage[docsAndCartageProperty]);
			packLine2.Containers.Add(Container2);
			AssertEquals(docsAndCartageProperty.Name + " after adding pack line", new ZDateTime(2000, 1, 1), Shipment.DocsAndCartage[docsAndCartageProperty]);

			packLine2.Containers.Remove(Container2);
			Factory.Save();
			CommonShipment loadedShipment = (CommonShipment)new BusinessObjectFactory().Load(Shipment.GetType(), Shipment.PK);
			AssertEquals("After saving and re-loading", new ZDateTime(2000, 2, 2), loadedShipment.DocsAndCartage[docsAndCartageProperty]);
		}

		[ExpectNoExceptions]
		public void TestHandleContainerAllocationAndHandleContainerDeallocation()
		{
			var handlerMock = new Mock<IContainerPenaltyCalculateHandler>();

			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container = Factory.New<DummyContainer>();
			container.Handler = handlerMock.Object;
			consol.Containers.Add(container);

			var packLine = shipment.OuterPackLines.AddNew();

			handlerMock.Verify(x => x.HandleContainerAllocation(packLine), Times.Once);
			handlerMock.Verify(x => x.HandleContainerDeallocation(packLine), Times.Never);

			packLine.Containers.RemoveAll();
			handlerMock.Verify(x => x.HandleContainerAllocation(packLine), Times.Once);
			handlerMock.Verify(x => x.HandleContainerDeallocation(packLine), Times.Once);
		}

		class DummyContainer : CommonContainer
		{
			public DummyContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override IContainerPenaltyCalculateHandler[] ContainerPenaltyCalculateHandlers => new IContainerPenaltyCalculateHandler[] { Handler };

			public IContainerPenaltyCalculateHandler Handler { get; set; }
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AutoJobContainerPackPivot.Schema.TableName);
		}

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
				}
				return consol;
			}
		}
		CommonConsol consol;

		CommonShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Consol.Shipments.AddNew();
				}
				return shipment;
			}
		}
		CommonShipment shipment;

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Consol.Containers.AddNew();
				}
				return container;
			}
		}
		CommonContainer container;

		CommonContainer Container2
		{
			get
			{
				if (container2 == null)
				{
					container2 = Consol.Containers.AddNew();
				}
				return container2;
			}
		}
		CommonContainer container2;

		#endregion
	}
}
