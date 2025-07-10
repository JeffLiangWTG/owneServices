using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingName))]
	public class SterlingNameTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingName();
		}

		public void TestOverriddenCountry()
		{
			var sterling = new SterlingName();
			var org = new Xsd.Organisation();
			var orgAddress = new Xsd.OrgAddress();
			var orgContact = Factory.New<OrgContact>();

			orgAddress.Location.Country = "NZ";
			sterling.SetName("TST", org, orgAddress, orgContact);

			AssertContains("|NZ|", sterling.Record);

			orgAddress.Location.Value = "USA";
			var newSterling = new SterlingName();
			newSterling.SetName("TST", org, orgAddress, orgContact);

			AssertNotContains("|NZ|", newSterling.Record);
			AssertContains("|US|", newSterling.Record);
		}

		public void TestSterlingNameRecord()
		{
			AssertEquals("There are 7 elements in this collection", 7, SterlingForTest.NameInfo.Count);

			var expectedShipFromRecord = "N01|SF|PickUpCompanyCODE|1|PickUpCompany|PickUpCompanyTest Address11|PickUpCompanyTest Address21|Los Angeles|PickUpCompanyST|PickUpCompany123321|US|MAIN||PickUpCompanyTelephoneNumber|PickUpCompanyemail@emailserver.com|PickUpCompany RegNo>\r\n";
			var expectedShipToRecord = "N01|ST|DeliveryCompanyCODE|1|DeliveryCompany|DeliveryCompanyTest Address11|DeliveryCompanyTest Address21|Sydney|DeliveryCompanyST|DeliveryCompany123321|AU|MAIN||DeliveryCompanyTelephoneNumber|DeliveryCompanyemail@emailserver.com|DeliveryCompany RegNo>\r\n";

			AssertEquals("Parsed record is different from expected", expectedShipFromRecord, SterlingForTest.NameInfo[0].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingNameRecord2, SterlingForTest.NameInfo[1].Record);
			AssertEquals("Parsed record is different from expected", expectedShipToRecord, SterlingForTest.NameInfo[2].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingNameRecord4, SterlingForTest.NameInfo[3].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingNameRecord5, SterlingForTest.NameInfo[4].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingNameRecord6, SterlingForTest.NameInfo[5].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingNameRecord7, SterlingForTest.NameInfo[6].Record);
		}

		const string ExpectedSterlingNameRecord2 = "N01|BT|LocalClientCODE|0|LocalClient|LocalClientTest Address10|LocalClientTest Address20|Brisbane|LocalClientST|LocalClient123321|AU|MAIN|The Accounts Payable Manager|LocalClientTelephoneNumber|LocalClientemail@emailserver.com|LocalClient RegNo>\r\n";
		const string ExpectedSterlingNameRecord4 = "N01|CN|CneeCODE|1|Cnee|CneeTest Address11|CneeTest Address21|Chicago|CneeST|Cnee123321|US|MAIN|ContactName|CneeTelephoneNumber|Cneeemail@emailserver.com|Cnee RegNo>\r\n";
		const string ExpectedSterlingNameRecord5 = "N01|SH|ConsignorCODE|1|Consignor|ConsignorTest Address11|ConsignorTest Address21|Moskva|ConsignorST|Consignor123321|RU|MAIN|The Export Manager|ConsignorTelephoneNumber|Consignoremail@emailserver.com|Consignor RegNo>\r\n";
		const string ExpectedSterlingNameRecord6 = "N01|NT|NotifyCODE|1|Notify|NotifyTest Address11|NotifyTest Address21|Singapore|NotifyST|Notify123321|SG|MAIN|All Documents|NotifyTelephoneNumber|Notifyemail@emailserver.com|Notify RegNo>\r\n";
		const string ExpectedSterlingNameRecord7 = "N01|CR|CarrierCODE|1|Carrier|CarrierTest Address11|CarrierTest Address21|Los Angeles|CarrierST|Carrier123321|US|MAIN|All Documents|CarrierTelephoneNumber|Carrieremail@emailserver.com|Carrier RegNo>\r\n";
	}
}
