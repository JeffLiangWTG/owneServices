using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class BookingContainerBuilderTest : TestCaseWithFactory
	{
		public void TestRenameDemurrageAndStorage()
		{
			var factory = new BusinessObjectFactory();
			var context = new CommonContext(factory);

			var bookingNumber1 = "SL001";
			var container1 = CreateContainer(context, "AAA");
			var packline1 = CreatePackingLine("AAA packline 1", bookingNumber1);
			container1.PackingLines = new[] { packline1 };

			var container = new BookingContainerBuilder(context, container1, bookingNumber1, container1.PK).Build();

			AssertEquals("DepartureTruckWaitCost", 100m, container.DepartureTruckWaitCost);
			AssertEquals("DepartureTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 1), container.DepartureTruckWaitTime);
		}

		public void TestPopulateContainerGenset()
		{
			var factory = new BusinessObjectFactory();
			var context = new CommonContext(factory);
			var container = CreateContainer(context, "AAA");
			container.PackingLines = System.Array.Empty<PackingLine>();
			container.HasControlledAtmosphere = true;
			container.Genset = false;
			var bookingContainer = new BookingContainerBuilder(context, container, "SL001", container.PK).Build();

			Assert(!bookingContainer.Genset);

			container.Genset = true;
			bookingContainer = new BookingContainerBuilder(context, container, "SL001", container.PK).Build();

			Assert(bookingContainer.Genset);

			container.HasControlledAtmosphere = false;
			bookingContainer = new BookingContainerBuilder(context, container, "SL001", container.PK).Build();

			Assert(!bookingContainer.Genset);
		}

		#region Implementation

		Container CreateContainer(IContext context, string containerNumber)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.Type = new ContainerType(context.ContainerTypes)
			{
				Code = "20FR",
				Type = new CodeDescription(context.ContainerTypes as IFindBoxListProvider)
				{
					Code = "RFG",
					Description = "Refrigerated"
				}
			};

			container.DepartureTruckWaitCost = 100m;
			container.DepartureTruckWaitTime = new ZDateTime(2019, 11, 1);

			return container;
		}

		PackingLine CreatePackingLine(string goodsDescription, string exportRefNumber)
		{
			var packingLine = new PackingLine(ZGuid.NewZGuid(), Factory);

			packingLine.Quantity = 3;
			packingLine.PackageType = new DummyCodeDescription
			{
				Code = "PLT",
				Description = "Pallet"
			};
			packingLine.Weight = new Measurement()
			{
				Value = 88,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			packingLine.Volume = new Measurement()
			{
				Value = 55,
				Unit = new DummyCodeDescription
				{
					Code = "M3",
				}
			};

			packingLine.GoodsDescription = goodsDescription;
			packingLine.ExportReferenceNumber = exportRefNumber;

			return packingLine;
		}

		#endregion
	}
}
