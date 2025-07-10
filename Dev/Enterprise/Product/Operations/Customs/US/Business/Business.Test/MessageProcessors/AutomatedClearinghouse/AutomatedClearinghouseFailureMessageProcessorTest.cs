using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public abstract class AutomatedClearinghouseFailureMessageProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where T : AutomatedClearinghouseFailureMessageProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
			where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
			where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected override void EndToEndCore()
		{
			var currentStaffMember = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			currentStaffMember.GS_EmailAddress = "test@cargowise.com";
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			chargeCode = chargeCode ?? Factory.NewWithValidTestData<AccChargeCode>();
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ3";
			declaration.JE_TransportMode = "SEA";
			declaration.ImportEntryNumber = "1";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			var image2 = new System.Drawing.Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);
			declaration.JE_GB = newBranch.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			Factory.Save();

			var header = Factory.New<CusStatementHeader>();
			header.B2_ProcessPort = "8888";
			header.B2_StatementNumber = "12345678943";
			header.B2_EntryFilerCode = "XJ3";
			header.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;

			var line1 = header.StatementLines.AddNew();
			line1.B3_EntryNum = "1";
			line1.B3_EntryFilerCode = "XJ3";
			line1.B3_CustomsFeesTotal = 2m;
			var charge1 = line1.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Avocado;
			charge1.B4_ChargeAmount = 2m;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = @"B018888XJXJ3                                               ~15000               Y  8888XJXJ300000";
			message.EM_MessageType = ACSapplicationIdentifier;
			message.EM_LinkedObject = header;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = ACSapplicationIdentifier;
			generator.B.ProcessingDistrictPortCode = "8888";
			generator.B.UserData = "~15000";

			generator.AddMessageBlock(CreateENSEB(@"EB""A"" AND ""B"" REC DP/FLR/OFFICE CONFLICT"));
			generator.AddMessageBlock(CreateENSEB("EBTRANSACTION DATA REJECTED"));

			ProcessMessage(generator);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains(ResponseSubject)));
			Assert(email.Body.Contains(ResponseSubject));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));

			AssertEquals(PaymentStatusList.Codes.PaymentFailed, header.B2_PaymentStatus);
			AssertEquals(ZDateTime.Empty, header.B2_PaymentAuthorizationDate);

			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = @"B018888XJXJ3                                               ~15000               Y  8888XJXJ300000";
			message.EM_MessageType = ACEapplicationIdentifier;
			message.EM_LinkedObject = header;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;

			generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = ACEapplicationIdentifier;
			generator.B.ProcessingDistrictPortCode = "8888";
			generator.B.UserData = "~15000";

			generator.AddMessageBlock(CreateENSEB(@"EB""A"" AND ""B"" REC DP/FLR/OFFICE CONFLICT"));
			generator.AddMessageBlock(CreateENSEB("EBTRANSACTION DATA REJECTED"));

			ProcessMessage(generator);

			AssertEquals(PaymentStatusList.Codes.PaymentFailed, header.B2_PaymentStatus);
			AssertEquals(ZDateTime.Empty, header.B2_PaymentAuthorizationDate);
		}

		protected abstract string ACEapplicationIdentifier { get; }
		protected abstract string ACSapplicationIdentifier { get; }
		protected abstract string ResponseSubject { get; }
		protected abstract ManifestGroupNotificationRegistryItem StatementGroup { get; }

		protected override void SetUp()
		{
			base.SetUp();
			StatementGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new ManifestGroupNotification(Enterprise.Core.Constants.EmailTo.StaffMemberAndNominatedGroup, groupZZ1.PK.ToGuid(), false));
		}

		ENSEB CreateENSEB(ZString data)
		{
			var response = new ENSEB();
			response.Deserialise(BlockPadder.Pad(data));
			return response;
		}
	}
}
