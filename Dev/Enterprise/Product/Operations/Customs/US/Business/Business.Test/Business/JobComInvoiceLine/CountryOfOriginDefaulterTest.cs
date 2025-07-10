using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CountryOfOriginDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultFromMID()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_RL_NKClosestPort = "CABRJ";
			var midAddress = manufacturer.Addresses.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();

			invoice.JZ_OA_ManufacturerAddress = midAddress.PK;
			AssertEquals("Should not be defaulted with 'CA'", "", invoice.US_UC_NKCountryOfOrigin);

			OrgCusCode cusCode = midAddress.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			cusCode.OK_CustomsRegNo = "XA12345678";
			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			invoice.JZ_OA_ManufacturerAddress = midAddress.PK;
			AssertEquals("Should not be defaulted with 'CA'", "XA", invoice.US_UC_NKCountryOfOrigin);

			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("Should not be defaulted with 'CA'", "XA", invoice.US_UC_NKCountryOfOrigin);

			midAddress = manufacturer.Addresses.AddNew();
			cusCode = midAddress.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			cusCode.OK_CustomsRegNo = "XO12345678";
			invoiceLine.JI_OA_ManufacturerAddress = midAddress.PK;
			AssertEquals("Should not be defaulted with 'CA'", "XO", invoiceLine.US_UC_NKCountryOfOrigin);
		}

		public void TestDefaultFromOrgMiscServ()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_RL_NKClosestPort = "MMAKY";
			manufacturer.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = "MM";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();

			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals(Core.Constants.CountryCodes.Myanmar, invoice.US_UC_NKCountryOfOrigin);

			var manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.OH_RL_NKClosestPort = "BUAKY";
			manufacturer2.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = USCCountry.Burma;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			AssertEquals(USCCountry.Burma, invoice2.US_UC_NKCountryOfOrigin);
		}
	}
}
