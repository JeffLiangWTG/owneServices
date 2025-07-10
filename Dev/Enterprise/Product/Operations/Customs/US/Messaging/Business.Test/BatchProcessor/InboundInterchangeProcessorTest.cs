using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	public class IncomingInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestExecute()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "Z1Z";
			otherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_Code = "Z1Z";
			otherBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			otherCompany.Branches.Add(otherBranch);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";

			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();

			var interchangeHeader1 =
				"A3910SV9      10281001   110110212849                                00000054211";

			var interchangeBody1 =
				"B018888BHCEI                                               54442                " +
				InboundMessageCreatorForTesting.ThrowInvalidMessageFormatExceptionTrigger.PadRight(80);

			var interchange1 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", interchangeHeader1, interchangeBody1, "");
			AssertEquals(CBPEDIInterchange.Status.Queued, interchange1.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange1.EI_InterchangeType.IsEmpty);

			var interchangeHeader2 =
				"A3910SV9      10281001   110110212849                                00000054212";

			var interchangeBody2 =
				"B018888BHCEI                                               54443                " +
				"EB A  AND  B  REC DP/FLR/OFFICE CONFLICT                                        " +
				"EBTRANSACTION DATA REJECTED                                                     " +
				"EB Z  RECORD MISSING OR INVALID                                                 " +
				"EBTRANSACTION DATA REJECTED                                                     ";

			var interchange2 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", interchangeHeader2, interchangeBody2, "");
			AssertEquals(CBPEDIInterchange.Status.Queued, interchange2.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange2.EI_InterchangeType.IsEmpty);

			var interchangeHeader3 =
				"A3910SV9      10281001   110110212849                                00000054213";

			var interchangeBody3 =
				"B018888BHCEI                                               54444                " +
				"EB A  AND  B  REC DP/FLR/OFFICE CONFLICT                                        " +
				"EBTRANSACTION DATA REJECTED                                                     " +
				"EB Z  RECORD MISSING OR INVALID                                                 " +
				"EBTRANSACTION DATA REJECTED                                                     ";

			var interchangeOfOtherBranch = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", interchangeHeader3, interchangeBody3, "");
			interchangeOfOtherBranch.EI_GB = otherBranch.PK;
			Factory.Save();
			AssertEquals(CBPEDIInterchange.Status.Queued, interchangeOfOtherBranch.EI_Status);
			AssertEquals("EI_InterchangeType empty on creation?", true, interchangeOfOtherBranch.EI_InterchangeType.IsEmpty);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logger = new LoggingInformation();
			var processor = new InboundInterchangeProcessorForTesting(logger);
			processor.ExecuteBatch();

			interchange1.Reload();
			AssertEquals(CBPEDIInterchange.Status.Failed, interchange1.EI_Status);
			AssertEquals("", interchange1.EI_InterchangeType);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Error processing message");
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("test@edi.com.au", email.Recipients[0].Email);
			AssertEquals("A message had the following error during processing.\r\nMessage text contain invalid format", email.Body);
			AssertEquals(1, email.Attachments.Count);
			var attachment = email.Attachments[0];
			AssertEquals("MessageData.zip", attachment.DisplayName);
			var extractor = new ZArchitecture.Core.ZipExtractor("");
			using (var messageDataStream = new VirtualMemoryStream())
			{
				extractor.ExtractZipStream(new MemoryStream(attachment.Data), messageDataStream, "MessageData.txt");
				messageDataStream.Flush();
				AssertEquals(interchangeBody1, new StreamReader(messageDataStream).ReadToEnd());
			}

			interchange2.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange2.EI_Status);
			AssertEquals("EI", interchange2.EI_InterchangeType);

			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, InboundInterchangeProcessorForTesting.AppCode);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, interchange2.EI_InterchangeType);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, "0");
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, Enterprise.Messaging.Business.EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Enterprise.Messaging.Business.EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			var msgs = Factory.Load<CBPMessageForTesting>(ediMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];

			//ZString expectedMessageText = "B018888BHCEI                                               54443                EB A  AND  B  REC DP/FLR/OFFICE CONFLICT                                        EBTRANSACTION DATA REJECTED                                                     EB Z  RECORD MISSING OR INVALID                                                 EBTRANSACTION DATA REJECTED                                                     Y           00000";
			AssertEquals("EM_MessageText", interchangeBody2 + "Y           00000", msg.EM_MessageText);

			interchangeOfOtherBranch.Reload();
			AssertEquals("This interchange should not be processed as it doesn't below to the current branch", CBPEDIInterchange.Status.Queued, interchangeOfOtherBranch.EI_Status);
			AssertEquals("", interchangeOfOtherBranch.EI_InterchangeType);

			string loggingResult = new StringCollectionX(logger.UserLogStrings).ToString();
			AssertContains("Logger contains", "Message text contain invalid format", loggingResult);
		}

		public static CBPEDIInterchange CreateAndSaveInterchange(BusinessObjectFactory factory, string applicationCode, string sender, string receiver, string headerText, string bodyText, string footerText)
		{
			var interchange = factory.New<CBPEDIInterchange>();

			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_Status = CBPEDIInterchange.Status.Queued;
			interchange.EI_From = sender;
			interchange.EI_To = receiver;

			interchange.EI_HeaderText = headerText;
			interchange.SetEI_BodyTextSource(new TextReaderSource(new StringReader(bodyText).GetPaddedMemoryStream()));
			interchange.EI_FooterText = footerText;

			factory.Save();

			return interchange;
		}
	}
}
