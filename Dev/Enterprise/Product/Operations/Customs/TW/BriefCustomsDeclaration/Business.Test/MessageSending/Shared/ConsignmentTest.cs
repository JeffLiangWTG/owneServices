using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(Consignment))]
	sealed class ConsignmentTest : TestCaseWithFactory
	{
		public void TestConsignmentData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_ManifestNumber = "mn01";
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = "M01";
			masterBill.ABL_GoodsLocation = "TWTPE";
			masterBill.ABL_CarrierReference = "SPOD";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "H01";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "H02";
			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "H03";
			header.BagNumber = "B001";
			header.AMA_LloydsNumber = "9832343";
			header.AMA_Voyage = "V345";
			header.AMA_VehicleRegistration = "REG123";
			header.AMA_RadioCallSign = "W6RO";
			header.AMA_VesselName = "CS CALLA";
			IBCDConsignment consignment = new Consignment(header, header.MasterBill);
			CombineAssertions("Test For IMP, SEA", () =>
			{
				AssertEquals("AssociatedTransportDocumentId", "B001", consignment.AssociatedTransportDocumentId);
				AssertEquals("BoardedQuantity", 3m, consignment.BoardedQuantity);
				AssertEquals("TransportContractDocumentId", "M01", consignment.TransportContractDocumentId);
				AssertEquals("ManifestSerialNumber", "mn01", consignment.ManifestSerialNumber);
				AssertEquals("ShippingOrderNumber", "SPOD", consignment.ShippingOrderNumber);
				AssertEquals("GoodsLocation", "TWTPE", consignment.GoodsLocation);
				AssertEquals("BorderTransportMeans.ID", "9832343", consignment.BorderTransportMeans.ID);
				AssertEquals("BorderTransportMeans.JourneyID", "V345", consignment.BorderTransportMeans.JourneyID);
				AssertEquals("BorderTransportMeans.Registration", "REG123", consignment.BorderTransportMeans.Registration);
				AssertEquals("BorderTransportMeans.CallSignID", "W6RO", consignment.BorderTransportMeans.CallSignID);
				AssertEquals("DepartureTransportMeans.ID", "9832343", consignment.DepartureTransportMeans.ID);
				AssertEquals("DepartureTransportMeans.Name", "CS CALLA", consignment.DepartureTransportMeans.Name);
			});

			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			consignment = new Consignment(header, header.MasterBill);
			CombineAssertions("Test For IMP, AIR", () =>
			{
				AssertEquals("ManifestSerialNumber", ZString.Empty, consignment.ManifestSerialNumber);
				AssertEquals("BorderTransportMeans.Registration", ZString.Empty, consignment.BorderTransportMeans.Registration);
			});

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			consignment = new Consignment(header, header.MasterBill);
			CombineAssertions("Test For EXP, AIR", () =>
			{
				AssertEquals("ManifestSerialNumber", "mn01", consignment.ManifestSerialNumber);
				AssertEquals("BorderTransportMeans.Registration", ZString.Empty, consignment.BorderTransportMeans.Registration);
			});

			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			consignment = new Consignment(header, header.MasterBill);
			CombineAssertions("Test For EXP, SEA", () =>
			{
				AssertEquals("ManifestSerialNumber", "mn01", consignment.ManifestSerialNumber);
				AssertEquals("BorderTransportMeans.Registration", "REG123", consignment.BorderTransportMeans.Registration);
			});
		}
	}
}
