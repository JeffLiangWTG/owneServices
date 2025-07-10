using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(BCDConsignee))]
	sealed class BCDConsigneeTest : TestCaseWithFactory
	{
		public void TestConsigneeData()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.MasterBill;
			bill.ABL_ConsigneeRegNo = "96944490";
			bill.ABL_ConsigneeName = "IMPORTER NAME TEST";
			bill.ABL_ConsigneeLocalName = "收貨人";
			bill.ABL_LocationInformation = "info test.";
			bill.ABL_ConsigneeStreet1 = "stree 1";
			bill.ABL_ConsigneeStreet2 = "stree 2";
			bill.ABL_ConsigneeCity = "Taipei";
			bill.ABL_ConsigneeState = "Taiwan";
			bill.ABL_ConsigneePostcode = "105";
			bill.ABL_RN_NKConsigneeCountry = "TW";
			bill.ABL_ConsigneeLocalState = "台灣";
			bill.ABL_ConsigneeLocalCity = "台北市";
			bill.ABL_ConsigneeLocalStreet1 = "街道1";
			bill.ABL_ConsigneeLocalStreet2 = "街道2";
			bill.ABL_ConsigneeRegNoType = "VAT";
			CombineAssertions(() =>
			{
				var consignee = new BCDConsignee(bill);
				var consigneeData = (IPartyDetails)consignee;
				AssertEquals("ID", "96944490", consigneeData.ID);
				AssertEquals("Name", "IMPORTER NAME TEST", consigneeData.Name);
				AssertEquals("ChineseName", "收貨人", consigneeData.ChineseName);
				AssertEquals("CustomsControlID", ZString.Empty, consigneeData.CustomsControlID);
				AssertEquals("TypeCode VAT", "58", consigneeData.TypeCode);

				bill.ABL_ConsigneeRegNoType = "PAS";
				AssertEquals("TypeCode PAS", "53", consigneeData.TypeCode);

				bill.ABL_ConsigneeRegNoType = "PID";
				AssertEquals("TypeCode PID", "174", consigneeData.TypeCode);

				var addressData = (IAddress)consignee;
				AssertEquals("Address Line", "stree 1 stree 2 Taipei 105 Taiwan", addressData.Line);
				AssertEquals("Address ChineseLine", "105台灣台北市街道1街道2", addressData.ChineseLine);
				AssertEquals("Address CountryCode", "TW", addressData.CountryCode);

				bill.ABL_RN_NKConsigneeCountry = "US";
				AssertEquals("US: Address Line", "stree 1 stree 2 Taipei Taiwan 105 United States", addressData.Line);
				AssertEquals("US: Address ChineseLine", "105美國台灣台北市街道1街道2", addressData.ChineseLine);
			});
		}
	}
}
