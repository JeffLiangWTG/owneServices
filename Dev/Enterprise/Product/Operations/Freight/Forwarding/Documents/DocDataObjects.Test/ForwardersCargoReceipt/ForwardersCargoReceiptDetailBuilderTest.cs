using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ForwardersCargoReceiptDetailBuilderTest : ForwardersCargoReceiptBuilderTest
	{
		protected override ForwardersCargoReceipt Build(ForwardingShipment shipment)
		{
			return new ForwardersCargoReceiptSummaryBuilder(shipment, Parameters).Build();
		}

		protected override IDocDataObjectParameters Parameters
		{
			get
			{
				var mock = new Moq.Mock<IDocDataObjectParameters>();
				mock.SetupGet(m => m.DataStoreName).Returns("Forwarders Cargo Receipt");
				mock.SetupGet(m => m.DocumentTitle).Returns("Forwarders Cargo Receipt Detail");
				return mock.Object;
			}
		}

		public void TestContainerPackingInfo_BasicMapping()
		{
			var shipment = CreateShipment();
			var builder = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters);
			var fcr = builder.Build();

			AssertEquals(3, fcr.ContainerPackingInfos.Count);

			var info1 = fcr.ContainerPackingInfos.ElementAt(0);
			AssertEquals(info1.ContainerNumber, "CONT0001");

			var line1 = info1.PackedOrderLines.ElementAt(0);
			AssertEquals("Line 1 Description", line1.GoodsDescription);
			AssertEquals("Line 1 Marks and No", line1.MarksAndNos);
			AssertEquals("ORD0001-1", line1.OrderLineNumber);
			AssertEquals(5, line1.PackageCount);
			AssertEquals("PLT", line1.PackageType.Code);

			AssertEquals((ZDecimal)10, line1.PackedQuantity);
			AssertEquals("PCE", line1.UnitOfQuantity.Code);

			AssertEquals((ZDecimal)60, line1.GrossWeight.Value);
			AssertEquals("KG", line1.GrossWeight.Unit.Code);

			AssertEquals((ZDecimal)1.274258, line1.GrossVolume.Value);
			AssertEquals("M3", line1.GrossVolume.Unit.Code);

			var info2 = fcr.ContainerPackingInfos.ElementAt(1);
			AssertEquals(info2.ContainerNumber, "2 x 40GP");

			var info3 = fcr.ContainerPackingInfos.ElementAt(2);
			AssertEquals(info3.ContainerNumber, "");

			AssertEquals(1, fcr.OrderLineCount);
			AssertEquals(1, fcr.ItemNumberCount);
			AssertEquals((ZDecimal)10, fcr.PackedQuantity);
			AssertEquals("UNT", fcr.UnitOfQuantity.Code);
			AssertEquals((ZDecimal)1.274258, fcr.GrossVolume.Value);
			AssertEquals("M3", fcr.GrossVolume.Unit.Code);
			AssertEquals((ZDecimal)60, fcr.GrossWeight.Value);
			AssertEquals("KG", fcr.GrossWeight.Unit.Code);
		}

		public void TestOrderOfContainer()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consol = shipment.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT0001";
			container1.JC_ContainerCount = 1;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT0002";
			container2.JC_ContainerCount = 2;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "40GP").PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 3;
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "40HC").PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.Containers.Add(container1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.Containers.Add(container2);

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.Containers.Add(container3);

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.Containers.Add(container3);
			packline4.Containers.RemoveAll();

			var fcr = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertEquals(4, fcr.ContainerPackingInfos.Count);
			AssertEquals("CONT0001", fcr.ContainerPackingInfos.ElementAt(0).ContainerNumber);
			AssertEquals("CONT0002", fcr.ContainerPackingInfos.ElementAt(1).ContainerNumber);
			AssertEquals("3 x 40HC", fcr.ContainerPackingInfos.ElementAt(2).ContainerNumber);
			AssertEquals(ZString.Empty, fcr.ContainerPackingInfos.ElementAt(3).ContainerNumber);
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packProduct = Factory.New<PackProduct>();
			packProduct.D2_ProductCode = "111333";
			var consol = shipment.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT0001";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "40GP").PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 2;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "40GP").PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.Containers.Add(container1);
			packline1.JL_Description = "Line 1 Description";
			packline1.JL_MarksAndNumbers = "Line 1 Marks and No";
			packline1.JL_PackageCount = 5;
			packline1.JL_F3_NKPackType = "PLT";

			packline1.JL_ActualWeight = 60;
			packline1.JL_ActualWeightUQ = "KG";

			packline1.JL_ActualVolume = 45;
			packline1.JL_ActualVolumeUQ = "CF";
			packline1.Products.Add(packProduct);

			var order = Factory.New<Order>();
			order.JD_OrderNumber = "ORD0001";

			var orderLine = Factory.New<OrderLine>();
			orderLine.JO_LineNo = 1;
			orderLine.JO_JD = order.PK;
			orderLine.JO_F3_NKPackType = "PCE";

			var bookingLine = Factory.New<JobSupplierBookingLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			var loadList = Factory.New<CommonContainerLoadList>();
			var loadListLine = loadList.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_JL_PackLine = packline1.PK;
			loadListLine.CLL_PackedQuantity = 10;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.Containers.Add(container2);

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.Containers.RemoveAll();

			return shipment;
		}
	}
}
