using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class DeclarationJobDocAddressValidation : TestCaseWithFactory
	{
		public void TestCheckSupplierDocumentaryAddressMandatory_Import()
		{
			var supplierNonCSC = Factory.New<OrgHeader>();
			supplierNonCSC.OH_Code = "SuppNonCSC";
			supplierNonCSC.OH_FullName = "Supplier Non with CSC";
			var addressNonCSC = supplierNonCSC.MainAddress;
			addressNonCSC.OA_OH = supplierNonCSC.PK;
			addressNonCSC.CompanyName = "Supplier Non with CSC Address";
			addressNonCSC.Address1 = "xAdress1";
			addressNonCSC.Address2 = "xAdress2";
			addressNonCSC.OA_Phone = "02122122692";
			addressNonCSC.OA_Fax = "02122122692";
			addressNonCSC.City = "IST";
			addressNonCSC.Postcode = "34300";
			addressNonCSC.OA_RN_NKCountryCode = "TR";

			var supplierWithCSC = Factory.New<OrgHeader>();
			supplierWithCSC.OH_Code = "SuppCSC";
			supplierWithCSC.OH_FullName = "Supplier with SCS";
			supplierWithCSC.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "1234567890123");
			var addressWithCSC = supplierWithCSC.MainAddress;
			addressWithCSC.OA_OH = supplierWithCSC.PK;
			addressWithCSC.CompanyName = "Supplier with SCS Address";
			addressWithCSC.Address1 = "xAdress1";
			addressWithCSC.Address2 = "xAdress2";
			addressWithCSC.OA_Phone = "02122122692";
			addressWithCSC.OA_Fax = "02122122692";
			addressWithCSC.City = "IST";
			addressWithCSC.Postcode = "34300";
			addressWithCSC.OA_RN_NKCountryCode = "TR";
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				declaration.JE_OH_Supplier = supplierNonCSC.PK;
				AssertHasMessageError("CSC type (Customs Supplier Code) is blank", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "Please provide CSC type (Customs Supplier Code) from TR Customs.");

				declaration.JE_OH_Supplier = supplierWithCSC.PK;
				AssertNoMessageErrors("CSC type (Customs Supplier Code) is correct", declaration.SupplierDocumentaryAddress.OrganisationPKInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
