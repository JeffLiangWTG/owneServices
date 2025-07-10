using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportJobDocAddressValidationTest : ImportJobDeclarationValidationTest
	{
		public void TestCheckOrganisationPK()
		{
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			declaration.US_EnableENS = true;
			AssertNoWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoBondedWarehouse("Inventory Management"));
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoFIRMSCodeForWarehouse("Inventory Management"));
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertHasWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoBondedWarehouse("Inventory Management"));
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoFIRMSCodeForWarehouse("Inventory Management"));
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertNoWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoBondedWarehouse("Inventory Management"));
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertHasWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoBondedWarehouse("Inventory Management"));
			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			warehouse.OH_FullName = "JPDuminy Bond Stores";
			warehouse.OH_IsWarehouseClient = true;
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			AssertNoWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoBondedWarehouse("Inventory Management"));
			AssertHasMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoFIRMSCodeForWarehouse("Inventory Management"));
			OrgAddress address = warehouse.Addresses.AddNew();
			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "WH01");
			declaration.WarehouseDocAddress.E2_OA_Address = address.PK;
			declaration.US_EnableENS = false;
			declaration.US_EnableENS = true;
			AssertNoWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoBondedWarehouse("Inventory Management"));
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoFIRMSCodeForWarehouse("Inventory Management"));
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			declaration.US_EnableENS = false;
			declaration.US_EnableENS = true;
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.NoFIRMSCodeForWarehouse("Inventory Management"));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.WarehouseDocAddress.OrganisationPK = ZGuid.Empty;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertHasMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableCRL = false;
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EnableCRL = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			Factory.ClearCachedValue<bool>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.WarehouseDocAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			OrgAddress address1 = warehouse.Addresses.AddNew();
			var cusCode1 = address1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "WH01", Core.Constants.CountryCodes.UnitedStates);
			declaration.WarehouseDocAddress.E2_OA_Address = address.PK;
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.FIRMSCodeShouldBeUnique);
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			address1.CustomsCodes.Delete(cusCode1);
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, ImportJobDocAddressValidation.FIRMSCodeShouldBeUnique);
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		}
	}
}
