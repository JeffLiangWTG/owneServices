using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class JobDeclarationBulkImportPopupOKButtonStrategyTest : TestCaseWithFactory
	{
		public void TestHandleFindBoxOKButton()
		{
			var declarationToAdd1 = Factory.New<JobDeclaration>();
			declarationToAdd1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationToAdd1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declarationToAdd1.US_EnableENS = true;
			declarationToAdd1.US_EntryFilerCode = "XJ5";
			var invoice1 = declarationToAdd1.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JobComInvoiceLines.AddNew();
			declarationToAdd1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var declarationToAdd2 = Factory.New<JobDeclaration>();
			declarationToAdd2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationToAdd2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declarationToAdd2.US_EnableENS = true;
			declarationToAdd2.US_EntryFilerCode = "XJ5";
			var invoice2 = declarationToAdd2.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JobComInvoiceLines.AddNew();
			declarationToAdd2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var declarationToAdd3 = Factory.New<JobDeclaration>();
			declarationToAdd3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationToAdd3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declarationToAdd3.US_EnableENS = true;
			declarationToAdd3.US_EntryFilerCode = "XJ5";
			var invoice3 = declarationToAdd3.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "INV3";
			invoice3.JobComInvoiceLines.AddNew();
			declarationToAdd3.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var declarationToAdd4 = Factory.New<JobDeclaration>();
			declarationToAdd4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationToAdd4.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declarationToAdd4.US_EnableENS = true;
			declarationToAdd4.US_EntryFilerCode = "XJ5";
			var invoice4 = declarationToAdd4.Invoices.AddNew();
			invoice4.JZ_InvoiceNumber = "INV4";
			invoice4.JobComInvoiceLines.AddNew();
			var declarationToAdd5 = Factory.New<JobDeclaration>();
			declarationToAdd5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationToAdd5.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declarationToAdd5.US_EnableENS = true;
			var invoice5 = declarationToAdd5.Invoices.AddNew();
			invoice5.JZ_InvoiceNumber = "INV5";
			invoice5.JobComInvoiceLines.AddNew();
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			using (var declarationModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration, Core.Constants.CountryCodes.UnitedStates))
			{
				var grid = (ZDisplayGrid)declarationModule.DisplayGrid;
				using (var popup = new EmbeddedModulePopup(declarationModule))
				{
					var strategy = new JobDeclarationBulkImportPopupOKButtonStrategy(popup, declaration);
					popup.EmbeddedModulePopupOKButtonStrategy = strategy;
					popup.Show();
					var moduleColl = (Customs.Business.BaseJobDeclarationCollection)grid.List;
					moduleColl.Add(declarationToAdd1);
					moduleColl.Add(declarationToAdd2);
					moduleColl.Add(declarationToAdd3);
					moduleColl.Add(declarationToAdd4);
					moduleColl.Add(declarationToAdd5);
					grid.Select(0);
					grid.Select(2);
					grid.Select(3);
					grid.Select(4);
					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("2 invoices have been imported", 2, declaration.Invoices.Count);
					AssertEquals("2 invoice lines have been imported", 2, declaration.InvoiceLines.Count);
					AssertEquals("2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenImported + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnoredNoEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals("No invoice has been imported", 2, declaration.Invoices.Count);
					AssertEquals("No invoice line has been imported", 2, declaration.InvoiceLines.Count);
					AssertEquals(BulkImportPopupConstants.NoDeclarationsImported + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnored + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnoredNoEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					strategy.ModuleDecisionProvider.HandleFindBoxOKButton(moduleColl.ToArray());
					AssertEquals("1 invoice has been imported", 3, declaration.Invoices.Count);
					AssertEquals("1 invoice line has been imported", 3, declaration.InvoiceLines.Count);
					AssertEquals(BulkImportPopupConstants.DeclarationHasBeenImported + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnored + System.Environment.NewLine + "2" + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnoredNoEntryNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}
	}
}
