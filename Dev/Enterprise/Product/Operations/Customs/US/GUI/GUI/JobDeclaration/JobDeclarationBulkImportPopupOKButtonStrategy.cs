using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	public class JobDeclarationBulkImportPopupOKButtonStrategy : IEmbeddedModulePopupOKButtonStrategy
	{
		public JobDeclarationBulkImportPopupOKButtonStrategy(EmbeddedModulePopup popup, JobDeclaration declaration)
		{
			this.popup = popup;
			this.declaration = declaration;
		}

		readonly EmbeddedModulePopup popup;
		readonly JobDeclaration declaration;

		public IModuleDecisionProvider ModuleDecisionProvider
		{
			get { return provider ?? (provider = new JobDeclarationBulkImportModuleDecisionProvider(this)); }
		}
		IModuleDecisionProvider provider;

		#region ModuleDecisionProvider class

		class JobDeclarationBulkImportModuleDecisionProvider : IModuleDecisionProvider
		{
			public JobDeclarationBulkImportModuleDecisionProvider(JobDeclarationBulkImportPopupOKButtonStrategy strategy)
			{
				this.strategy = strategy;
			}
			readonly JobDeclarationBulkImportPopupOKButtonStrategy strategy;

			#region IModuleDecisionProvider Members

			public bool EnablePreviousNextSupport
			{
				get { return true; }
			}

			public bool ShouldLoadFilterBizObj
			{
				get { return true; }
			}

			public bool ShouldSaveFilterBizObj
			{
				get { return true; }
			}

			public bool ShouldDisplayNotifications
			{
				get { return false; }
			}

			public bool ShouldIgnoreAdditionalFilter
			{
				get { return false; }
			}

			public bool AllowExcelExport
			{
				get { return true; }
			}

			public IBusinessObjectCollection List
			{
				get { return null; }
			}

			public void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				HandleFindBoxOKButton(selectedBusinessObjects);
			}

			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
			{
				strategy.HandleFindBoxOKButton(selectedBusinessObjects);
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
			}

			public void InitialiseFindBoxControllerLink(ZController controller)
			{
			}

			public void SetFindBoxCodeDescription(BusinessObject bizo)
			{
			}

			#endregion
		}
		#endregion

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects == null || selectedBusinessObjects.Length == 0)
			{
				Globals.Message.ShowInformation(BulkImportPopupConstants.SelectOneDeclaration);
			}
			else
			{
				popup.Close();

				int importCountExpected = 0;
				int ignoredNoEntryNumber = 0;
				var declarationsForReleaseEntries = new JobDeclarationCollection(declaration.Factory, GlbCompany.CurrentCompany.PK);
				foreach (JobDeclaration declarationToImport in selectedBusinessObjects)
				{
					if (!declarationToImport.EntryFilerCode.IsEmpty && !declarationToImport.ImportEntryNumber.IsEmpty)
					{
						declarationsForReleaseEntries.Add(declarationToImport);
						importCountExpected++;
					}
					else
					{
						ignoredNoEntryNumber++;
					}
				}
				var importedCount = new ReleaseEntryInvoiceRetriever(declaration).ImportDeclarations(declarationsForReleaseEntries);
				var ignoredCount = importCountExpected - importedCount;

				var decsImportedMsg = importedCount == 0 ? BulkImportPopupConstants.NoDeclarationsImported : importedCount == 1 ? BulkImportPopupConstants.DeclarationHasBeenImported : importedCount + BulkImportPopupConstants.MultipleDeclarationsHaveBeenImported;
				var informationBuilder = new ZStringBuilder(decsImportedMsg);
				if (ignoredCount > 0)
				{
					var decsIgnoredMsg = ignoredCount == 1 ? BulkImportPopupConstants.DeclarationHasBeenIgnored : ignoredCount + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnored;
					informationBuilder.Append(decsIgnoredMsg);
				}
				if (ignoredNoEntryNumber > 0)
				{
					var noEntryDecsMsg = ignoredNoEntryNumber == 1 ? BulkImportPopupConstants.DeclarationHasBeenIgnoredNoEntryNumber : ignoredNoEntryNumber + BulkImportPopupConstants.MultipleDeclarationsHaveBeenIgnoredNoEntryNumber;
					informationBuilder.Append(noEntryDecsMsg);
				}
				Globals.Message.ShowInformation(informationBuilder.ToStringWithNewLineBetweenAppends(), "Bulk Import");
			}
		}
	}
}
