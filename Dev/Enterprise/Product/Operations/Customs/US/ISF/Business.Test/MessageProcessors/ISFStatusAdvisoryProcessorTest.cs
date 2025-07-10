using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFStatusAdvisoryProcessorTest : ABIProcessorTest<ISFStatusAdvisoryProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory;
			message.EM_MessageNum = "~150000";
			message.EM_MessageText = "B018888FLRSA                                                                    " + "SA10FLR-20080000001                                                             " + "SA30HBSC999999999999                                                            " + "SA50S1BILL ON FILE                                                              " + "Y8888FLRSA00005                                                                 ";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_ApplicationReference should be updated from message", "FLR-20080000001", message.EM_ApplicationReference);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject.Contains("Importer Security Filing");
			}));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
			string expectedBodyMessage = string.Format(@"<br />
Transaction Number FLR-20080000001<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Number</th><th>Disposition Code</th><th>Remarks</th></tr></thead><tr><td>HBSC999999999999</td><td>S1</td><td>{0}</td></tr></table>
<br />", DispositionCodeList.Descriptions.S1);
			AssertContains(expectedBodyMessage, email.Body);
		}

		public void TestProcessMessageNoBillOnFile()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory;
			message.EM_MessageNum = "~150000";
			message.EM_MessageText = "B018888FLRSA                                                                    " +
				"SA10FLR-20080000001                                                             " +
				"SA30HBSC555555555555                                                            " +
				"SA50S2NO BILL ON FILE; 30 DAYS TO EXPIRATION                                    " +
				"Y8888FLRSA00005                                                                 ";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject.Contains("Importer Security Filing");
			}));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
			string expectedBodyMessage = string.Format(@"<br />
Transaction Number FLR-20080000001<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Number</th><th>Disposition Code</th><th>Remarks</th></tr></thead><tr><td>HBSC555555555555</td><td>S2</td><td>{0}</td></tr></table>
<br />", DispositionCodeList.Descriptions.S2);
			AssertContains(expectedBodyMessage, email.Body);
		}

		public void TestProcessMessageNoBillOnFileISFExpired()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory;
			message.EM_MessageNum = "~150000";
			message.EM_MessageText = "B018888FLRSA                                                                    " +
				"SA10FLR-20080000001                                                             " +
				"SA30OBSC777777777777                                                            " +
				"SA50S5NO BILL ON FILE; ISF EXPIRED                                              " +
				"Y8888FLRSA00005                                                                 ";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject.Contains("Importer Security Filing");
			}));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
			string expectedBodyMessage = string.Format(@"<br />
Transaction Number FLR-20080000001<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Number</th><th>Disposition Code</th><th>Remarks</th></tr></thead><tr><td>OBSC777777777777</td><td>S5</td><td>{0}</td></tr></table>
<br />", DispositionCodeList.Descriptions.S5);
			AssertContains(expectedBodyMessage, email.Body);
		}

		public void TestUpdateBill()
		{
			var notificationForPM = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			notificationForPM.GS_EmailAddress = "test@cargowise.com";
			AssertUpdateBill("HBSC999999999999");
		}

		public void TestUpdateBillCaseInsensitive()
		{
			var notificationForPM = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			notificationForPM.GS_EmailAddress = "test@cargowise.com";
			AssertUpdateBill("hbsc999999999999");
		}

		public void TestProcessingEmails_WhenParentHasHVLTRFLog_UseRegistryHVLVImporterSecurityFilingMessagesGroup()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "KNZ";

			var hvlvRecipient = group.Staff.AddNew();
			hvlvRecipient.GS_Code = "K1";
			hvlvRecipient.GS_LoginName = "K1";
			hvlvRecipient.GS_EmailAddress = "k1@k1.email.com";
			Factory.Save();

			ISFRegistry.Instance.HVLVImporterSecurityFilingMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, false));

			var header = Factory.New<CusISFHeader>();
			((ZArchitecture.Business.IStmALogProvider)header).Logs.AddNew(ZArchitecture.Business.AutoEvents.Transferred, "|TYP=HVL");

			var bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = "BILL0001";
			bill.BB_BillType = "BM";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageText = "B018888XJ5SF                                                                     Y         SF00001000000000000000000000000";
			outgoingMessage.EM_LinkedObject = header;

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingInterchange.EI_HeaderText = "A3901SV9      07160901   071609021226                                00004006830";
			incomingInterchange.EI_BodyText = "B018888FLRSA                                                                    " +
				"SA10FLR-20080000001                                                             " +
				("SA30" + bill.BB_BillNum.ToUpper()).PadRight(80, ' ') +
				"SA50S1BILL ON FILE                                                              " +
				"Y8888FLRSA00005                                                                 ";
			incomingInterchange.EI_FooterText = "Z3901SV9      07160901   071609021227                                00004006830";
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_From = "USC";
			incomingInterchange.EI_To = "SV93902";
			incomingInterchange.EI_InterchangeNum = "~15000";

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText = incomingInterchange.EI_BodyText;
			incomingMessage.EM_EI = incomingInterchange.PK;

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Contains recipient from the HVLVImporterSecurityFilingMessagesGroup", true, email.Recipients.Contains("k1@k1.email.com"));
				AssertEquals("Does not contain staffZ1's email", true, !email.Recipients.Contains("dong@pretend.email.com"));
				AssertEquals("Does not contain staffZ2's email", true, !email.Recipients.Contains("dong2@pretend.email.com"));
			});
			mock.VerifyAll();
		}

		public void AssertUpdateBill(ZString billNo)
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newCompany.GC_Name = "New Company";
			newCompany.GC_Code = "NEC";
			newBranch.GB_GC = newCompany.PK;
			newBranch.GB_Code = "NEB";
			Factory.Save();
			var context = new TemporaryUserContext();
			context.BranchPK = newBranch.PK.ToGuid();
			using (context.Set())
			{
				EnvProxy.Instance.Registry.MailboxDisplayName = "New Company Display Name";
			}

			var header = Factory.New<CusISFHeader>();
			header.BF_GB = newBranch.PK;
			var bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = billNo;
			bill.BB_BillType = "BM";
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock
				.Protected()
				.Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			MQEDIMessage outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageText = "B018888XJ5SF                                                                     Y         SF00001000000000000000000000000";
			outgoingMessage.EM_LinkedObject = header;
			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingInterchange.EI_HeaderText = "A3901SV9      07160901   071609021226                                00004006830";
			incomingInterchange.EI_BodyText = "B018888FLRSA                                                                    " +
				"SA10FLR-20080000001                                                             " +
				("SA30" + billNo.ToUpper()).PadRight(80, ' ') +
				"SA50S1BILL ON FILE                                                              " +
				"Y8888FLRSA00005                                                                 ";
			incomingInterchange.EI_FooterText = "Z3901SV9      07160901   071609021227                                00004006830";
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_From = "USC";
			incomingInterchange.EI_To = "SV93902";
			incomingInterchange.EI_InterchangeNum = "~15000";
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText = incomingInterchange.EI_BodyText;
			incomingMessage.EM_EI = incomingInterchange.PK;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			AssertEquals("New Company Display Name", Env.OutgoingCustomsMailManager.EmailsCreated[0].FromDisplayName);
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			bill.Reload();
			AssertEquals("S1", bill.BB_CustomsStatus);
			AssertEquals(new ZDateTime(2009, 07, 16), bill.BB_MatchDate);
			mock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();
			ISFRegistry.Instance.ImporterSecurityFilingMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupZZ1.PK.ToGuid());
		}
	}
}
