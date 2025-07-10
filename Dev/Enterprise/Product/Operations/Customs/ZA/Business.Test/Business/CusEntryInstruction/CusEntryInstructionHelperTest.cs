using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryInstructionHelperTest : TestCaseWithFactory
	{
		public void TestGetRegNoWithOrganisationAddressInHelper()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inst = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code = testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CODCA", "ZA");
			var warehouseAddress = testOrg2.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = testOrg2.MainAddress.PK;
			inst.CEI_OA_Warehouse2 = testOrg2.MainAddress.PK;
			Factory.Save();
			AssertEquals("RegNo with CarrierCode", org2Code.OK_CustomsRegNo, inst.Warehouse2.GetRegNoWithOrganisationAddress(OrgCusCode.CodeTypes.CarrierCode));
		}
	}
}
