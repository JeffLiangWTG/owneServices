using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.CALINF;
using Enterprise.Customs.ZA.Business.MessageBuilders.CALINF.Testing;
using CALINFMessage = Enterprise.Edifact.D16A.Messages.CALINF.CALINFMessage;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class CALINFMessageTextBuilderTest : TestCaseWithFactory
	{
		public void TestMessageText_Sea_Schedule()
		{
			var dataProvider = CALINFMessageDataTestHelper.CreateCALINFMessageTestData_Sea_Schedule();
			var builder = new CALINFMessageTextBuilderHelper(dataProvider);
			var messageText = builder.GenerateMessageBody();

			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CALINF:D:16A:UN:RCG001'
BGM+96:::SCH+<<SYSCAR>>+9'
DTM+137:202006051122:203'
NAD+MS+ABC::ZZZ'
TDT+20+VOY456+1++CARRIERCODE:172:20:CARRIERNAME+++RCS123:103::VESSELNAME:GB'
RFF+ACL:VOY123'
LOC+5+DEHAM:139:6'
DTM+136:202006011313:203'
LOC+11+GBLON:139:6'
DTM+178:202006021414:203'
LOC+153+ZADUR:139:6'
DTM+132:202006031515:203'
UNT+13+<<MSGNO PLACEHOLDER>>'";

			AssertMultilineASCIIEquals(expected, messageText.Replace("'", "'\n"));
		}

		public void TestMessageText_Sea_Schedule_Amendment()
		{
			var dataProvider = CALINFMessageDataTestHelper.CreateCALINFMessageTestData_Sea_Schedule();
			dataProvider.DocumentToBeAmended = "OrigDocNo";

			var builder = new CALINFMessageTextBuilderHelper(dataProvider);
			var messageText = builder.GenerateMessageBody(MessageSubTypes.Amend);

			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CALINF:D:16A:UN:RCG001'
BGM+96:::SCH+<<SYSCAR>>+4'
DTM+137:202006051122:203'
RFF+ACW:ORIGDOCNO'
NAD+MS+ABC::ZZZ'
TDT+20+VOY456+1++CARRIERCODE:172:20:CARRIERNAME+++RCS123:103::VESSELNAME:GB'
RFF+ACL:VOY123'
LOC+5+DEHAM:139:6'
DTM+136:202006011313:203'
LOC+11+GBLON:139:6'
DTM+178:202006021414:203'
LOC+153+ZADUR:139:6'
DTM+132:202006031515:203'
UNT+14+<<MSGNO PLACEHOLDER>>'";

			AssertMultilineASCIIEquals(expected, messageText.Replace("'", "'\n"));
		}

		public void TestMessageText_Air_Schedule()
		{
			var dataProvider = CALINFMessageDataTestHelper.CreateCALINFMessageTestData_Air_Schedule(Factory);
			var builder = new CALINFMessageTextBuilderHelper(dataProvider);
			var messageText = builder.GenerateMessageBody();

			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CALINF:D:16A:UN:RCG001'
BGM+96:::ASC+<<SYSCAR>>+9'
DTM+137:202006051122:203'
NAD+MS+ABC::ZZZ'
TDT+20+BA5577+4++BA:172:3:BRITISH AIRWAYS+++:146:::GB'
RFF+ACL:BA2233'
LOC+5+HAM:145:3'
DTM+136:202006011313:203'
LOC+11+LON:145:3'
DTM+178:202006021414:203'
LOC+153+DUR:145:3'
DTM+132:202006031515:203'
UNT+13+<<MSGNO PLACEHOLDER>>'";

			AssertMultilineASCIIEquals(expected, messageText.Replace("'", "'\n"));
		}

		public void TestMessageText_Air_Schedule_Cancellation()
		{
			var dataProvider = CALINFMessageDataTestHelper.CreateCALINFMessageTestData_Air_Schedule(Factory);
			dataProvider.DocumentToBeAmended = "OrigDocNo";

			var builder = new CALINFMessageTextBuilderHelper(dataProvider);
			var messageText = builder.GenerateMessageBody(MessageSubTypes.Withdraw);

			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CALINF:D:16A:UN:RCG001'
BGM+96:::ASC+<<SYSCAR>>+1'
DTM+137:202006051122:203'
RFF+ACW:ORIGDOCNO'
NAD+MS+ABC::ZZZ'
TDT+20+BA5577+4++BA:172:3:BRITISH AIRWAYS+++:146:::GB'
RFF+ACL:BA2233'
LOC+5+HAM:145:3'
DTM+136:202006011313:203'
LOC+11+LON:145:3'
DTM+178:202006021414:203'
LOC+153+DUR:145:3'
DTM+132:202006031515:203'
UNT+14+<<MSGNO PLACEHOLDER>>'";

			AssertMultilineASCIIEquals(expected, messageText.Replace("'", "'\n"));
		}

		sealed class CALINFMessageTextBuilderForTest : CALINFMessageTextBuilder
		{
			public CALINFMessageTextBuilderForTest(CALINFMessage edifactMessage, ICALINFMessageDataProvider source, MessageSubTypes subType)
				: base(edifactMessage, source, subType)
			{
			}

			public void PopulateCALINFMessage()
			{
				Create();
			}
		}

		sealed class CALINFMessageTextBuilderHelper
		{
			public CALINFMessageTextBuilderHelper(ICALINFMessageDataProvider dataProvider)
			{
				this.dataProvider = dataProvider;
			}

			readonly ICALINFMessageDataProvider dataProvider;

			public string GenerateMessageBody(MessageSubTypes subType = MessageSubTypes.Create)
			{
				var message = new CALINFMessage();
				var builder = new CALINFMessageTextBuilderForTest(message, dataProvider, subType);
				builder.PopulateCALINFMessage();
				return message.ToString(new Edifact.UNOACharacterSet());
			}
		}
	}
}
