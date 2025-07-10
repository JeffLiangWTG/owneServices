using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class GOVGIOMessageBuilderTest : TestCaseWithFactory
	{
		public void TestCreateMessage()
		{
			var header = GOVGIOExamples.CreateSeaDepotGateIn(Factory);

			var messageHedaer = new GOVGIOMessageHeader(header);
			var messageBuilder = new GOVGIOMessageBuilderForTest(messageHedaer, MessageSubTypes.Create);
			AssertEquals(MessageSubTypeCodes.Codes.Original, messageBuilder.GetMessageSubType_Exposed());

			AssertEquals("pre-condition", 0, header.Messages.Count);
			messageBuilder.PopulateMessages();

			AssertEquals(1, header.Messages.Count);
			var ediMessage = header.Messages[0];

			CombineAssertions(() =>
			{
				AssertEquals(SARSEDIMessage.MessageTypes.GOVGIO, ediMessage.EM_MessageType);
				AssertEquals(MessageSubTypeCodes.Codes.Original, ediMessage.EM_MessageSubType);
				AssertContains("message should look like GOVGIO message", "+GOVCBR:D:", ediMessage.EM_MessageText);
				AssertEquals(EDIMessage.ApplicationCodes.SouthAfricanCustoms, ediMessage.EM_ApplicationCode);
				AssertEquals(EDIMessage.Status.Queued, ediMessage.EM_Status);
			});
		}

		public void TestMessageContentEscapedWithCorrectCharacterSet()
		{
			var header = GOVGIOExamples.CreateSeaDepotGateIn(Factory);
			header.AMA_Voyage = "1-+':?-9";

			var messageHedaer = new GOVGIOMessageHeader(header);
			var messageBuilder = new GOVGIOMessageBuilderForTest(messageHedaer, MessageSubTypes.Create);
			AssertEquals(MessageSubTypeCodes.Codes.Original, messageBuilder.GetMessageSubType_Exposed());

			AssertEquals("pre-condition", 0, header.Messages.Count);
			var result = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText;
			AssertContains("Escaped Voyage", "TDT+20+1-?+?'?:??-9+1++:::", result);
		}
	}

	sealed class GOVGIOMessageBuilderForTest : GOVGIOMessageBuilder
	{
		public GOVGIOMessageBuilderForTest(GOVGIOMessageHeader source, MessageSubTypes messagesubType) : base(source, messagesubType)
		{
		}

		public ZString GetMessageSubType_Exposed()
		{
			return GetMessageSubType();
		}
	}
}
