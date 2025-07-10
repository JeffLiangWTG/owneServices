using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestProcessCommodityShipmentResponseMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var interchangeText =
				"B  60612456712E          US EXPORTER NAME                                       " +
				"SC1N11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 N    " +
				"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
				"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
				"Y  60612456712E          US EXPORTER NAME                                       ";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.USCustomsExport, "USC", "861161674",
				"A    861161674CAREDIEXT20091207T9AUS9N861161674",
				interchangeText, "Z    861161674      EXT20091207T9AUS9 861161674");

			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);

			IInboundInterchangeProcessor processor = new AESInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, interchange.EI_InterchangeType);

			var msgs = Factory.Load<AESTIREDIMessage>(EdiMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];
			AssertEquals(interchangeText.TrimEnd(), msg.EM_MessageText);
			AssertEquals("2147483647", msg.EM_MessageNum);
		}

		public void TestProcessCommodityShipmentResponseMessageWhenAblockHasMessageNumberCombinationOfNumbersAndCharacters()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var interchangeText =
"B  04352570700E          ANDE CORPORATION                                       " +
"SC1N40JPMAJL  S300055934       AJAPAN AIRLINES CO., LTD2      041720231130 N    " +
"CL1OS 0001PARTS AND ACCESSORIES OF NON-ELECTRICAL INSTR0000000000 AC33F        1" +
"CL29027908400KG 00000000410000015400   00000000000000000148EAR99NLR             " +
"ES18W1  V SHPING WGT/QUANTITY 1 OUT OF RANGE                                    " +
"ES1972 AV SHIPMENT ADDED; MUST VERIFY             X20230504639873               " +
"Y  04352570700E          ANDE CORPORATION                                       ";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.USCustomsExport, "USC", "861161674",
				"A    042773397CAREDIEXT20231127G5G                                              ",
				interchangeText, "Z    042773397      EXT20231127G5G                                              ");

			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);

			IInboundInterchangeProcessor processor = new AESInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, interchange.EI_InterchangeType);

			var msgs = Factory.Load<AESTIREDIMessage>(EdiMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];
			AssertEquals(interchangeText.TrimEnd(), msg.EM_MessageText);
			AssertEquals("8898", msg.EM_MessageNum);
		}

		public void TestProcessCommodityShipmentFilingType()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var interchangeText =
				"B  60612456712E          US EXPORTER NAME                                       " +
				"SC1N11AUCAUNKNS40002509        ABAI YUN HE             3F60267270420081101 N    " +
				"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
				"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
				"Y  60612456712E          US EXPORTER NAME                                       ";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.USCustomsExport, "USC", "861161674",
				"A    861161674CAREDIEXT20091207T9AUS9N861161674",
				interchangeText, "Z    861161674      EXT20091207T9AUS9 861161674");

			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);

			IInboundInterchangeProcessor processor = new AESInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, interchange.EI_InterchangeType);

			var msgs = Factory.Load<AESTIREDIMessage>(EdiMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];
			AssertEquals(interchangeText.TrimEnd(), msg.EM_MessageText);
		}

		public void TestHandlingInvalidData()
		{
			var postMaster = Factory.LoadFromNaturalKey<MasterFiles.Business.GlbStaff>(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
			postMaster.GS_EmailAddress = "postMaster@dummy.com";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.USCustomsExport, "USC", "861161674",
				"A    861161674CAREDIEXT20091207T9AUS9N861161674", "@$#$#$%KLDSDJ", "");

			IInboundInterchangeProcessor processor = new AESInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			var messages = Factory.Load<MQEDIMessage>(new ZQuery());
			AssertEquals(0, messages.Length);
			var emails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(x => x.Subject == "Error processing message");
			AssertEquals(1, emails.Count);
			var email = emails[0];

			AssertEquals(1, email.Attachments.Count);
			var attachment = email.Attachments[0];
			AssertEquals("MessageData.zip", attachment.DisplayName);
			var extractor = new ZArchitecture.Core.ZipExtractor("");

			using (var messageDataStream = new VirtualMemoryStream())
			{
				extractor.ExtractZipStream(new MemoryStream(attachment.Data), messageDataStream, "MessageData.txt");
				messageDataStream.Flush();
				AssertContains("@$#$#$%KLDSDJ", new StreamReader(messageDataStream).ReadToEnd());
			}
		}

		public void TestProcessUnsolicitedShipmentResponseMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var interchangeText =
				"B  58123456789EU1QqODI0KjACE TEST IMPORTER 1                                    " +
				"SC1N11HKPAAPLUS300055934        TITANIC                2 58201110120230508 N    " +
				"ES197H AI SHIPMENT ON HOLD.CONTACT 5555555555     X20230504639873               " +
				"Y  58123456789EU0UqNSowMDACE TEST IMPORTER 1                                    ";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(
				Factory, CBPEDIInterchange.ApplicationCodes.USCustomsExport, "USC", "861161674",
				"A  SV753406230      EXT202305040     N                                          ",
				interchangeText, "Z  SU753406230R0UqMSEXT202305040                                                ");

			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);

			IInboundInterchangeProcessor processor = new AESInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, interchange.EI_InterchangeType);

			var msgs = Factory.Load<AESTIREDIMessage>(EdiMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];
			AssertEquals(string.Empty, msg.EM_MessageNum);
		}

		ZQuery EdiMessageQuery
		{
			get
			{
				var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CBPEDIInterchange.ApplicationCodes.USCustomsExport);
				ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.AES.CommodityShipmentResponse);
				ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
				ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
				return ediMessageQuery;
			}
		}
	}
}
