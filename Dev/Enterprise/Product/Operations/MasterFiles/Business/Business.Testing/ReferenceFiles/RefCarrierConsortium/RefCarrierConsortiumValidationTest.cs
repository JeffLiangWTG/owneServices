using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCarrierConsortiumValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRG_Code()
		{
			RefCarrierConsortium consortium = Factory.New<RefCarrierConsortium>();
			consortium.RunPreSaveValidation();
			AssertHasErrors("Expecting consortium name to have errors", consortium.RG_CodeInfo);

			consortium.RG_Code = "ABC";
			AssertNoErrors("Expecting consortium name to have no errors", consortium.RG_CodeInfo);
		}

		public void TestRG_OHBlank()
		{
			RefCarrierConsortium consortium = Factory.New<RefCarrierConsortium>();
			consortium.RG_OH = OrgHeader.New(Factory).PK;
			AssertNoWarnings("Org specified", consortium.RG_OHInfo);

			consortium.RG_OH = ZGuid.Empty;
			AssertHasWarnings("Org not specified", consortium.RG_OHInfo);
		}

		public void TestRG_OHAlreadyUsed()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			RefCarrierConsortium consortium = Factory.New<RefCarrierConsortium>();
			consortium.RG_OH = org.PK;
			consortium.RG_Code = "TEST";
			Factory.Save();

			RefCarrierConsortium consortium2 = Factory.New<RefCarrierConsortium>();
			consortium2.RG_OH = org.PK;
			AssertHasErrors("Same org used on 2 consortiums", consortium2.RG_OHInfo);

			consortium2.RG_OH = OrgHeader.New(Factory).PK;
			AssertNoErrors("NEW org used on 2nd consortium", consortium2.RG_OHInfo);
		}
	}
}
