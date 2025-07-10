using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocZADA306))]
	public class DocZADA306Test : DocumentWrapperTestCase
	{
		public void TestToString()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Unique123";
			var wrapper = DocZADA306.New(shipment, Factory);
			AssertEquals("Unique123", wrapper.ToString());
		}

		public void TestTransportDocumentNumber()
		{
			using (Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV, new Moq.Mock<IContactType>().Object))
			{
				var shipment = Factory.New<ForwardingShipment>();

				var wrapper = DocZADA306.New(shipment, Factory);
				AssertEquals(string.Empty, wrapper.TransportDocumentNumber);

				shipment.JS_RL_NKOrigin = "AUMEL";
				shipment.JS_RL_NKDestination = "ZAJNB";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "AUMEL";
				departureConsol.JK_RL_NKDischargePort = "NZAKL";
				departureConsol.JK_MasterBillNum = "1";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "NZAKL";
				arrivalConsol.JK_RL_NKDischargePort = "ZAJNB";
				arrivalConsol.JK_MasterBillNum = "2";

				AssertEquals("2", wrapper.TransportDocumentNumber);
			}
		}

		public void TestFlightNo()
		{
			using (Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV, new Moq.Mock<IContactType>().Object))
			{
				var shipment = Factory.New<ForwardingShipment>();

				var wrapper = DocZADA306.New(shipment, Factory);
				AssertEquals(string.Empty, wrapper.FlightNumber);

				shipment.JS_RL_NKOrigin = "AUMEL";
				shipment.JS_RL_NKDestination = "ZAJNB";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "AUMEL";
				departureConsol.JK_RL_NKDischargePort = "NZAKL";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "NZAKL";
				arrivalConsol.JK_RL_NKDischargePort = "ZAJNB";

				var transport = arrivalConsol.Transports[0];
				transport.JW_VoyageFlight = "123";

				AssertEquals("123", wrapper.FlightNumber);
			}
		}

		public void TestFlightDate()
		{
			using (Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV, new Moq.Mock<IContactType>().Object))
			{
				var shipment = Factory.New<ForwardingShipment>();

				var wrapper = DocZADA306.New(shipment, Factory);
				AssertEquals(ZDateTime.Empty, wrapper.DateOfFlight);

				shipment.JS_RL_NKOrigin = "AUMEL";
				shipment.JS_RL_NKDestination = "ZAJNB";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "AUMEL";
				departureConsol.JK_RL_NKDischargePort = "NZAKL";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "NZAKL";
				arrivalConsol.JK_RL_NKDischargePort = "ZAJNB";

				var transport = arrivalConsol.Transports[0];

				transport.JW_ATA = new ZDateTime("1-OCT-2007");

				AssertEquals(new ZDateTime("1-OCT-2007"), wrapper.DateOfFlight);
				transport.JW_ATA = ZDateTime.Empty;
				transport.JW_ETA = new ZDateTime("2-OCT-2007");

				AssertEquals(new ZDateTime("2-OCT-2007"), wrapper.DateOfFlight);
			}
		}

		public void TestTotalNumberOfPackages()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			header.Consignments.AddNew().Items.AddNew();

			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew();
			consignment.Items.AddNew();

			var wrapper = DocZADA306.New(shipment, Factory);
			AssertEquals(3, wrapper.TotalNumberOfPackages);
		}

		public void TestTotalCustomsValueInZAR()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			consignment.HVC_RX_NKGoodsValueCurrency = "ZAR";
			var item1 = consignment.Items.AddNew();
			var line1 = item1.Lines.AddNew();
			line1.HVS_CustomsValue = 10;
			var line2 = item1.Lines.AddNew();
			line2.HVS_CustomsValue = 10;
			var item2 = consignment.Items.AddNew();
			var line3 = item2.Lines.AddNew();
			line3.HVS_CustomsValue = 10;

			var wrapper = DocZADA306.New(shipment, Factory);
			AssertEquals(30m, wrapper.TotalCustomsValueInZAR);
		}

		public void TestMarksAndNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var wrapper = DocZADA306.New(shipment, Factory);
			AssertEquals(string.Empty, wrapper.MarksNumbers);

			shipment.JS_MarksAndNumbers = "Marks and Numbers";
			AssertEquals("Marks and Numbers", wrapper.MarksNumbers);
		}

		public void TestDescriptionOfGoods()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var wrapper = DocZADA306.New(shipment, Factory);
			AssertEquals(string.Empty, wrapper.DescriptionOfGoods);

			shipment.JS_GoodsDescription = "Description";
			AssertEquals("Description", wrapper.DescriptionOfGoods);
		}

		public void TestPlaceOfEntry()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var wrapper = DocZADA306.New(shipment, Factory);
			AssertEquals(string.Empty, wrapper.PlaceOfEntry);

			shipment.JS_RL_NKDestination = "ZAJNB";
			AssertEquals("JSA", wrapper.PlaceOfEntry);

			shipment.JS_RL_NKDestination = "ZACPT";
			AssertEquals("DFM", wrapper.PlaceOfEntry);

			shipment.JS_RL_NKDestination = "ZAPPP";
			AssertEquals("Other destination not specified", string.Empty, wrapper.PlaceOfEntry);
		}

		public void TestContainerNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var item1 = header.Consignments.AddNew().Items.AddNew();
			item1.HVI_ContainerNumber = "A1";

			var consignment = header.Consignments.AddNew();
			var item2 = consignment.Items.AddNew();
			item2.HVI_ContainerNumber = "A1";
			var item3 = consignment.Items.AddNew();
			item3.HVI_ContainerNumber = "B2";

			var wrapper = DocZADA306.New(shipment, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { "A1", "B2" }, wrapper.ContainerNumbers.Split(","));
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new DocumentWrapper[]
			{
				DocZADA306.New(shipment, Factory)
			};
		}
	}
}
