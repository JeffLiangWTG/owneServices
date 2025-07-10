using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	class JobDeclarationTransportSupporterTest : TestCaseWithFactory
	{
		public void TestImportDirectShipment()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

			var transport1 = AddTransportLeg("CNSHA", new ZDateTime(2024, 1, 1), "ZADUR", new ZDateTime(2024, 1, 31), TransportTypeList.Codes.Sea);
			AssertTransportDataInDeclaration(transport1, "CNSHA");
		}

		public void TestImportTransshipmentToNonBeln()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

			_ = AddTransportLeg("CNSHA", new ZDateTime(2024, 1, 1), "SGSIN", new ZDateTime(2024, 1, 10), TransportTypeList.Codes.Sea);
			AssertTransportDataInDeclaration(null, ZString.Empty);

			var transport2 = AddTransportLeg("SGSIN", new ZDateTime(2024, 1, 11), "ZADUR", new ZDateTime(2024, 1, 31), TransportTypeList.Codes.Sea);
			AssertTransportDataInDeclaration(transport2, "CNSHA");

			_ = AddTransportLeg("ZADUR", new ZDateTime(2024, 2, 1), "ZMLUN", new ZDateTime(2024, 2, 2), TransportTypeList.Codes.Road);
			AssertTransportDataInDeclaration(transport2, "CNSHA");
		}

		public void TestImportTransshipmentFromBeln()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

			var transport1 = AddTransportLeg("BWGBE", new ZDateTime(2024, 1, 1), "ZADUR", new ZDateTime(2024, 1, 5), TransportTypeList.Codes.Road);
			AssertTransportDataInDeclaration(transport1, "BWGBE");

			_ = AddTransportLeg("ZADUR", new ZDateTime(2024, 1, 6), "CNSHA", new ZDateTime(2024, 1, 31), TransportTypeList.Codes.Sea);
			AssertTransportDataInDeclaration(transport1, "BWGBE");
		}

		public void TestExportDirectShipment()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;

			var transport1 = AddTransportLeg("ZADUR", new ZDateTime(2024, 1, 1), "CNSHA", new ZDateTime(2024, 1, 31), TransportTypeList.Codes.Sea);
			AssertTransportDataInDeclaration(transport1, "ZADUR");
		}

		public void TestExportTransshipment()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;

			var transport1 = AddTransportLeg("ZADUR", new ZDateTime(2024, 1, 1), "SGSIN", new ZDateTime(2024, 1, 21), TransportTypeList.Codes.Sea);
			AssertTransportDataInDeclaration(transport1, "ZADUR");

			_ = AddTransportLeg("SGSIN", new ZDateTime(2024, 1, 22), "CNSHA", new ZDateTime(2024, 1, 31), TransportTypeList.Codes.Sea);
			AssertTransportDataInDeclaration(transport1, "ZADUR");
		}

		Transport AddTransportLeg(ZString loadPort, ZDateTime etd, ZString dischargePort, ZDateTime eta, ZString transportMode)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.FillWithValidTestData();

			var legOrder = declaration.Transports.Count + 1;
			var transport = declaration.Transports.AddNew();
			transport.JW_LegOrder = (ZByte)legOrder;
			if (declaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)
			{
				transport.JW_RL_NKDiscPort = dischargePort;
				transport.JW_RL_NKLoadPort = loadPort;
			}
			else
			{
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = dischargePort;
			}
			transport.JW_TransportMode = transportMode;
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			transport.JW_Vessel = (transportMode == TransportTypeList.Codes.Sea) ? $"VESSEL {legOrder}" : ZString.Empty;
			transport.JW_VoyageFlight = $"VOYAGE {legOrder}";
			transport.CarrierPK = carrier.PK;

			return transport;
		}

		void AssertTransportDataInDeclaration(Transport transport, ZString portOfOrigin)
		{
			AssertEquals(transport?.CarrierPK ?? ZGuid.Empty, declaration.JE_OH_ShippingLine);
			AssertEquals(transport?.JW_RL_NKDiscPort ?? ZString.Empty, declaration.JE_RL_NKPortOfArrival);
			AssertEquals(transport?.JW_ETA ?? ZDateTime.Empty, declaration.JE_DateOfArrival);
			AssertEquals(transport?.JW_RL_NKLoadPort ?? ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			AssertEquals(transport?.JW_ETD ?? ZDateTime.Empty, declaration.JE_ExportDate);
			AssertEquals(transport?.JW_TransportMode ?? ZString.Empty, declaration.JE_TransportMode);
			AssertEquals(transport?.JW_Vessel ?? ZString.Empty, declaration.JE_VesselName);
			AssertEquals(transport?.JW_VoyageFlight ?? ZString.Empty, declaration.JE_VoyageFlightNo);
			AssertEquals(portOfOrigin, declaration.JE_RL_NKOrigin);
		}

		JobDeclaration declaration;

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
		}
	}
}
