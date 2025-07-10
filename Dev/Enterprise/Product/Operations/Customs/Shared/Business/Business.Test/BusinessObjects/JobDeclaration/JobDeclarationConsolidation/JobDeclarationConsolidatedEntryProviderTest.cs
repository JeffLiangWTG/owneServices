using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class JobDeclarationConsolidatedEntryProviderTest : TestCaseWithFactory
	{
		public void TestQueueForConsolidation_SaveFailure()
		{
			Factory.RefreshEnabled = false;
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var consolidationProvider = new JobDeclarationConsolidatedEntryProvider(declaration);

			declaration.JE_EntryStatus = ZString.Empty;
			declaration.JE_MessageStatus = ZString.Empty;
			Factory.Save();

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;
			var sameDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			sameDeclaration.JE_EntryStatus = "ABC";
			newFactory.Save();

			var result = consolidationProvider.QueueForConsolidation(out var message);
			AssertEquals(false, result);
			AssertNull(message);
			AssertContains("While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCanRemove()
		{
			var consolidatedDeclaration =
				ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			var declaration = consolidatedDeclaration.LeadDeclaration;
			var provider =
				new JobDeclarationConsolidatedEntryProvider(declaration, new ConsolidatedEntryDeclarationRemover());
			Assert("Can remove when no entry number", provider.CanRemove);
			declaration.CustomsEntryHeaders.First().EntryNumber = "12345";
			Assert("Can't remove when there is entry number", !provider.CanRemove);
		}

		public void TestRemoveFromConsolidation_CannotUnlink_OnlyOneDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var declaration = consolidatedDeclaration.LeadDeclaration;
			Factory.Save();
			var consolidationProvider = new JobDeclarationConsolidatedEntryProvider(declaration, new ConsolidatedEntryDeclarationRemover());
			var result = consolidationProvider.DequeueOrRemoveFromConsolidation(out var resultMessage);
			Assert("Declaration hasn't been deleted", consolidatedDeclaration.JobDeclarations.Contains(declaration));
			AssertEquals("Result message", "A Consolidation requires at least 1 declaration, you cannot remove the last one.", resultMessage);
			Assert("Dequeue result", !result);
		}

		public void TestRemoveFromConsolidation_UnlinkNotLeadDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			var declaration = consolidatedDeclaration.JobDeclarations.First(x => x != consolidatedDeclaration.LeadDeclaration);
			Factory.Save();
			var consolidationProvider = new JobDeclarationConsolidatedEntryProvider(declaration, new ConsolidatedEntryDeclarationRemover());
			var result = consolidationProvider.DequeueOrRemoveFromConsolidation(out var resultMessage);
			Assert("Declaration has been removed", !consolidatedDeclaration.JobDeclarations.Contains(declaration));
			Assert("Dequeue result", result);
			AssertEquals("Result message", "Job is no longer in Consolidation.", resultMessage);
		}

		public void TestRemoveFromConsolidation_UnlinkLeadDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			var notLeadingDeclaration = consolidatedDeclaration.JobDeclarations.First(x => x != consolidatedDeclaration.LeadDeclaration);
			var declaration = consolidatedDeclaration.LeadDeclaration;
			Factory.Save();
			var consolidationProvider = new JobDeclarationConsolidatedEntryProvider(declaration, new ConsolidatedEntryDeclarationRemover());
			var result = consolidationProvider.DequeueOrRemoveFromConsolidation(out var resultMessage);
			Assert("No lead declaration", !consolidatedDeclaration.JobDeclarations.Contains(declaration));
			AssertEquals("Other declaration is now lead", notLeadingDeclaration, consolidatedDeclaration.LeadDeclaration);
			Assert("Dequeue result", result);
			AssertEquals("Result message", "Job is no longer in Consolidation.", resultMessage);
		}

		public void TestRemoveFromConsolidation_NewEntryHeader()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2, DeclarationApplicationCodeList.Codes.Builtin);
			var declaration = consolidatedDeclaration.JobDeclarations.First(x => x != consolidatedDeclaration.LeadDeclaration);
			var header = declaration.Invoices.AddNew();
			header.InvoiceLines.AddNew();
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders.First();
			var consolidationProvider = new JobDeclarationConsolidatedEntryProvider(declaration, new ConsolidatedEntryDeclarationRemover());
			var result = consolidationProvider.DequeueOrRemoveFromConsolidation(out var resultMessage);
			Assert("Old entry header is deleted", entryHeader.IsDeleted);
			AssertNotNull("New entry header is created", declaration.ActiveEntryHeaders.FirstOrDefault());
			Assert("Dequeue result", result);
			AssertEquals("Result message", "Job is no longer in Consolidation.", resultMessage);
		}

		public void TestConsolidationStatus()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var consolidationHelper = new JobDeclarationConsolidatedEntryProvider(declaration);

			declaration.JE_EntryStatus = ZString.Empty;
			AssertEquals(ZString.Empty, consolidationHelper.ConsolidationStatus);

			Assert("CanConsolidateEntry", consolidationHelper.CanConsolidateEntry);
			Assert("IsQueuedForConsolidation", !consolidationHelper.IsQueuedForConsolidation);
			Assert("IsConsolidated", !consolidationHelper.IsConsolidated);

			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			AssertEquals(ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, consolidationHelper.ConsolidationStatus);

			Assert("CanConsolidateEntry", !consolidationHelper.CanConsolidateEntry);
			Assert("IsQueuedForConsolidation", consolidationHelper.IsQueuedForConsolidation);
			Assert("IsConsolidated", !consolidationHelper.IsConsolidated);

			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			AssertEquals(ConsolidatedEntryStatusList.Codes.AppliedToConsolidation, consolidationHelper.ConsolidationStatus);

			Assert("CanConsolidateEntry", !consolidationHelper.CanConsolidateEntry);
			Assert("IsQueuedForConsolidation", !consolidationHelper.IsQueuedForConsolidation);
			Assert("IsConsolidated", consolidationHelper.IsConsolidated);

			declaration.JE_EntryStatus = "ABC";
			AssertEquals("Invalid Consolidation Status code is ignored", ZString.Empty, consolidationHelper.ConsolidationStatus);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Messages.AddNew();
			Assert("CanConsolidateEntry", !consolidationHelper.CanConsolidateEntry);
			Assert("IsQueuedForConsolidation", !consolidationHelper.IsQueuedForConsolidation);
			Assert("IsConsolidated", !consolidationHelper.IsConsolidated);
		}

		public void TestQueueForConsolidation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var consolidationHelper = new JobDeclarationConsolidatedEntryProvider(declaration);

			declaration.JE_EntryStatus = ZString.Empty;
			declaration.JE_MessageStatus = ZString.Empty;
			AssertEquals(ZString.Empty, consolidationHelper.ConsolidationStatus);
			Factory.Save();

			Assert("Can queue for consolidation", consolidationHelper.QueueForConsolidation(out _));
			AssertEquals(ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, consolidationHelper.ConsolidationStatus);

			Factory.Save();
			Assert("Can de-queue from consolidation", consolidationHelper.DequeueOrRemoveFromConsolidation(out _));
			AssertEquals(ZString.Empty, consolidationHelper.ConsolidationStatus);

			declaration.JE_EntryStatus = "ABC";
			Factory.Save();

			AssertEquals("EntryStatus is not a recognised Consolidation Status", ZString.Empty, consolidationHelper.ConsolidationStatus);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Messages.AddNew();
			Assert("Cannot queue when has messages", !consolidationHelper.QueueForConsolidation(out _));
			Assert("Cannot de-queue when has messages", !consolidationHelper.DequeueOrRemoveFromConsolidation(out _));
		}

		public void TestNestingConsolidationLocks()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			jobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var consolidationHelper = new JobDeclarationConsolidatedEntryProvider(jobDeclaration);
			Factory.Save();

			using (var sqlLock1 = consolidationHelper.LockDeclarationForConsolidation())
			{
				AssertNotNull("Helper returns the lock", sqlLock1);

				AssertEquals(ZString.Empty, consolidationHelper.ConsolidationStatus);
				Assert("Can queue for consolidation", consolidationHelper.QueueForConsolidation(out _));
				AssertEquals(ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, consolidationHelper.ConsolidationStatus);
			}
		}
	}

	public class JobDeclarationConsolidatedEntryProviderAppLockTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestLockDeclarationForConsolidation()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var jobDeclaration1 = factory1.New<BaseJobDeclaration>();
			var invoice = jobDeclaration1.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			jobDeclaration1.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var consolidationHelper1 = new JobDeclarationConsolidatedEntryProvider(jobDeclaration1);
			factory1.Save();

			using (var conn2 = Db.NewExtraConnectionToMainDb())
			{
				var factory2 = new BusinessObjectFactory(conn2) { RefreshEnabled = false };
				var jobDeclaration2 = factory2.Load<BaseJobDeclaration>(jobDeclaration1.PK);
				jobDeclaration2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				var consolidationHelper2 = new JobDeclarationConsolidatedEntryProvider(jobDeclaration2);

				using (var sqlLock1 = consolidationHelper1.LockDeclarationForConsolidation())
				{
					AssertNotNull("Helper 1 has the lock", sqlLock1);

					using (var sqlLock2 = consolidationHelper2.LockDeclarationForConsolidation())
					{
						AssertNull("Helper 2 cannot lock", sqlLock2);
						Assert("Helper 2 cannot queue for consolidation", !consolidationHelper2.QueueForConsolidation(out _));
					}

					Assert("Helper 1 can queue for consolidation", consolidationHelper1.QueueForConsolidation(out _));
					factory1.Save();

					using (var sqlLock2b = consolidationHelper2.LockDeclarationForConsolidation())
					{
						AssertNull("Helper 2 still cannot lock after nested inner lock was disposed", sqlLock2b);
						Assert("Helper 2 still cannot queue for consolidation", !consolidationHelper2.QueueForConsolidation(out _));
					}

					Assert("Helper 1 is queued for consolidation", consolidationHelper1.IsQueuedForConsolidation);
					Assert("Helper 2 is NOT queued for consolidation", !consolidationHelper2.IsQueuedForConsolidation);
				}

				using (var sqlLock3 = consolidationHelper2.LockDeclarationForConsolidation())
				{
					AssertNotNull("Dec2 has the lock", sqlLock3);
					Assert("Dec2 EntryStatus is reloaded from DB and Helper 2 is now marked as queued for consolidation", consolidationHelper2.IsQueuedForConsolidation);
				}
			}
		}
	}
}
