using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class REQDOCMessageTextBuilderTest : TestCase
	{
		[TestDate(2016, 07, 19)]
		public void TestGenerateMessageBody()
		{
			var testInput = new REQDOCMessageDataProviderForTest();
			REQDOCMessageTextBuilderForTest testBuilder;
			string result = "";
			CombineAssertions(() =>
			{
				testInput.MessageType = MessageTypeList.Codes.Import;
				testInput.LocalReferenceNumber = "TestReference1";
				testInput.MessageFunction = MessageFunctionCodeList.Codes.Original;
				testInput.FinalMRN = ZString.Empty;
				testInput.RequestDate = ZDateTime.Empty;
				testInput.MessageSender = "TST51051342";
				testBuilder = new REQDOCMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("General Output", ExpectedOutput, result.Replace("'", "'\n"));
				testInput.FinalMRN = "KFN201607115000007";
				testInput.RequestDate = ZDateTime.Today;
				testBuilder = new REQDOCMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Output With MRN and Date", ExpectedOutputWithMrnAndDate, result.Replace("'", "'\n"));
				testInput.DocumentMessageSource = "552700155ba6de52cnode1";
				testBuilder = new REQDOCMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Output With Document Source", ExpectedOutputWithDocSource, result.Replace("'", "'\n"));
			});
		}

		const string ExpectedOutput = @"UNH+<<MSGNO PLACEHOLDER>>+REQDOC:D:99B:UN:ZZZ01'
BGM+929+TestReference1+9'
DOC+929'
NAD+MS+TST51051342'
LIN+1'
UNT+6+<<MSGNO PLACEHOLDER>>'";
		const string ExpectedOutputWithMrnAndDate = @"UNH+<<MSGNO PLACEHOLDER>>+REQDOC:D:99B:UN:ZZZ01'
BGM+929+TestReference1+9'
DOC+929+KFN201607115000007'
DTM+318:20160719:102'
NAD+MS+TST51051342'
LIN+1'
UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string ExpectedOutputWithDocSource = @"UNH+<<MSGNO PLACEHOLDER>>+REQDOC:D:99B:UN:ZZZ01'
BGM+929+TestReference1+9'
DOC+929+KFN201607115000007::552700155ba6de52cnode1'
DTM+318:20160719:102'
NAD+MS+TST51051342'
LIN+1'
UNT+7+<<MSGNO PLACEHOLDER>>'";

		sealed class REQDOCMessageTextBuilderForTest
		{
			public REQDOCMessageTextBuilderForTest(IREQDOCMessageDataProvider dataProvider)
			{
				this.dataProvider = dataProvider;
			}

			readonly IREQDOCMessageDataProvider dataProvider;
			public string GenerateMessageBody()
			{
				var result = new Edifact.D99B.Messages.REQDOC.REQDOCMessage();
				REQDOCMessageTextBuilder.PopulateREQDOCMessage(result, dataProvider);
				return result.ToString(new BatchProcessor.ZACharacterSetNoCasing());
			}
		}
	}
}
