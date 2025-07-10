using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class ACEEntrySummaryQueryProcessorTest : ABIProcessorTest<ACEEntrySummaryQueryProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestKeysForBlockingParallelProcessing_MissingOriginOrLinkToEntryHeader()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse;
			message.EM_Status = EDIMessage.Status.Queued;
			var logger = new LoggingInformation();
			var processor = new ACEEntrySummaryQueryProcessor();
			var provider = processor as IKeysForBlockingParallelProcessingProvider;
			var actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			var actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Message can not find link Job", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.Empty), actualMetaData);
				AssertEquals(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, actualKeys.ReturnValue.ResultType);
			});

			var declaration = GetMergedDeclaration("70022932");
			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery;
			message.EM_MessageNum = "123456";
			outgoingMessage.EM_MessageNum = "123456";
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			outgoingMessage.EM_LinkedObject = entry;
			Factory.Save();

			actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Message link to Job", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(entry.TableName, entry.PK, GlbBranch.CurrentBranch.PK, declaration.JE_DeclarationReference)), actualMetaData);
				AssertArrayEqualsByElements("Keys", new[] { $"Entry:SV9-70022932|{GlbBranch.CurrentBranch.PK}" }, actualKeys.ReturnValue.Keys.ToArray());
			});
		}

		protected override void EndToEndCore()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			var image2 = new System.Drawing.Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			var declaration = GetMergedDeclaration("70022932");
			declaration.JE_GB = newBranch.PK;

			var message = CreateMessage("58315",
"B001101SV9JD                                               58315                " +
"JBSV9  7002293200100061911093801PM       Y2                                     " +
"JC1106191100      10      0            01                                       " +
"JD2060413000000120079000000004659000000001436                                   " +
"10ASV9  70022932 1101B00155968   0140 YY          2062911                       " +
"1113-14792700013-147927000                     061911       IL                  " +
"20BA  1101061911A001                                                            " +
"21001                                                                           " +
"2200000003BF                                                                    " +
"23M    12534234233                                                              " +
"318BN037                                                                        " +
"4A00001                                                                         " +
"40  001 CHCH061911                 0            120    N  N                     " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"503926909980        530        100             X                                " +
"62499     021                                                                   " +
"894992500                                                                       " +
"90        530        2500         000         000         000                   " +
"Y  1101SV9JD00000");

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.OriginalMessage.EM_LinkedObject = entry;

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			entry.Reload();
			AssertEquals("Not standalone message and should be attached to the entry", entry, message.EM_LinkedObject);
			AssertContains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber, Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("Liquidation Date", ZDateTime.Empty, entry.US_ALDate);
			AssertEquals("Collection Date", ZDateTime.Empty, entry.US_CollectionDate);
			AssertEquals("Liquidation Duty", 1200.79m, entry.US_ALDuty);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		public void TestGenerateJB()
		{
			var message = CreateMessage("58322", "B003902SV9JD                                               6009273              JBSV9  6002008600100062711015805PM       Y1042212                               JC2106271100      13      0            01                                       Y  3902SV9JD00000");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Liquidation Status</td><td>Entry Summary Liquidated/Closed</td>"));
		}

		public void TestJCDoNotHaveStatusCode()
		{
			var message = CreateMessage("58316",
"B001101SV9JD                                               58316                " +
"JA EES 050111000000AM050211115959PM                                             " +
"JC1406191100      16      9            01NO1                                    " +
"Y  1101SV9JD00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Status</td><td>Entry Summary Inactive in ACE</td></tr><tr><td>"));
			Assert(sentMail.Body.Contains("<td>Protest Status</td><td>No</td>"));
			Assert(sentMail.Body.Contains("<td>Quota Status</td><td>Quota processed</td>"));
			Assert(!sentMail.Body.Contains("<td>Liquidation Hold Status</td>"));
			Assert(sentMail.Body.Contains("<td>Collection Status</td><td>Authorized</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 1</td>"));
		}

		public void TestGenerateEmailNotificationForNewJCFormat()
		{
			var message = CreateMessage("58316",
"B001101SV9JD                                               58316                " +
"JA EES 050111000000AM050211115959PM                                             " +
"JBSV9  7002293200100061911093801PM       Y2                                     " +
"JC1406191100      16                   01NO1                          43475165  " +
"Y  1101SV9JD00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Status</td><td>Entry Summary Inactive in ACE</td></tr><tr><td>"));
			Assert(sentMail.Body.Contains("<td>Protest Status</td><td>No</td>"));
			Assert(sentMail.Body.Contains("<td>Quota Status</td><td>Quota processed</td>"));
			Assert(!sentMail.Body.Contains("<td>Liquidation Hold Status</td>"));
			Assert(sentMail.Body.Contains("<td>Collection Status</td><td>Authorized</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 1</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 2</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 3</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 4</td>"));
		}

		public void TestJDIncludesJEMessageBlock()
		{
			ErrorReporter.Clear();
			var message = CreateMessage("HYEDUSCMT_175303",
					"B001101SV9JD                                               HYEDUSCMT_175303     " +
					"JBSV9  7102388900100091316032828PM       Y2                                     " +
					"JC1109131600      10      0            1 NO                                     " +
					"JD2      000000000000000000000000000000000000000000089012000000009012           " +
					"JE00001000000000000000000000000000000000000000000000000000000000000             " +
					"Y  1101SV9JD00003"
			);
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertNullOrEmpty("There should not be errors processing a message with a JE Block", ErrorReporter.LastMessageReported);
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Liquidated Interest</td><td>890.12</td>"));
			Assert(sentMail.Body.Contains("<td>Liquidated ADD/CVD</td><td>90.12</td>"));
			Assert(sentMail.Body.Contains("Estimated Duty"));
			Assert(sentMail.Body.Contains("Estimated Tax"));
			Assert(sentMail.Body.Contains("Estimated Fees"));
			Assert(sentMail.Body.Contains("Estimated Interest"));
			Assert(sentMail.Body.Contains("Estimated ADD/CVD"));
		}

		public void TestQueryFromReconJob()
		{
			var declaration = GetMergedDeclaration("70022841");
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.CH_MessageType = "REC";
			declaration.JE_DeclarationReference = "JBZ0001";
			Factory.Save();
			var message = CreateMessage("58322"
				, "B003902SV9JD                                               6009273              JBSV9  7002284100100062711015805PM       Y1042212                               JC2106271100      13      0            01                                       Y  3902SV9JD00000",
				"B      XJ5JC                                               6009273              J1   SV9  70022841                                                              Y      XJ5JC");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Subject.Contains("JBZ0001"));

			var entryHeader2 = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
			entryHeader2.Messages.Load();
			AssertEquals(2, entryHeader2.Messages.Count);
		}

		public void TestStandAloneQuery()
		{
			GetMergedDeclaration("70022841");
			GetMergedDeclaration("70022460");
			GetMergedDeclaration("70022551");
			GetMergedDeclaration("70022627");
			GetMergedDeclaration("70022635");

			var declaration = GetMergedDeclaration("70022668");
			var message = CreateMessage("58322",
				"B001101SV9JD                                               58322                JA EES 050111000000AM053011115959PM                                             JBSV9  6002003700100052411112231PM       Y2                                     JC11052411                             01                                       10ASV9  60020037 3902B00005256   0110 YY      002 2052611                       1191-01319900091-013199000                     052411       IL                  20AAAA3901052411A002ADMIRALENGRACHT                                             21DE1                                                                           2200000001BF                                                                    23MAPLUMASTER1                                                                  318BN891                                                                        4A00001                                                                         40  001XHTHT052411                 060267        45    N  N                     47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 501902194000       6400       1000             KG                               62501     125                                                                   4A00002                                                                         40  002VHTHT052411                 060267        24    N  N                     47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 501902194000        000        500             KG                               4A00003                                                                         40  003VTHTH052411                 060267        23300MN  N                     47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 505206140000        000        250             KG                               4A00004                                                                         40  004VTHHT052411                 060267        25    N  N                     47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 502002908020        000        250             KG                               89501125                                                                        90       6400         125         000         000         000                   JBSV9  7002256900100050311024709PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022569 1101B00155763   0111 YY          2051311                       1157-12345678957-123456789                     050211       PA                  20APLU1101050211B815TITANIC                                                     21Q49                                                                           2200000100CS                                                                    23MAPLUMASTERQ49                                                                318BN891                                                                        4A00001                                                                         40  001 VCVC041311      W       100058866       100    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 500210992000        000      10000 1000000     KG                               62501    1250                                                                   895011250                                                                       90        000        1250         000         000         000                   JBSV9  7002261900100050311032447PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022619 1101B00155768   0111 Y           2060311                       1158-12345678958-123456789                                  PA                  20APLU1101052311B815TITANIC                                                     21Q54                                                                           2200000100CS                                                                    23MAPLUMASTERQ54                                                                318BN891                                                                        4A00001                                                                         40  001 JPJP050311              100058866       100    N  N                     47MJPMATELE288OSA                                                               47C58-123456789                                                                 47S58-123456789                                                                 508536490055      46170      17100 100000      NO                               OA  FC0                                                                         62501    2138                                                                   62499    3591                                                                   894993591       5012138                                                         90      46170        5729         000         000         000                   JBSV9  1000257200100050511052553PM       Y2                                     JC11050511                             01                                       10ASV9  10002572 2704B00005241   0110 YYY         2051711                       1191-01319900091-013199000                     050111       AK                  20OTT12704050111A101ADMIRALENGRACHT                                             2147399                                                                         2200000001PK                                                                    23MAPLU70934790                                                                 318BN891                                                                        4A00001                                                                         40  001 THAU042511                5060267     15000    N  N                     42THLIATHA191NAK UIE809               1 1                                       44COMB HONEY,AND PCK RETAIL                                                     47MTHLIATHA191NAK                                                               47C91-013199000                                                                 47S91-013199000                                                                 500409000025      28500      10000 1500000     KG                               62501    1250                                                                   62055   33000                                                                   62499    2100                                                                   894992500       05533000      5011250                                           90      28500       36750         000         000         000                   JBSV9  7002277500100050611015823PM       Y2                                     JC1105061100      10      0            01                                       10ASV9  70022775 1101B00155797   0111 Y           2051811                       1157-12345678957-123456789                     051611       PA                  20APLU1101050611B815TITANIC                                                     21Q72A                                                                          2200000100PK                                                                    23MAPLUMASTERQ72A                                                               318BN891                                                                        4A00001                                                                         40  001 JPJP042611              100058866       100    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 504911998000        000      10000 100000      KG                               62501    1250                                                                   62499    2100                                                                   895011250       4992500                                                         90        000        3750         000         000         000                   JBSV9  7002290800100052711035055PM       Y2                                     JC1105271100      10      0            01                                       10ASV9  70022908 1101B00155910   0111 YY          2060911                       1191-01319900091-013199000                     052711       IL                  20APLU1101052711B815TITANICSEB                                                  210527                                                                          2200000010CS                                                                    23MAPLUMASTERSEB                                                                319BN8910000010618AB1234567                                                     4A00001                                                                         40  001 JPJP021611              100058866       100    N  N                     47MJPMATELE288OSA                                                               47C91-013199000                                                                 47S91-013199000                                                                 503920995000      58000      10000 10000       KG                               62501    1250                                                                   62499    2100                                                                   895011250       4992500                                                         90      58000        3750         000         000         000                   JBSV9  7002284100100052611012328AM       Y2                                     JC1105261100      10      0            01                                       10ASV9  70022841 1101B00155888   0111 YY          2060811                       1157-12345678957-123456789               052611052411       PA                  20APLU3901052411A00223                             052611                       21FR4                                                                           2200000006BN                                                                    23MAPLUMASTER45                                                                 23HAPLUHOUSE45                                                                  23S    SUBHOUSE45                                                               23I    123456782                                                                2200000001VY                                                                    23MAPLUMASTERTEST                                                               23I    VAP12345670                                                              2200000004BN                                                                    23MAPLUMASTER45                                                                 23HAPLUHOUSE45                                                                  23S    SUBHOUSE45                                                               23I    654789870                                                                318BN891                                                                        319A 89100000005001321345646                                                    4A00001                                                                         40  001 CLCL052411      CL         041380        50    N  N                     47MCLGOZFLA2808SAN                                                              47C57-123456789                                                                 47S57-123456789                                                                 504703110000        000        500 240000      CTN                              62501     063                                                                   4A00002                                                                         40  002 ITCL052411                 041380        50   MN  N                     47MITXANSRLVIL                                                                  47C57-123456789                                                                 47S57-123456789                                                                 504703110000        000        500 240000      CTN                              62499     105                                                                   62501     063                                                                   89501126        4992500                                                         90        000        2626         000         000         000                   JBSV9  7002246000100050311014431PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022460 1101B00155751   0140 Y           2051311                       1157-12345678957-123456789                     123110       PA                  20AA  1103123110B815                               123110                       21123                                                                           2200000100CS                                                                    23M    001314566                                                                23I    430042900                                                                318BN891                                                                        4A00001                                                                         40  001 SGSG123110      SG      1000          10000    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 501901104500      18000      10000 1000000     KG                               90      18000         000         000         000         000                   JBSV9  7002255100100050311024355PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022551 1101B00155762   0130 YY          2051311                       1157-12345678957-123456789                     050311       PA                  20APLU1101050311B815                                                            2200000100CS                                                                    23MAAADSV970022551                                                              318BN891                                                                        4A00001                                                                         40  001 XCCA050311      CA        57             57    N  N                     47MXCKELELE3990BUR                                                              47C57-123456789                                                                 47S57-123456789                                                                 508538908080        000       2000             X                                4A00002                                                                         40  002 TWCA050311                43             43    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 508538908080       5250       1500             X                                62499     315                                                                   894992500                                                                       90       5250        2500         000         000         000                   JBSV9  7002262700100050311034021PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022627 1101B00155770   0111 Y           2051311                       1157-12345678957-123456789                     050311       PA                  20APLU1101050311B815TITANIC                                                     21Q56                                                                           2200000100CS                                                                    23MAPLUMASTERQ56                                                                318BN891                                                                        4A00001                                                                         40  001 ZAZA041311      D       100058866       100    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 504113903000        000      10000 10000       M2                               62501    1250                                                                   62499    2100                                                                   894992500       5011250                                                         90        000        3750         000         000         000                   JBSV9  7002263500100050311035650PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022635 1101B00155771   0140 YY          2051311                       1157-12345678957-123456789                     050311       PA                  20AA  1101050311B815                                                            21123                                                                           2200000100CS                                                                    23M    00132145678                                                              318BN891                                                                        4A00001                                                                         40  001 JPJP050311              1200            100    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 508471410150        000      10000 100000      NO                               OI        WORD PROCESSOR                                                        FC0103 001                 SHAKESPEARE                   WD-1760                FC02000000000001                                                                62499    2100                                                                   894992500                                                                       90        000        2500         000         000         000                   JBSV9  7002270000100050411015813PM       Y2                                     JC1105041100      10      0            01                                       10ASV9  70022700 1101B00155780   0111 YYY         2051611                       1157-12345678957-123456789                     050411       PA                  20APLU1101050411B815TITANIC                                                     21Q67                                                                           2200000100CS                                                                    23MAPLUMASTERQ67                                                                318BN891                                                                        4A00001                                                                         40  001 JPJP041411              100058866       100    N  N                     42JPMATELE288OSA INVQ67               1 5                                       47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 503920995000      58000      10000 100000      KG                               62501    1250                                                                   62499    2100                                                                   894992500       5011250                                                         90      58000        3750         000         000         000                   JBSV9  7002286600100052511102640PM       Y2                                     JC1105251100      10      0            01                                       10ASV9  70022866 1101B00155895   0140 Y     1     2060711                       1191-01319900091-013199000                     052511       IL                  20BA  8888052511I317                                                            21019                                                                           2200000001PK                                                                    23M    12522934844                                                              318BN891                                                                        4A00001                                                                         40  001 GBGB052511                75             50    N  N                     47MGBBOOMED295LON                                                               47C91-013199000                                                                 47S91-013199000                                                                 502204212000        561        224 2835        L                                600170000002471                                                                 62499     047                                                                   894992500                                                                       90        561        2500        2471         000         000                   JBSV9  7002244500100050811091313PM       Y2                                     JC1105081100      10      0            01                                       10ASV9  70022445 1101B00155747   0111 Y           2051311                       1157-12345678957-123456789                     050311       PA                  20APLU1101050311B815TITANIC                                                     21Q34                                                                           2200000100CAS                                                                   23MAPLUMASTERQ34                                                                318BN891                                                                        4A00001                                                                         40  001 GBGB041311              100041380       100    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 500809402000        000      10000 100000      KG                               62501    1250                                                                   62499    2100                                                                   CW02     27D04                                                                  894992500       5011250                                                         90        000        3750         000         000         000                   JBSV9  7002260100100050311031021PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022601 1101B00155767   0111 Y           2051311                       1157-12345678957-123456789                     050311       PA                  20APLU1101050311B815TITANIC                                                     21Q53                                                                           2200000100CS                                                                    23MAPLUMASTERQ53                                                                318BN891                                                                        4A00001                                                                         40  001 JPJP041311              100158866        56    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 509802004040        000       3406                                              509102111010      15644       1852 100000      NO                               509802004040        000          0                                              509102111020      22123       2619 100000      NO                               509802004040        000          0                                              509102111030      11361       1345 100000      NO                               509802004040        000          0                                              509102111040       1723        204 100000      NO                               62501    1179                                                                   895011179                                                                       90      50851        1179         000         000         000                   JBSV9  7002253600100050311023454PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022536 1101B00155760   1150 YY          2051311                       1157-12345678957-123456789                                  PA                  20APLU1101050311B815                                                            2200000100CS                                                                    318BN891                                                                        34496     500                                                                   4A00001                                                                         40  001 JPJP050311               125            100    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 509106100000      11218       1189 12000       NO                               89496500                                                                        90      11218         500         000         000         000                   JBSV9  7002266800100050311041840PM       Y2022912                               JC2105051100      131110120            01                                       JD1062313000000005027000000012379000000007921                                   10ASV9  70022668 1101B00155774   0111 YY          2051311                       1158-12345678958-123456789                     050311       MD                  20APLU1101050311B815TITANIC                                                     21Q63                                                                           2200000100CS                                                                    23MAPLUMASTERQ63                                                                318BN891                                                                        4A00001                                                                         40  001 HKHK041311              100058866       100    N  N                     47MHKGROIND1711HON                                                              47C58-123456789                                                                 47S58-123456789                                                                 508527910500      49000      10000 100000      NO                               OA  FC0                                                                         62501    1250                                                                   62499    2100                                                                   894992500       5011250                                                         90      49000        3750         000         000         000                   JBSV9  7002227000100050411111632AM       Y2                                     JC1105041100      10      0            01                                       10ASV9  70022270 1101B00155595   0110 YY          2051611                       1157-12345678957-123456789                     050411       PA                  20APLU1101050411A001TITANIC                                                     21Q232                                                                          2200000100CTN                                                                   23MAPLUMASTERQ232                                                               318BN891                                                                        4A00001                                                                         40  001 CLCL041411      CL      125041380    245939    N  N                     47MTWNICSAN435TAI                                                               47C57-123456789                                                                 47S57-123456789                                                                 504703110000        000    2054587 2455580000  CTN                              62501  256823                                                                   CW02     27C49                                                                  89501256823                                                                     90        000      256823         000         000         000                   JBSV9  7002272600100052711040925PM       Y2                                     JC1105271100      10      0            11                                       10ASV9  70022726 1101B00155782   0130 Y           2053111                       1157-12345678957-123456789                                  PA                  20    1101050411B815                                                            2200000001PK                                                                    23M    SV970022726                                                              318BN891                                                                        4A00001                                                                         40  001 XCCA050411      CA      1256           5000    N  N                     47MXCKELELE3990BUR                                                              47C57-123456789                                                                 47S57-123456789                                                                 5098178501          000     500000                                              508703330045        000          0 100         NO                               CW02     27D                                                                    90        000         000         000         000         000                   JBSV9  7002276700100050411111022PM       Y2                                     JC1105041100      10      0            01                                       10ASV9  70022767 1101B00155788   0111 Y           2050611                       1157-12345678957-123456789                     033111       PA                  20APLU1101          TITANIC                                                     21Q23                                                                           2200000001CS                                                                    23MAPLUMASTERBILL1                                                              318BN891                                                                        4A00001                                                                         40  001XCHCH031111              100041380        50    N  N                     47MTWNICSAN435TAI                                                               47C57-123456789                                                                 47S57-123456789                                                                 501902194000      32000       5000             KG                               62501     625                                                                   62499    1050                                                                   4A00002                                                                         40  002VCHCH031111               48041380        50    N  N                     47MTWNICSAN435TAI                                                               47C57-123456789                                                                 47S57-123456789                                                                 501902194000        000       2400             KG                               CW02     27C                                                                    4A00003                                                                         40  003VFRCH031111               26041380        13    N  N                     47MTWNICSAN435TAI                                                               47C57-123456789                                                                 47S57-123456789                                                                 500712311000        000       1300             KG                               4A00004                                                                         40  004VITCH031111               26041380        13    N  N                     47MTWNICSAN435TAI                                                               47C57-123456789                                                                 47S57-123456789                                                                 502002908020        000       1300             KG                               89501625        4992500                                                         90      32000        3125         000         000         000                   JBSV9  7002280900100051811020236AM       Y2                                     JC11051811                             01                                       10ASV9  70022809 3902B00155849   0111 YYY         2052711                       1157-12345678957-123456789                     051711       PA                  20APLU4601051711B81523                             051711                       210511                                                                          2200000002BL                                                                    23MAPLU1002MASTER                                                               23HAPLU1002HOUSE                                                                23S    1002SUBHOUSE                                                             23I    123457891                                                                2200000007BL                                                                    23MAPLU1002MASTER                                                               23HAPLU1002HOUSE                                                                23S    1002SUBHOUSE                                                             23I    V1234787896                                                              2200000001BL                                                                    23MAPLU1002MASTER                                                               23HAPLU1002HOUSE                                                                23S    1002SUBHOUSE                                                             23I    456789874                                                                2200000001CS                                                                    23MAPLU1001MASTER                                                               23I    123456782                                                                318BN891                                                                        4A00001                                                                         40  001 JPJP051711                 058866        10    N  N                     42JPMATELE288OSA 1-1                  1 1                                       47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 503920995000        290         50             KG                               62501     006                                                                   62499     011                                                                   89501006        4992500                                                         90        290        2506         000         000         000                   JBSV9  7002249400100050311020800PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022494 1101B00155754   1111 Y   X       2051311                       1157-12345678957-123456789                     051311       PA                  20APLU1101050311B815TITANIC                                                     21Q41                                                                           2200000100CS                                                                    23MAPLUMASTERQ41                                                                318BN891                                                                        34311     200                                                                   4A00001                                                                         40  001 JPJP042311               10058866      1000340 N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 506205202016       4433        225 1200        DOZ                              89311200                                                                        90       4433         200         000         000         000                   JBSV9  7002258500100050311025740PM       Y2                                     JC1105031100      10      0            01                                       10ASV9  70022585 1101B00155765   0140 YY          2051311                       1157-12345678957-123456789                     050311       PA                  20AA  1101050311B815                                                            21123                                                                           2200000100CS                                                                    23M    00112345678                                                              318BN891                                                                        4A00001                                                                         40  001 TWTW050311               500             50    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 503103100020        000       5000 1000        T                                62499    1050                                                                   4A00002                                                                         40  002 KRKR050311               500             50    N  N                     47MJPMATELE288OSA                                                               47C57-123456789                                                                 47S57-123456789                                                                 503103100020        000       5000 1000        T                                62499    1050                                                                   894992500                                                                       90        000        2500         000         000         000                   Y  1101SV9JD00000");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response for multiple entries", sentMail.Subject);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Collection Date", new ZDateTime(2012, 11, 10), entry.US_CollectionDate);
			AssertEquals("Liquidation Duty", 50.27m, entry.US_ALDuty);

			Assert(sentMail.Body.Contains("SV9-7002284-1"));
			Assert(sentMail.Body.Contains("SV9-7002246-0"));
			Assert(sentMail.Body.Contains("SV9-7002255-1"));
			Assert(sentMail.Body.Contains("SV9-7002262-7"));
			Assert(sentMail.Body.Contains("SV9-7002263-5"));
		}

		public void TestNoSummariesFound()
		{
			var message = CreateMessage("58316",
"B001101SV9JD                                               58316                " +
"JA EES 050111000000AM050211115959PM                                             " +
"JZ015   QUERY COMPLETE - NO SUMMARIES FOUND                                     " +
"Y  1101SV9JD00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response for multiple entries", sentMail.Subject);
			Assert(sentMail.Body.Contains("No entries found for the query"));

			message = CreateMessage("58317",
"B001101SV9JD                                               58317                " +
"JZ013   ENTRY SUMMARY NOT FOUND FOR QUERY        SV9  13456789                  " +
"Y  1101SV9JD00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("No summaries found with Entry Number SV9-13456789."));
		}

		public void TestFailedQuery()
		{
			//Message 1
			var message = CreateMessage("58322",
"B001101SV9JD                                               58322                " +
"JA EES 063211012200PM063211255959PM                                             " +
"JZ008   REQUESTED FROM DATE TIME UNKNOWN                                        " +
"JZ010   REQUESTED TO DATE TIME UNKNOWN                                          " +
"Y  1101SV9JD00000");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response (Failure) for multiple entries", sentMail.Subject);

			//Message 2
			message = CreateMessage("58320",
"B001101SV9JD                                               58320                " +
"JA XXX 062011000000AM062011115959PM                                             " +
"JZ006   CRITERIAQUERY TYPE CODE UNKNOWN                                         " +
"JZ017   FUTURE REQUESTED TO DATE NOT ALLOWED                                    " +
"Y  1101SV9JD00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response (Failure) for multiple entries", sentMail.Subject);
			Assert(sentMail.Body.Contains("CRITERIAQUERY TYPE CODE UNKNOWN"));
			Assert(sentMail.Body.Contains("FUTURE REQUESTED TO DATE NOT ALLOWED"));

			//Message 3
			message = CreateMessage("58318",
"B001101SV9JD                                               58318                " +
"JA EES                                                                          " +
"JZ007   REQUESTED FROM DATE TIME MISSING                                        " +
"JZ009   REQUESTED TO DATE TIME MISSING                                          " +
"Y  1101SV9JD00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response (Failure) for multiple entries", sentMail.Subject);
			Assert(sentMail.Body.Contains("REQUESTED FROM DATE TIME MISSING"));

			//Message 4
			message = CreateMessage("58319",
"B001101SV9JD                                               58319                " +
"JA EES 061611012200PM061311115959PM                                             " +
"JZ011   REQUESTED TO DATE < REQUESTED FROM DATE                                 " +
"Y  1101SV9JD00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response (Failure) for multiple entries", sentMail.Subject);
			AssertContains("<td>REQUESTED TO DATE &lt; REQUESTED FROM DATE</td>", sentMail.Body);
			AssertContains("Requested From Date: 16-Jun-11 13:22:00</b><br><b>Requested To Date: 13-Jun-11 23:59:59", sentMail.Body);

			//Message 5
			message = CreateMessage("58319", "B001101SV9JD                                               58319                " +
"JA EES 061611012200PM061311115959PM                                             " +
"JZ011   REQUESTED TO DATE < REQUESTED FROM DATE                                 " +
"Y  1101SV9JD00000");

			message.RelatedMessage.EM_SystemCreateUser = User.ServiceUserCode;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals("No emails sent for auto query", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestQueryEntryNumber()
		{
			var message = CreateMessage("58322", "B003902SV9JD                                               6009273              JBSV9  6002008600100062711015805PM       Y2042212                               JC2106271100      13      0            01                                       Y  3902SV9JD00000");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("SV9-6002008-6"));
		}

		[TestDate(2021, 06, 28)]
		public void TestProcessMessage_LiquidationType_Liquidated_NoPreExistingLiquidation()
		{
			var dec = GetMergedDeclaration("71027633");
			var message = CreateMessage("123",
				"B001101SV9JD                                               123                  JBSV9  7102763300100041217102822AM       Y1041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9JD00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Anticipated Liquidation Date should not be updated for status 1", ZDateTime.Empty, entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNotNull("A liquidation should have been created", liquidation);
			AssertEquals("The liquidation should be set against the Declaration", dec.PK, liquidation.B8_JE);
			AssertEquals("The liquidation type should be set to L", "L", liquidation.B8_LiquidationType);
			AssertEquals("The liquidation date should be set", new ZDateTime(2018, 04, 18), liquidation.B8_LiquidationDate);
			AssertEquals("The liquidation fees should be set", 47.14m, liquidation.B8_TotalLiquidatedFees);
			AssertEquals("The liquidation duty should be set", 156.50m, liquidation.B8_LiquidatedDuty);
			AssertEquals("The entry filer code should be set", "SV9", liquidation.B8_EntryFilerCode);
			AssertEquals("The entry number should be set", "71027633", liquidation.B8_EntryNumber);
			AssertEquals("The liquidation create date should be set", new ZDateTime(2021, 06, 28), liquidation.B8_SystemCreateDate);
		}

		public void TestProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation() => ProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation(true);

		public void TestProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation_LiquidationDateIsInvalid() => ProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation(false);

		public void ProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation(bool liquidationDateIsValid)
		{
			var dec = GetMergedDeclaration("71027633");
			var liq = Factory.NewWithValidTestData<CusLiquidation>();
			liq.B8_JE = dec.PK;
			var liquidationDate = new ZDateTime(2017, 06, 28);
			if (liquidationDateIsValid)
			{
				liq.B8_LiquidationDate = liquidationDate;
			}
			liq.B8_EntryFilerCode = "SV9";
			liq.B8_EntryNumber = "71027633";
			Factory.Save();
			var message = CreateMessage("123",
				"B001101SV9JD                                               123                  JBSV9  7102763300100041217102822AM       Y1041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9JD00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Anticipated Liquidation Date should not be updated for status 1", ZDateTime.Empty, entry.US_ALDate);

			var liquidations = Factory.Load<CusLiquidation>(new ZQuery());
			if (liquidationDateIsValid)
			{
				AssertEquals("No new liquidation should have been created", 1, liquidations.Length);
				AssertEquals("The existing liquidation should not have been modified", liquidationDate, liquidations[0].B8_LiquidationDate);
			}
			else
			{
				AssertEquals("new liquidation should have been created", 2, liquidations.Length);
			}
		}

		public void TestProcessMessage_LiquidationType_Reliquidated()
		{
			var dec = GetMergedDeclaration("71027633");
			var message = CreateMessage("123",
				"B001101SV9JD                                               123                  JBSV9  7102763300100041217102822AM       Y3041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9JD00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Anticipated Liquidation Date should not be updated for status 3", ZDateTime.Empty, entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNotNull("A liquidation should have been created", liquidation);
			AssertEquals("The liquidation should be set against the Declaration", dec.PK, liquidation.B8_JE);
			AssertEquals("The liquidation type should be set to R", "R", liquidation.B8_LiquidationType);
			AssertEquals("The liquidation date should be set", new ZDateTime(2018, 04, 18), liquidation.B8_LiquidationDate);
			AssertEquals("The liquidation fees should be set", 47.14m, liquidation.B8_TotalLiquidatedFees);
			AssertEquals("The liquidation duty should be set", 156.50m, liquidation.B8_LiquidatedDuty);
			AssertEquals("The entry filer code should be set", "SV9", liquidation.B8_EntryFilerCode);
			AssertEquals("The entry number should be set", "71027633", liquidation.B8_EntryNumber);
		}

		public void TestProcessMessage_LiquidationType_NotLiquidated()
		{
			var dec = GetMergedDeclaration("71027633");
			var message = CreateMessage("123",
				"B001101SV9JD                                               123                  JBSV9  7102763300100041217102822AM       Y2041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9JD00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Anticipated Liquidation Date should be updated for status 2", new ZDateTime(2018, 04, 18), entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNull("No liquidation should have been created", liquidation);
		}

		public void TestProcessMessageForReconJob_LiquidationType_Reliquidated()
		{
			var dec = GetMergedDeclaration("71027633");
			var entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.CH_MessageType = "REC";
			Factory.Save();

			var message = CreateMessage("123",
				"B001101SV9JD                                               123                  JBSV9  7102763300100041217102822AM       Y3041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9JD00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
			entry.Messages.Load();
			AssertEquals("Anticipated Liquidation Date should not be updated for status 3", ZDateTime.Empty, entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNotNull("A liquidation should have been created", liquidation);
			AssertEquals("The liquidation should be set against the Declaration", dec.PK, liquidation.B8_JE);
			AssertEquals("The liquidation type should be set to R", "R", liquidation.B8_LiquidationType);
			AssertEquals("The liquidation date should be set", new ZDateTime(2018, 04, 18), liquidation.B8_LiquidationDate);
			AssertEquals("The liquidation fees should be set", 47.14m, liquidation.B8_TotalLiquidatedFees);
			AssertEquals("The liquidation duty should be set", 156.50m, liquidation.B8_LiquidatedDuty);
			AssertEquals("The entry filer code should be set", "SV9", liquidation.B8_EntryFilerCode);
			AssertEquals("The entry number should be set", "71027633", liquidation.B8_EntryNumber);
		}

		public void TestProcessMessage_LiquidationForReconJob()
		{
			var dec = GetMergedDeclaration("71027633");
			var entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.CH_MessageType = "REC";
			dec.JE_DeclarationReference = "JBZ0001";
			Factory.Save();

			var message = CreateMessage("123",
				"B001101SV9JD                                               123                  JBSV9  7102763300100041217102822AM       Y2041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9JD00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
			entry.Messages.Load();
			AssertEquals("Anticipated Liquidation Date should be updated for status 2", new ZDateTime(2018, 04, 18), entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNull("No liquidation should have been created", liquidation);
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "SV9";

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			dec.ActiveEntryHeaders.EntrySummaryEntry.EntryNumber = entryNumber;

			Factory.Save();
			return dec;
		}

		MQEDIMessage CreateMessage(ZString messageNum, ZString receiveMessageText, string originalMessageText = "")
		{
			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery;
			if (!string.IsNullOrEmpty(originalMessageText))
			{
				outgoingMessage.EM_MessageText = originalMessageText;
			}

			var message = GetMessageToProcess();
			message.EM_MessageText = receiveMessageText;
			Factory.Save();
			message.EM_MessageNum = messageNum;
			outgoingMessage.EM_MessageNum = messageNum;
			Factory.Save();
			return message;
		}

		MQEDIMessage GetMessageToProcess()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}
	}
}
