using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public abstract class AutomatedClearinghouseMessageProcessorTest<TPaymentAuthorisationResponse, T> : ABIProcessorTest<T, APLA, APLB, APLY>
			where TPaymentAuthorisationResponse : MessageBlock, IPaymentAuthorisationResponse, new()
			where T : AutomatedClearinghouseMessageProcessor
	{
		protected override void EndToEndCore()
		{
			var currentStaffMember = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			currentStaffMember.GS_EmailAddress = "test@cargowise.com";

			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var chargeCode = testHelper.DisbursementChargeCode;
			var creditor = testHelper.DisbursementCreditor;
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

			var image1 = new System.Drawing.Bitmap(1, 2);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			var declaration = GetDeclaration("1", testHelper.Importer);
			declaration.CustomsEntryHeaders[0].EntryNumber = "1";
			var declaration2 = GetDeclaration("2", testHelper.Importer);
			declaration2.CustomsEntryHeaders[0].EntryNumber = "2";
			var declaration3 = GetDeclaration("3", testHelper.Importer);
			declaration3.CustomsEntryHeaders[0].EntryNumber = "3";

			var header = Factory.New<CusStatementHeader>();
			header.B2_ProcessPort = "8888";
			header.B2_StatementNumber = "12345678943";
			header.B2_EntryFilerCode = "XJ3";
			header.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			header.B2_StatementAmount = 102m;
			var line1 = header.StatementLines.AddNew();
			line1.B3_EntryNum = "1";
			line1.B3_EntryFilerCode = "XJ5";
			line1.B3_CustomsFeesTotal = 2m;
			var charge1 = line1.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Avocado;
			charge1.B4_ChargeAmount = 2m;

			var line2 = header.StatementLines.AddNew();
			line2.B3_EntryNum = "2";
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_CustomsFeesTotal = 80m;
			var charge2 = line2.Charges.AddNew();
			charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Avocado;
			charge2.B4_ChargeAmount = 80m;

			var line3 = header.StatementLines.AddNew();
			line3.B3_EntryNum = "3";
			line3.B3_EntryFilerCode = "XJ5";
			line3.B3_CustomsFeesTotal = 20m;
			var charge3 = line3.Charges.AddNew();
			charge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Avocado;
			charge3.B4_ChargeAmount = 20m;

			Factory.Save();

			var generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = applicationIdentifier;
			generator.B.ProcessingDistrictPortCode = "8888";

			generator.AddMessageBlock(CreatePaymentAuthorisationResponse("12345678943XJ3ZG40000132553040307CQ3THANKS FOR THE PAYMENT        123456"));
			generator.AddMessageBlock(CreatePaymentAuthorisationResponse("79123456789XJ3ZG40000132553040307CQ3THANKS FOR THE PAYMENT        123456"));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var factory = new BusinessObjectFactory();
			var decLoaded = factory.Load<JobDeclaration>(declaration.PK);
			ProcessMessage(factory, generator);
			factory.Save();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains(responseSubject)));
			Assert(email.Body.Contains(responseSubject));
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));

			var headerLoaded = factory.Load<CusStatementHeader>(header.PK);

			AssertNotNull(headerLoaded.ServiceTaskLogger);
			AssertEquals(PaymentStatusList.Codes.PaymentAuthorizationAccepted, headerLoaded.B2_PaymentStatus);
			AssertEquals(new ZDateTime(2007, 4, 3), headerLoaded.B2_PaymentAuthorizationDate);
			AssertEquals(new ZDateTime(2007, 4, 3), decLoaded.US_PaymentDate);

			AssertAPInvoiceDetails("1", -2m, declaration);
			AssertAPInvoiceDetails("2", -80m, declaration2);
			AssertAPInvoiceDetails("3", -20m, declaration3);

			var email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains(responseSubject)));
			var banner = email2.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		void AssertAPInvoiceDetails(ZString invoicenumber, ZDecimal lineAmountExpected, JobDeclaration declaration)
		{
			var invoicingJob = new JobHeader.Loader(declaration).Load();
			AssertNotNull(invoicingJob);

			var invoiceQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, invoicenumber);
			invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_JH, invoicingJob.PK);
			invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoicingJob.JH_GC);

			var apInvoice = Factory.LoadTop1<AccTransactionHeader>(invoiceQuery);
			AssertNull(apInvoice);
		}

		protected JobDeclaration GetDeclaration(ZString entryNumber, OrgHeader importer)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = "SEA";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = entryNumber;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			return declaration;
		}

		protected abstract string applicationIdentifier { get; }
		protected abstract string responseSubject { get; }
		protected abstract string resposeBlockIdentifier { get; }
		protected abstract ManifestGroupNotificationRegistryItem StatementGroup { get; }

		protected override void SetUp()
		{
			base.SetUp();
			StatementGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new ManifestGroupNotification(Enterprise.Core.Constants.EmailTo.StaffMemberAndNominatedGroup, groupZZ1.PK.ToGuid(), false));
		}

		protected TPaymentAuthorisationResponse CreatePaymentAuthorisationResponse(ZString data)
		{
			var response = new TPaymentAuthorisationResponse();
			response.Deserialise(resposeBlockIdentifier + data.PadRight(78));
			return response;
		}
	}
}
