using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Test.Declaration
{
	sealed class NZEDIMenuForConsolidatedDeclarationTest : TestCaseWithFactory
	{
		public void TestSubmit()
		{
			var declaration = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().Last().JI_HadErrorInLastResponse = true;
			Factory.Save();
			consolidatedDeclaration.CRD_JobReferenceNumber = "CRDRef123";
			Factory.Save();
			using (ZForm parentForm = new ConsolidatedDeclarationForm(consolidatedDeclaration, new ConsolidatedDeclarationFormAdaptationsProvider()))
			{
				var menu = new NZEDIMenu();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				parentForm.Menu.MenuItems.Add(menu);
				UnitTestUserNotification.Instance.AddYesAnswer();
				menu.MenuItems.FindByText("Submit Job").PerformClick();
			}

			consolidatedDeclaration = new BusinessObjectFactory().Load<ConsolidatedDeclaration>(consolidatedDeclaration.PK);
			CombineAssertions(() =>
			{
				AssertEquals("JE_EntryStatus", true, consolidatedDeclaration.JobDeclarations.All(dec => dec.JE_EntryStatus == FormalEntryStatusList.Codes.SentToCustoms));
				AssertEquals("JI_HadErrorInLastResponse", true, consolidatedDeclaration.JobDeclarations.SelectMany(dec => dec.InvoiceLines.Select(li => (li as JobComInvoiceLine).JI_HadErrorInLastResponse)).All(b => !b));
				AssertEquals("JE_EDITransmitDate", true, consolidatedDeclaration.JobDeclarations.All(dec => (dec as JobDeclaration).JE_EDITransmitDate.Date == declaration.CachedTodaysDate.Date));
				AssertEquals("Message submitted", 1, consolidatedDeclaration.Messages.Count);
				AssertEquals("Message is consolidated", true, consolidatedDeclaration.Messages[0].EM_MessageText.Contains("CRDRef123"));
				AssertEquals("TSWCombinedStatus", true, consolidatedDeclaration.JobDeclarations.All(dec => (dec as JobDeclaration).JE_TSWCombinedStatus == TSWEntryStatusList.Codes.STC));
				AssertEquals("MessageStatus", true, consolidatedDeclaration.JobDeclarations.All(dec => (dec as JobDeclaration).JE_MessageStatus == ""));
				AssertEquals("Job number is not changed", declaration.JE_DeclarationReference, consolidatedDeclaration.LeadDeclaration.JE_DeclarationReference);
			});
		}

		public void TestCancel()
		{
			var declaration = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().Last().JI_HadErrorInLastResponse = true;
			Factory.Save();
			using (consolidatedDeclaration.SuspendSettingHasChanges())
			{
				consolidatedDeclaration.CRD_JobReferenceNumber = "CRDRef123";
				declaration.DeclarationNumber = "12345678";
			}

			using (ZForm parentForm = new ConsolidatedDeclarationForm(consolidatedDeclaration, new ConsolidatedDeclarationFormAdaptationsProvider()))
			{
				var menu = new NZEDIMenu();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				parentForm.Menu.MenuItems.Add(menu);
				menu.MenuItems.FindByText("Cancel Job").PerformClick();
			}

			consolidatedDeclaration = new BusinessObjectFactory().Load<ConsolidatedDeclaration>(consolidatedDeclaration.PK);
			CombineAssertions(() =>
			{
				AssertEquals("JE_EntryStatus", true, consolidatedDeclaration.JobDeclarations.All(dec => dec.JE_EntryStatus == FormalEntryStatusList.Codes.SentToCustoms));
				AssertEquals("JI_HadErrorInLastResponse", true, consolidatedDeclaration.JobDeclarations.SelectMany(dec => dec.InvoiceLines.Select(li => (li as JobComInvoiceLine).JI_HadErrorInLastResponse)).All(b => !b));
				AssertEquals("JE_EDITransmitDate", true, consolidatedDeclaration.JobDeclarations.All(dec => (dec as JobDeclaration).JE_EDITransmitDate.Date == declaration.CachedTodaysDate.Date));
				AssertEquals("Message submitted", 1, consolidatedDeclaration.Messages.Count);
				AssertEquals("Message is consolidated", true, consolidatedDeclaration.Messages[0].EM_MessageText.Contains("CRDRef123"));
				AssertEquals("TSWCombinedStatus", true, consolidatedDeclaration.JobDeclarations.All(dec => (dec as JobDeclaration).JE_TSWCombinedStatus == TSWEntryStatusList.Codes.DCP));
				AssertEquals("MessageStatus", true, consolidatedDeclaration.JobDeclarations.All(dec => (dec as JobDeclaration).JE_MessageStatus == ""));
				AssertEquals("Job number is not changed", declaration.JE_DeclarationReference, consolidatedDeclaration.LeadDeclaration.JE_DeclarationReference);
			});
		}

		public void TestReset()
		{
			var declaration = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().Last().JI_HadErrorInLastResponse = true;
			consolidatedDeclaration.JobDeclarations.ForEach(dec => { (dec as JobDeclaration).JE_EDITransmitDate = ZDate.Today; });
			Factory.Save();
			consolidatedDeclaration.CRD_JobReferenceNumber = "CRDRef123";
			var message = Factory.New<TSWMessage>();
			consolidatedDeclaration.Messages.Add(message);
			message.IsTransmitMessage = true;
			message.EM_Status = NZCMessage.Status.Queued;
			declaration.DeclarationNumber = "12345678";
			Factory.Save();
			var originalEntryHeaderPKs = consolidatedDeclaration.JobDeclarations.Select(x => x.ActiveEntryHeaders[0].PK).ToArray();
			using (ZForm parentForm = new ConsolidatedDeclarationForm(consolidatedDeclaration, new ConsolidatedDeclarationFormAdaptationsProvider()))
			{
				var menu = new NZEDIMenu();
				menu.ConsolidatedDeclaration = consolidatedDeclaration;
				parentForm.Menu.MenuItems.Add(menu);
				menu.MenuItems.FindByText("Reset to Original").PerformClick();
			}

			consolidatedDeclaration = new BusinessObjectFactory().Load<ConsolidatedDeclaration>(consolidatedDeclaration.PK);
			CombineAssertions(() =>
			{
				AssertEquals("JE_EntryStatus", true, consolidatedDeclaration.JobDeclarations.All(dec => dec.JE_EntryStatus == FormalEntryStatusList.Codes.NotSentToCustoms));
				AssertEquals("CH_IsActive should all be false", false, originalEntryHeaderPKs.Any(pk => consolidatedDeclaration.Factory.Load<CusEntryHeader>(pk).CH_IsActive));
				AssertEquals("JE_EDITransmitDate", true, consolidatedDeclaration.JobDeclarations.All(dec => (dec as JobDeclaration).JE_EDITransmitDate.IsEmpty));
				AssertEquals("No new message submitted", 1, consolidatedDeclaration.Messages.Count);
				AssertEquals("Message is cancelled", NZCMessage.Status.Cancelled, consolidatedDeclaration.Messages[0].EM_Status);
				AssertEquals("TSWCombinedStatus", true, consolidatedDeclaration.JobDeclarations.All(dec => (dec as JobDeclaration).JE_TSWCombinedStatus == ""));
				AssertEquals("Job number is not changed", declaration.JE_DeclarationReference, consolidatedDeclaration.LeadDeclaration.JE_DeclarationReference);
			});
		}

		#region Implementation
		ConsolidatedDeclaration consolidatedDeclaration;

		protected override void SetUp()
		{
			base.SetUp();
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Common.ChargeDistributeByList.Codes.Value);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Business.Testing.TestHelper.SetupMessagingEnvironment();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "00009917B");
			RawDataRegistry.Instance.EnableConsolidatedEntries.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var declarant = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = declarant.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			wrapper.NZBPassword.GP_CurrentPassword = new TwoWayEncoder(declarant.PK.ToGuid()).Encrypt("NN12WW");
			declarant.GS_EmailAddress = "staff.user@company.org";

			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<TestConsolidatedDeclaration>(Factory, 2);
			foreach (var dec in consolidatedDeclaration.JobDeclarations)
			{
				dec.Invoices.AddNew().InvoiceLines.AddNew().JI_LinePrice = 100m;
				dec.Invoices[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "NZD");
				dec.ResumeApportionment();
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				dec.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
				dec.ActiveEntryHeaders[0].CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
				dec.GrossWeight = new ZArchitecture.ZWeight(100m, "KG");
				dec.Volume = new ZArchitecture.ZVolume(200m, "");
				dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}
		IDisposable distributeByForExport;

		class TestConsolidatedDeclaration : ConsolidatedDeclaration
		{
			public TestConsolidatedDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override Customs.Business.IConsolidatedJobDeclarationCollection<Customs.Business.BaseJobDeclaration> CreateNewJobDeclarationCollection()
			{
				return new Customs.Business.ConsolidatedJobDeclarationCollection<AggregateJobDeclarationNoMerging>(this);
			}

			class AggregateJobDeclarationNoMerging : JobDeclaration
			{
				public AggregateJobDeclarationNoMerging(BusinessObjectFactory factory, DataRow row) : base(factory, row)
				{
				}

				protected override bool DoMergeCore(Customs.Business.ISendsMessagesToCustoms notifier)
				{
					if (Factory is ReadOnlyBusinessObjectFactory)
					{
						throw new Exception("Aggregate declaration must not be merged!");
					}
					else
					{
						return base.DoMergeCore(notifier);
					}
				}
			}
		}
		#endregion
	}
}
