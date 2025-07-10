using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ReconBulkImportPopupOKButtonStrategyTest : TestCaseWithFactory
	{
		public void TestHandleFindBoxOKButton()
		{
			JobDeclaration declaration1 = GetImportMergedDeclaration();
			JobDeclaration declaration2 = GetImportMergedDeclaration();
			JobDeclaration declaration3 = GetImportMergedDeclaration();
			JobDeclaration declaration4 = GetImportMergedDeclaration();
			JobDeclaration declaration5 = GetImportMergedDeclaration();
			Factory.Save();
			declaration4.ImportEntryNumber = ZString.Empty;
			declaration5.US_EntryFilerCode = ZString.Empty;
			Factory.Save();
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			using (ZFilterGridModule declarationModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration, Core.Constants.CountryCodes.UnitedStates))
			{
				ZDisplayGrid grid = (ZDisplayGrid)declarationModule.DisplayGrid;
				using (EmbeddedModulePopup popup = new EmbeddedModulePopup(declarationModule))
				{
					ReconBulkImportPopupOKButtonStrategy strategy = new ReconBulkImportPopupOKButtonStrategy(popup, reconDeclaration);
					popup.EmbeddedModulePopupOKButtonStrategy = strategy;
					popup.Show();
					Customs.Business.BaseJobDeclarationCollection moduleColl = (Customs.Business.BaseJobDeclarationCollection)grid.List;
					moduleColl.Add(declaration1);
					moduleColl.Add(declaration2);
					moduleColl.Add(declaration3);
					moduleColl.Add(declaration4);
					moduleColl.Add(declaration5);
					grid.Select(0);
					grid.Select(2);
					grid.Select(3);
					grid.Select(4);
					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("2 original entries created", 2, reconDeclaration.OriginalEntries.Count);
					AssertEquals("2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenImported + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnoredNoEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("2 original entries created", 2, reconDeclaration.OriginalEntries.Count);
					AssertEquals(BulkImportPopupConstants.NoDeclarationsImported + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnored + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnoredNoEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					strategy.ModuleDecisionProvider.HandleFindBoxOKButton(moduleColl.ToArray());
					AssertEquals("3 original entries created", 3, reconDeclaration.OriginalEntries.Count);
					AssertEquals(BulkImportPopupConstants.DeclarationHasBeenImported + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnored + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnoredNoEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandleFindBoxOKButton_CopyPaymentDueDate()
		{
			var declaration = GetImportMergedDeclaration();
			declaration.US_PaymentDueDate = new ZDateTime(2018, 8, 15);
			Factory.Save();
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			using (var declarationModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration, Core.Constants.CountryCodes.UnitedStates))
			{
				var grid = (ZDisplayGrid)declarationModule.DisplayGrid;
				using (var popup = new EmbeddedModulePopup(declarationModule))
				{
					var strategy = new ReconBulkImportPopupOKButtonStrategy(popup, reconDeclaration);
					popup.EmbeddedModulePopupOKButtonStrategy = strategy;
					popup.Show();
					var moduleCollection = (Customs.Business.BaseJobDeclarationCollection)grid.List;
					moduleCollection.Add(declaration);
					grid.Select(0);
					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("1 original entries created", 1, reconDeclaration.OriginalEntries.Count);
					AssertEquals("Payment Due Date would be copied from the declaration's Due Date", new ZDateTime(2018, 8, 15), reconDeclaration.OriginalEntries[0].US_PaymentDate);
				}
			}
		}

		JobDeclaration GetImportMergedDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}
	}
}
