using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoBillValidationTest : TestCaseWithFactory
	{
		// CS00078424
		public void TestPackTypeForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.JE_HouseBill = "HB2342342";
			Bill housebill = declaration.PrimaryHouseBill;
			housebill.CU_NoOfPacks = 0;
			housebill.CU_PackType = "KG";
			AssertNoMessageErrors(housebill);
		}
	}
}
