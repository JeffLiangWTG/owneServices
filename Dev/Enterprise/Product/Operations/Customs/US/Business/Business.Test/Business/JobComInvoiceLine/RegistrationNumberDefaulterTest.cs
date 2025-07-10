using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RegistrationNumberDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultDDTCRegistrationNumber()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU });

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "DDTC Registration Number";
			org.MainAddress.OA_Address1 = "MAIN ADD 1";

			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.USACodeTypes.DDTCRegistrationNumber;
			customsCode.OK_CustomsRegNo = "G-1234";
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "12599675660";
			declaration.US_LicenseType = "C33";

			var header = declaration.Invoices.AddNew();
			header.JZ_OH_Supplier = org.PK;

			var invoice1 = header.InvoiceLines.AddNew();
			var invoice2 = header.InvoiceLines.AddNew();
			var invoice3 = header.InvoiceLines.AddNew();

			AssertEquals(header.US_LicenseType, declaration.US_LicenseType);
			AssertEquals(invoice1.US_LicenseType, declaration.US_LicenseType);
			AssertEquals(invoice2.US_LicenseType, declaration.US_LicenseType);
			AssertEquals(invoice3.US_LicenseType, declaration.US_LicenseType);

			invoice1.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertEquals(ZString.Empty, invoice1.US_DDTCRegistrationNo);
			invoice2.US_LicenseType = USAESLicenseCode.Codes.SAG;
			AssertEquals("G-1234", invoice2.US_DDTCRegistrationNo);
			invoice3.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertEquals(ZString.Empty, invoice3.US_DDTCRegistrationNo);

			invoice1.US_LicenseType = USAESLicenseCode.Codes.SAU;
			invoice2.US_LicenseType = USAESLicenseCode.Codes.SAU;
			AssertEquals("G-1234", invoice1.US_DDTCRegistrationNo);
			AssertEquals("G-1234", invoice2.US_DDTCRegistrationNo);
			AssertEquals(ZString.Empty, invoice3.US_DDTCRegistrationNo);
			AssertEquals(header.US_LicenseType, declaration.US_LicenseType);

			header.US_LicenseType = USAESLicenseCode.Codes.SAG;
			AssertEquals("G-1234", header.US_DDTCRegistrationNo);

			header.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasWarning(header.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceHeaderValidation.LicenseTypeSyncError);
		}

		public void TestDefaultDDTCRegistrationNumber_Import()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "DDTC Registration Number";
			org.MainAddress.OA_Address1 = "MAIN ADD 1";

			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.USACodeTypes.DDTCRegistrationNumber;
			customsCode.OK_CustomsRegNo = "G-1234";
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "12599675660";

			var header = declaration.Invoices.AddNew();
			header.JZ_OH_Buyer = org.PK;
			var invoice1 = header.InvoiceLines.AddNew();

			invoice1.US_DDTCInd = ZString.Empty;
			AssertEquals(ZString.Empty, invoice1.US_DDTCRegistrationNo);
			invoice1.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("G-1234", invoice1.US_DDTCRegistrationNo);
		}
	}
}
