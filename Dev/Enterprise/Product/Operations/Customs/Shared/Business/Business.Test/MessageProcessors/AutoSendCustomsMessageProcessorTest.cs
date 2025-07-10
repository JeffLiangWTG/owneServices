using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(AutoSendCustomsMessageProcessor))]
	public abstract class AutoSendCustomsMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2019, 1, 1, 12, 0, 0)]
		[TestUtcOffset(1, 0, 0)]
		public void TestEndToEndTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			PrepareDeclaration(declaration);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV11071801";
			invoice.JZ_InvoiceAmount = 10000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2106900300";
			invoiceLine.JI_LinePrice = 10000m;
			PrepareInvoiceLine(invoiceLine);
			Factory.Save();
			AssertNull("Entry should be null as job is not merged.", GetEntryHeader(declaration));

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				IProcessor processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(@"message has been sent to customs for Job", informationNotifications[0].Message);

				var newFactory = new BusinessObjectFactory();
				var loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				var entry = GetEntryHeader(loadedDeclaration);
				AssertNotNull("Entry should NOT be empty as job should have been merged", entry);
				AssertEntryAndMessageResultForEndToEndTest(entry);

				processor = CreateProcessor(loadedDeclaration);
				processor.Process(notifications);
				AssertContains(ExpectedLastNotification, notifications.AsString);

				newFactory = new BusinessObjectFactory();
				loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				entry = GetEntryHeader(loadedDeclaration);
				AssertEntryAndMessageResultForEndToEndTest(entry);
			}
		}

		protected virtual string ExpectedLastNotification => "is waiting for response from customs";

		public void TestNoEntryFoundWhenAutoSendingMessage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			PrepareDeclaration(declaration);
			Factory.Save();
			AssertNull("Entry should be null as job is not merged.", GetEntryHeader(declaration));

			var processor = CreateProcessor(declaration);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertContains("There is no entry found to send ", notifications.AsString);

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			AssertNull("Entry should be null as job has no invoices.", GetEntryHeader(loadedDeclaration));
		}

		public void TestEntryHasBeenAcceptedByCustoms()
		{
			if (MessageCanBeAutoSentForLodgedEntries)
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				PrepareDeclaration(declaration);
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV11071801";
				invoice.JZ_InvoiceAmount = 10000m;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "2106900300";
				invoiceLine.JI_LinePrice = 10000m;
				PrepareInvoiceLine(invoiceLine);

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				var entry = GetEntryHeader(declaration);
				AssertNotNull(entry);
				SetEntryClearedStatus(entry);
				AssertEquals(0, entry.Messages.Count);
				AssertEquals(true, entry.HasBeenLodgedAtCustoms);
				Factory.Save();

				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					processor.Process(notifications);
				}
				AssertContains("has been sent to customs", notifications.AsString);

				SetEntryClearedStatus(entry);
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				var loadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				var loadedEntry = GetEntryHeader(loadedDeclaration);
				AssertEquals(1, loadedEntry.Messages.Count);
				AssertEquals(true, loadedEntry.HasBeenLodgedAtCustoms);
			}
			else
			{
				Assert("This test is skipped for current country.", true);
			}
		}

		protected virtual bool MessageCanBeAutoSentForLodgedEntries => true;

		public void TestReportDeclarationErrors()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001100";
			PrepareDeclaration(declaration);
			TriggerNonEmptyMessageTypeValidation(declaration);
			Assert(declaration.JE_MessageTypeInfo.HasErrors());

			var processor = CreateProcessor(declaration);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertContains("following errors on Job:B00001100, please fix all of them and try again.\r\nError - JE_MessageType: Please enter ", notifications.AsString);

			declaration.JE_MessageType = "IMP";
			Factory.Save();
			processor = CreateProcessor(declaration);
			notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertNotContains("following errors on Job:B00001100, please fix all of them and try again.\r\nError - JE_MessageType: Please enter ", notifications.AsString);
		}

		protected virtual void TriggerNonEmptyMessageTypeValidation(BaseJobDeclaration declaration) => declaration.JE_MessageType = "";

		public void TestLockEntryHeaderWhenSendingMessage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			PrepareDeclaration(declaration);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV11071801";
			invoice.JZ_InvoiceAmount = 10000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2106900300";
			invoiceLine.JI_LinePrice = 10000m;
			PrepareInvoiceLine(invoiceLine);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = GetEntryHeader(declaration);
			SetEntryClearedStatus(entry);
			var entryHeaderMutex = new ZGlobalMutex(Enterprise.ZArchitecture.Modules.MutexIDs.SendCustomsMessage, entry.PK.ToString());
			AssertEquals(true, entryHeaderMutex.Lock());

			Factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);
				AssertContains("as CargoWise Support", notifications.AsString);
				AssertContains("is trying to send the same message for this entry. Please wait unitl the lock has been released before trying to send the message again.", notifications.AsString);

				entryHeaderMutex.Unlock();
				processor = CreateProcessor(declaration);
				notifications = new NotificationBuffer();
				processor.Process(notifications);
				AssertContains("has been sent to customs", notifications.AsString);
			}
		}

		public void TestMessageDescription()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			PrepareDeclaration(declaration);
			var processor = CreateProcessor(declaration);
			AssertEquals(ExpectedMessageDescription, (ZString)processor.GetType().GetProperty("MessageDescription", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(processor));
		}

		public void TestNoMessageIsGeneratedWhenServiceProviderInterfaceIsConfiguredInRegistry()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				PrepareDeclaration(declaration);
				Factory.Save();

				if (declaration.ShowSubmitMenuItem)
				{
					var processor = CreateProcessor(declaration);
					var notifications = new NotificationBuffer();
					processor.Process(notifications);
					AssertContains("because this job is configured to submit through a designated service provider interface", notifications.AsString);
				}
				else
				{
					Assert("Service Provider Interface is not configured for this country yet.", true);
				}
			}
		}

		protected abstract void SetEntryClearedStatus(CusEntryHeader entry);

		protected abstract ZString ExpectedMessageDescription { get; }

		protected virtual void PrepareInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
		}
		protected abstract IProcessor CreateProcessor(BaseJobDeclaration declaration);
		protected abstract void PrepareDeclaration(BaseJobDeclaration declaration);
		protected abstract CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration);
		protected abstract void AssertEntryAndMessageResultForEndToEndTest(CusEntryHeader entry);

		protected override void SetUp()
		{
			base.SetUp();
			Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		}
	}
}
