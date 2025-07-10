using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class GBISubmissionStatusListTest : TestCase
	{
		public void TestIsWaitingForResponse()
		{
			AssertEquals(true, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.AwaitingGBIAdd));
			AssertEquals(true, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.AwaitingGBIUpdate));
			AssertEquals(true, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.AwaitingGBIDelete));
			AssertEquals(false, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.ClearGBIAdd));
			AssertEquals(false, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.ClearGBIUpdate));
			AssertEquals(false, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.ClearGBIDelete));
			AssertEquals(false, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.ErrorGBIAdd));
			AssertEquals(false, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.ErrorGBIUpdate));
			AssertEquals(false, GBISubmissionStatusList.IsWaitingForResponse(GBISubmissionStatusList.Codes.ErrorGBIDelete));
		}
	}
}
