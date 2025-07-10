using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Consol.Reading
{
	sealed class ForwardingConsolMatchServiceTest : TestCaseWithFactory
	{
		public void Test_GetConsolFetchRule()
		{
			var fetchParam = new ConsolFetchParam()
			{
				MasterBillNumber = "123456",
				BookingConfirmationReference = "654321",
				CoLoadMasterBillNumber = "coload_123456",
				CoLoadBookingConfirmationReference = "coload_654321",
				SCAC = "SCAC",
				CoLoadSCAC = "CoLoadSCAC",
				C1C = "C1C",
				CoLoadC1C = "CoLoadC1C"
			};
			var consolFetchRule = ForwardingConsolMatchService.GetConsolFetchRule(fetchParam);

			AssertEquals(3, consolFetchRule.Count);

			AssertEquals(true, consolFetchRule[0].isCoLoad);
			AssertEquals(2, consolFetchRule[0].consolFetchRule.MatchRules.Count);
			AssertEquals(3, consolFetchRule[0].consolFetchRule.ScoreRules.Count);

			AssertEquals(true, consolFetchRule[1].isCoLoad);
			AssertEquals(2, consolFetchRule[1].consolFetchRule.MatchRules.Count);
			AssertEquals(3, consolFetchRule[1].consolFetchRule.ScoreRules.Count);

			AssertEquals(null, consolFetchRule[2].isCoLoad);
			AssertEquals(2, consolFetchRule[2].consolFetchRule.MatchRules.Count);
			AssertEquals(3, consolFetchRule[2].consolFetchRule.ScoreRules.Count);

			fetchParam = new ConsolFetchParam()
			{
				MasterBillNumber = "123456",
				BookingConfirmationReference = "654321",
				CoLoadMasterBillNumber = "",
				CoLoadBookingConfirmationReference = "",
				SCAC = "SCAC",
				CoLoadSCAC = "",
				C1C = "C1C",
				CoLoadC1C = ""
			};
			consolFetchRule = ForwardingConsolMatchService.GetConsolFetchRule(fetchParam);

			AssertEquals(2, consolFetchRule.Count);

			AssertEquals(true, consolFetchRule[0].isCoLoad);
			AssertEquals(2, consolFetchRule[0].consolFetchRule.MatchRules.Count);
			AssertEquals(2, consolFetchRule[0].consolFetchRule.ScoreRules.Count);

			AssertEquals(null, consolFetchRule[1].isCoLoad);
			AssertEquals(2, consolFetchRule[1].consolFetchRule.MatchRules.Count);
			AssertEquals(3, consolFetchRule[1].consolFetchRule.ScoreRules.Count);
		}
	}
}
