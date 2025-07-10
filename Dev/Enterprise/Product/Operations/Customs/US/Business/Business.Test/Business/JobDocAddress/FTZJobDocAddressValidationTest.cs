using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZJobDocAddressValidationTest : FTZJobDeclarationValidationTest
	{
		public void TestCheckWarehouseOrganisationPK()
		{
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			warehouse.OH_FullName = "JPDuminy Bond Stores";
			warehouse.OH_IsWarehouseClient = true;
			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.UnitedStates);
			var message = "You have not entered a FTZ operator.";
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			OrgAddress address = warehouse.Addresses.AddNew();
			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "WH01");
			declaration.WarehouseDocAddress.E2_OA_Address = address.PK;
			declaration.Validation.ValidateAll();
			AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, message);
			declaration.WarehouseDocAddress.OrganisationPK = ZGuid.Empty;
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
		}
		JobDeclaration declaration;
	}
}
