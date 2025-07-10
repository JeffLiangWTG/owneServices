namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportEntryStatusListTest : NUnit.Framework.TestCase
	{
		public void TestGetEntryStatusFromErrorCode()
		{
			AssertEquals(ImportEntryStatusList.Codes.CRL, ImportEntryStatusList.GetEntryStatusFromErrorCode("2A5"));
			AssertEquals(ImportEntryStatusList.Codes.CRL, ImportEntryStatusList.GetEntryStatusFromErrorCode("2A4"));
			AssertEquals(ImportEntryStatusList.Codes.CRF, ImportEntryStatusList.GetEntryStatusFromErrorCode("2A3"));
			AssertEquals(ImportEntryStatusList.Codes.CRN, ImportEntryStatusList.GetEntryStatusFromErrorCode("IN4"));
			AssertEquals(ImportEntryStatusList.Codes._05, ImportEntryStatusList.GetEntryStatusFromErrorCode("57A"));
		}

		public void TestIsCRLCertified()
		{
			AssertEquals(true, ImportEntryStatusList.IsCRLCertified("2A5"));
			AssertEquals(true, ImportEntryStatusList.IsCRLCertified("2A4"));
			AssertEquals(false, ImportEntryStatusList.IsCRLCertified("IN4"));
		}
	}
}
