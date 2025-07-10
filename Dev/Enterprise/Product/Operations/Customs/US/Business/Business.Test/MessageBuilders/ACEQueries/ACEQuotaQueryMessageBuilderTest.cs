using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEQuotaQueryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerate()
		{
			var queryInput = new ACEACCaseQueryInput();
			queryInput.CaseStatus = ACCaseStatusList.QueryMessageCodes.Active;
			queryInput.CountryCode = "AU";
			queryInput.HTSNumber = "7007110010";

			var message = new ACEQuotaQueryMessageBuilder().GenerateMessage(Factory, QueryTypeList.Codes.TariffNumber, "5001000000", "", "FR");
			AssertContains("", message.EM_MessageText);

			message = new ACEQuotaQueryMessageBuilder().GenerateMessage(Factory, QueryTypeList.Codes.TextileCategoryNumber, "123", "5001000000", "FR");
			AssertContains("", message.EM_MessageText);
		}
	}
}
