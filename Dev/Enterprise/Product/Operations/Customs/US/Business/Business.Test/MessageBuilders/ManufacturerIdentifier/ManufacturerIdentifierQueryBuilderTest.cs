using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ManufacturerIdentifierQueryBuilderTest : TestCaseWithFactory
	{
		[TestDate(2017, 12, 31)]
		public void TestGenerateACEMessage()
		{
			DeclarationTestHelper.SetupForSendMessage();
			var messageData = new USMIDQuery(Factory);
			messageData.US_MID = "123";
			var message = ManufacturerIdentifierQueryBuilder.Generate(messageData);
			Assert(message.EM_MessageText.StartsWith("B  8888XJ5MA"));
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameAndAddressQuery, message.EM_MessageType);
		}
	}
}
