namespace Enterprise.Customs.US.Business.Testing
{
	sealed class VisaStatusListTest : NUnit.Framework.TestCase
	{
		public void TestIsOnFile()
		{
			AssertEquals(true, VisaStatusList.IsOnFile(VisaStatusList.Codes.OnFileCancelled));
			AssertEquals(false, VisaStatusList.IsOnFile(VisaStatusList.Codes.CountryNoInELVIS));
			AssertEquals(true, VisaStatusList.IsOnFile(VisaStatusList.Codes.OnFileUsed));
			AssertEquals(false, VisaStatusList.IsOnFile(VisaStatusList.Codes.NotOnFile));
			AssertEquals(true, VisaStatusList.IsOnFile(VisaStatusList.Codes.OnFileNotUsed));
		}

		public void TestIsStatusDateLastStateUpdateChangeDate()
		{
			AssertEquals(true, VisaStatusList.IsStatusDateLastStateUpdateChangeDate(VisaStatusList.Codes.OnFileCancelled));
			AssertEquals(false, VisaStatusList.IsStatusDateLastStateUpdateChangeDate(VisaStatusList.Codes.CountryNoInELVIS));
			AssertEquals(true, VisaStatusList.IsStatusDateLastStateUpdateChangeDate(VisaStatusList.Codes.OnFileUsed));
		}
	}
}
