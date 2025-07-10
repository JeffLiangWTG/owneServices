using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	public class FTZMessageSenderTest : MessageSenderTest
	{
		public void TestSendMessageIfNotSaved()
		{
			shouldSave = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnAllowNotificationsNotSet()
		{
			shouldAllowNotifications = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnAllowNotificationsReturnsFalse()
		{
			shouldReturnTrueOnAllowNotifications = false;
			Assert(!Sender.SendMessage());
		}

		public override void TestSendMessageIfAllOK()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";

			//should be merged automatically before message send
			bool sentSuccessfuly = Sender.SendMessage();
			Assert(sentSuccessfuly);
			var query = new ZQuery(StmALogSchema.SL_Parent, Declaration.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CCC");

			var customsCommencedLogs = Declaration.Logs.Find(query);
			AssertEquals("One Customs Commenced Log exists", 1, customsCommencedLogs.Length);
			CargoWise.Common.ErrorReporter.Clear();
			Assert(!declaration.JE_EntrySubmittedDate.IsEmpty);

			var ftzEntry = declaration.ActiveEntryHeaders.FTZEntry;
			Assert(!ftzEntry.CH_EntrySubmittedDate.IsEmpty);
		}

		public void TestUpdateFTZYear()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			Declaration.FTZYear = ZDate.Today.AddYears(-1).ToString("yy");
			Declaration.FTZZoneID = "1234567";
			Factory.Save();

			bool sentSuccessfuly = Sender.SendMessage();
			Assert(sentSuccessfuly);
			AssertEquals(ZDate.Today.ToString("yy"), Declaration.FTZYear);
		}

		public void TestCreditOnHold()
		{
			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.CompanyData.OB_AROnCreditHold = true;
			declaration.Importer.CompanyData.OB_IsDebtor = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var initiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = initiator;
			declaration.DoMerge();
			declaration.FTZZoneID = "1111111";
			declaration.FTZControlNumber = "ABC22222";

			Factory.Save();

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var ftzSender = new FTZMessageSender(ftzMessageSendingObject);
			ftzSender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate
			{ return true; });
			ftzSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ Factory.Save(); });
			ftzSender.SendMessage();

			AssertEquals("Credit limiting on -- should not be 'lodged'", Enterprise.Customs.Business.MessageSender.MessageSendingStatus.PreparingJob, ftzSender.SendingStatus);

			declaration.Importer.CompanyData.OB_AROnCreditHold = false;
			declaration.Importer.CompanyData.OB_IsDebtor = false;
			Factory.Save();

			ftzSender.SendMessage();

			AssertEquals("Credit limiting off -- should be 'lodged'", Enterprise.Customs.Business.MessageSender.MessageSendingStatus.Done, ftzSender.SendingStatus);

			declaration.Importer.CompanyData.OB_AROnCreditHold = true;
			declaration.Importer.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var ftzSender2 = new FTZMessageSender(ftzMessageSendingObject);
			ftzSender2.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate
			{ return true; });
			ftzSender2.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ Factory.Save(); });
			ftzSender2.SendMessage();

			AssertEquals("Credit limiting on, but credit should not be checked since original successfully 'lodged'", Enterprise.Customs.Business.MessageSender.MessageSendingStatus.Done, ftzSender.SendingStatus);

			IStatusList list = new FTZMessageStatusList();
			declaration.LogManager.AddAClearLogIfNecessary(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd, FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, list);
			Factory.Save();

			ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Delete);
			ftzSender2 = new FTZMessageSender(ftzMessageSendingObject);
			ftzSender2.SendMessage();
			AssertEquals("Credit limiting on, but credit should not be checked since original successfully 'lodged'", Enterprise.Customs.Business.MessageSender.MessageSendingStatus.Done, ftzSender.SendingStatus);
		}

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			sender = null; //on every test sender will be constructed			
			Declaration.HasChanges = true; //on every test sender will check "Declaration.HasChanges" property
			shouldSave = true;
			shouldAllowNotifications = true;
			shouldReturnTrueOnAllowNotifications = true;
		}

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					var ftzMessageSendingObject = new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);
					sender = new FTZMessageSender(ftzMessageSendingObject);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldAllowNotifications)
					{
						sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
						{ return shouldReturnTrueOnAllowNotifications; });
					}
				}
				return sender;
			}
		}
		FTZMessageSender sender;

		bool shouldSave = true;
		bool shouldAllowNotifications = true;
		bool shouldReturnTrueOnAllowNotifications = true;

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					declaration.US_EntryFilerCode = "XJ5";
					declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
					var importer = Factory.NewWithValidTestData<OrgHeader>();
					var stmNums = Factory.New<OrganisationViewStmNums>();
					stmNums.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
					stmNums.SN_ZoneIDPrefix = "1234567";
					stmNums.SN_ClientPrefix = "AAA";
					stmNums.SN_Owner = importer.PK;
					declaration.JE_OH_Importer = importer.PK;
					declaration.IOROrgPK = importer.PK;
					Factory.Save();
					declaration.FTZZoneID = "7777777";
					declaration.FTZControlNumber = "CCC00001";
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
