using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEACQueryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerate()
		{
			var queryInput = new ACEACCaseQueryInput();
			queryInput.CaseStatus = ACCaseStatusList.QueryMessageCodes.Active;
			queryInput.CountryCode = "AU";
			queryInput.HTSNumber = "7007110010";

			var message = new ACEACQueryMessageBuilder().Generate(Factory, queryInput);
			AssertContains("Q2 A AU", message.EM_MessageText);

			var q2 = message.MessageBlock.MessageBlocks.OfType<AADQQ2>().FirstOrDefault();
			AssertNotNull(q2);
			AssertEquals("7007110010", q2.HTSNumber);

			var queryData = new ACEACCaseQuery(Factory);
			queryData.CaseNumbers.AddNew().CaseNumber = "A1234567";
			queryData.CaseNumbers.AddNew().CaseNumber = "C789456123";
			message = new ACEACQueryMessageBuilder().Generate(Factory, queryData);
			q2 = message.MessageBlock.MessageBlocks.OfType<AADQQ2>().FirstOrDefault();
			AssertNull(q2);

			var q1 = message.MessageBlock.MessageBlocks.OfType<AADQQ1>().FirstOrDefault();
			AssertNotNull(q1);
			AssertEquals("A123456", q1.CaseNumber1);
			AssertEquals("7", q1.CaseNumber1Suffix);
			AssertEquals("C789456", q1.CaseNumber2);
			AssertEquals("123", q1.CaseNumber2Suffix);
			AssertEquals(ZString.Empty, q1.CaseNumber3);
		}

		public void TestGenerateWithCaseNumber()
		{
			var message = new ACEACQueryMessageBuilder().Generate(Factory, new ZString[] { "A111" });
			AssertContains("Q1 A111", message.EM_MessageText);
		}
	}
}
