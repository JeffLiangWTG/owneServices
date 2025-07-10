namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImporterTypeListTest : NUnit.Framework.TestCase
	{
		public void TestIsGovernmentImporter()
		{
			AssertEquals(false, ImporterTypeList.IsGovernmentImporter(ImporterTypeList.Codes.Corporation));
			AssertEquals(true, ImporterTypeList.IsGovernmentImporter(ImporterTypeList.Codes.ForeignGovernment));

			AssertEquals(false, ImporterTypeList.IsGovernmentImporter(ImporterTypeList.Codes.Individual));
			AssertEquals(true, ImporterTypeList.IsGovernmentImporter(ImporterTypeList.Codes.StateGovernment));

			AssertEquals(false, ImporterTypeList.IsGovernmentImporter(ImporterTypeList.Codes.Partnership));
			AssertEquals(true, ImporterTypeList.IsGovernmentImporter(ImporterTypeList.Codes.USGovernment));
			AssertEquals(false, ImporterTypeList.IsGovernmentImporter(ImporterTypeList.Codes.SoleProprietor));
		}
	}
}
