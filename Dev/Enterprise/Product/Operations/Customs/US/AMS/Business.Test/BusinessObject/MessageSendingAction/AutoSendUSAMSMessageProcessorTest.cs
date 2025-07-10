using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using IProcessor = CargoWise.EntityFramework.IProcessor;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class AutoSendUSAMSMessageProcessorTest : TestCaseWithFactory
	{
		public void TestAutoSendUSAMSMessageProcessor_ActionCodeAndAmendmentCodeWillBePurged()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			header.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			bill.B0_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity;
			bill.B0_BillAmendmentCode = AMSAmendmentCodeList.Codes._05;
			Factory.Save();

			AssertEquals(AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity, bill.B0_BillActionCode);
			AssertEquals(AMSAmendmentCodeList.Codes._05, bill.B0_BillAmendmentCode);

			IProcessor processor = new AutoSendUSAMSMessageProcessor(header);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			AssertEquals(ZString.Empty, bill.B0_BillActionCode);
			AssertEquals(ZString.Empty, bill.B0_BillAmendmentCode);
		}

		public void TestAutoSendUSAMSMessageProcessor_TwoMessagesWillBeCreatedWhenActionCodeIsX()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			header.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			bill.B0_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd;
			bill.B0_BillAmendmentCode = AMSAmendmentCodeList.Codes._05;
			var moveHeader = header.MovementHeader;
			Factory.Save();

			IProcessor processor = new AutoSendUSAMSMessageProcessor(header);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			AssertEquals(2, moveHeader.Messages.Count);
			AssertNotNull(moveHeader.Messages.Where(x => x.EM_MessageSubType == AMSMessageSubTypeList.Codes.AmendingDelete).FirstOrDefault());
			AssertNotNull(moveHeader.Messages.Where(x => x.EM_MessageSubType == AMSMessageSubTypeList.Codes.Creating && x.EM_SendWithMessageErrors && x.EM_Status == EDIMessage.Status.Pending).FirstOrDefault());
		}

		public void TestAutoSendUSAMSMessageWithNoError()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			header.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			IProcessor processor = new AutoSendUSAMSMessageProcessor(header);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			AssertContains(@"Manifest message has been sent to customs for Job:AMS AMS0000001", notification.AsString);
		}

		public void TestAutoSendUSAMSMessageWithError()
		{
			var header = Factory.New<CusInBondHeader>();
			header.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			IProcessor processor = new AutoSendUSAMSMessageProcessor(header);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			AssertContains(@"There is no Manifest message sent to customs for Job:AMS AMS0000001", notification.AsString);
		}

		public void TestAutoSendUSAMSMessage_WhenNoSCACCode_LogsValidationError()
		{
			var header = Factory.New<CusInBondHeader>();
			header.Company.OrgProxy.CustomsCodes.AddNew(string.Empty, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			IProcessor processor = new AutoSendUSAMSMessageProcessor(header);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			AssertContains(@"System cannot send Manifest message because of following errors on Job:AMS AMS0000001, please fix all of them and try again.", notification.AsString);
		}

		public void TestAutoSendMessageBatchProcessor_WhenBizObjIsICustomsManifestMessageSupporter_SendAMSMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var header = Factory.NewWithValidTestData<CusInBondHeader>();
				var bill = header.Bills.AddNew();
				bill.FillWithValidTestData();
				header.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);

				CustomsStmProcessQueueLoader.New(header, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, "SEM");
				CustomsStmProcessQueueLoader.New(header, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, "SMM");

				var srmProcessQueue = Factory.New<StmProcessQueue>();
				srmProcessQueue.SW_ApplicationCode = "ASC";
				srmProcessQueue.SW_JobTypeCode = "CUS";
				srmProcessQueue.SW_ActionCode = "SMM";
				srmProcessQueue.SW_ReferenceID = header.PK;
				srmProcessQueue.SW_ReferenceTableCode = "BH";
				srmProcessQueue.SW_PostedTimeUtc = ZDateTime.Today;

				Factory.Save();

				var processor = new AutoSendCustomsMessagingBatchProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				var logs = new ZStringBuilder();
				var enumerator = processor.Logger.UserLogStrings.GetEnumerator();
				while (enumerator.MoveNext())
				{
					logs.Append(enumerator.Current.Trim());
				}

				AssertContains(@"Manifest message has been sent to customs for Job:AMS AMS0000001", logs.ToStringWithNewLineBetweenAppends());
				AssertContains(@"System cannot send the Manifest message for Bill of Lading  , please check whether it's waiting for response from customs.
There is no Manifest message sent to customs for Job:AMS AMS0000001", logs.ToStringWithNewLineBetweenAppends());
			}
		}

		public void TestAutoSendMessageBatchProcessor_Send1000MessagesWith10MinuteInterval()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var header = Factory.NewWithValidTestData<CusInBondHeader>();
				var moveHeader = header.MovementHeader;
				var helperForTest = new AMSMessageHelperForTest();
				IProcessor processor = new AutoSendUSAMSMessageProcessor(header) { AMSMessageHelper = helperForTest };
				var batchSize = helperForTest.BatchSize;
				var delay = helperForTest.Delay;

				for (int i = 0; i < batchSize * 4; i++)
				{
					var bill = header.Bills.AddNew();
				}
				header.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);

				processor.Process(new NotificationBuffer());
				moveHeader.Messages.Sort(AMSEDIMessage.Schema.EM_HeldUntilDate);

				for (int i = 0; i < batchSize; i++)
				{
					AssertEquals(moveHeader.Messages[i].EM_HeldUntilDate, ZDateTime.Empty);
					AssertEquals(moveHeader.Messages[i + 2 * batchSize].EM_HeldUntilDate, moveHeader.Messages[i + batchSize].EM_HeldUntilDate.AddMinutes(delay));
					AssertEquals(moveHeader.Messages[i + 3 * batchSize].EM_HeldUntilDate, moveHeader.Messages[i + 2 * batchSize].EM_HeldUntilDate.AddMinutes(delay));
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Env.NumberFountains.USAMSJobReference.SetNext(Factory, 1);
		}

		class AMSMessageHelperForTest : AMSMessageHelper
		{
			public AMSMessageHelperForTest()
				: base()
			{ }

			public override int BatchSize
			{
				get
				{
					return 10;
				}
			}
		}
	}
}
