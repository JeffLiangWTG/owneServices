using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(BCDExporter))]
	sealed class BCDExporterTest : TestCaseWithFactory
	{
		public void TestExporterData()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OrgTest1";
			var mainAddress = orgHeader.MainAddress;
			mainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "123456", Core.Constants.CountryCodes.Taiwan);
	
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			masterBill.ABL_OA_Shipper = mainAddress.PK;
			masterBill.ABL_ShipperRegNo = "96944490";
			masterBill.ABL_ShipperName = "SHIPPER NAME TEST";
			masterBill.ABL_ShipperLocalName = "發貨人";
			masterBill.ABL_ShipperStreet1 = "stree 1";
			masterBill.ABL_ShipperStreet2 = "stree 2";
			masterBill.ABL_ShipperCity = "Taipei";
			masterBill.ABL_ShipperState = "Taiwan";
			masterBill.ABL_ShipperPostcode = "105";
			masterBill.ABL_RN_NKShipperCountry = "TW";
			masterBill.ABL_ShipperLocalState = "台灣";
			masterBill.ABL_ShipperLocalCity = "台北市";
			masterBill.ABL_ShipperLocalStreet1 = "街道1";
			masterBill.ABL_ShipperLocalStreet2 = "街道2";
			masterBill.ABL_ShipperRegNoType = "VAT";
			CombineAssertions(() =>
			{
				var exporter = new BCDExporter(masterBill);
				var exporterData = (IPartyDetails)exporter;
				AssertEquals("ID", "96944490", exporterData.ID);
				AssertEquals("Name", "SHIPPER NAME TEST", exporterData.Name);
				AssertEquals("ChineseName", "發貨人", exporterData.ChineseName);
				AssertEquals("CustomsControlID", "123456", exporterData.CustomsControlID);
				AssertEquals("TypeCode VAT", "58", exporterData.TypeCode);

				masterBill.ABL_ShipperRegNoType = "PAS";
				AssertEquals("TypeCode PAS", "53", exporterData.TypeCode);

				masterBill.ABL_ShipperRegNoType = "PID";
				AssertEquals("TypeCode PID", "174", exporterData.TypeCode);

				var addressData = (IAddress)exporter;
				AssertEquals("Address Line", "stree 1 stree 2 Taipei 105 Taiwan", addressData.Line);
				AssertEquals("Address ChineseLine", "105台灣台北市街道1街道2", addressData.ChineseLine);
			});
		}
	}
}
