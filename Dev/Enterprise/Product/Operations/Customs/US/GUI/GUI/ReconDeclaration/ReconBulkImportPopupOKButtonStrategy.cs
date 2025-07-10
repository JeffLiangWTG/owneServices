using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	public class ReconBulkImportPopupOKButtonStrategy : IEmbeddedModulePopupOKButtonStrategy
	{
		public ReconBulkImportPopupOKButtonStrategy(EmbeddedModulePopup popup, ReconDeclaration reconDeclaration)
		{
			this.popup = popup;
			this.reconDeclaration = reconDeclaration;
		}

		readonly EmbeddedModulePopup popup;
		readonly ReconDeclaration reconDeclaration;

		public IModuleDecisionProvider ModuleDecisionProvider
		{
			get { return provider ?? (provider = new ReconModuleDecisionProvider(this)); }
		}
		IModuleDecisionProvider provider;

		#region ModuleDecisionProvider class

		public class ReconModuleDecisionProvider : IModuleDecisionProvider
		{
			public ReconModuleDecisionProvider(ReconBulkImportPopupOKButtonStrategy strategy)
			{
				this.strategy = strategy;
			}
			readonly ReconBulkImportPopupOKButtonStrategy strategy;

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

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects == null || selectedBusinessObjects.Length == 0)
			{
				Globals.Message.ShowInformation(BulkImportPopupConstants.SelectOneDeclaration);
			}
			else
			{
				popup.Close();

				int importCount = 0;
				int ignoredCount = 0;
				int ignoredNoEntryNumber = 0;

				using (reconDeclaration.SuspendOriginalDeclarationLoad())
				{
					foreach (JobDeclaration declaration in selectedBusinessObjects)
					{
						if (!declaration.EntryFilerCode.IsEmpty && !declaration.ImportEntryNumber.IsEmpty)
						{
							ZString entryFilerAndEntryNumber = declaration.EntryFilerCode + declaration.ImportEntryNumber;
							if (reconDeclaration.OriginalEntries.FindEntryBy(entryFilerAndEntryNumber) == null)
							{
								importCount++;
								ReconOriginalEntryHeader entryHeader = reconDeclaration.OriginalEntries.AddNew();
								using (entryHeader.GetValidationSuspender())
								{
									entryHeader.CH_OrigEntryReference = declaration.EntryFilerCode + declaration.ImportEntryNumber;
								}
							}
							else
							{
								ignoredCount++;
							}
						}
						else
						{
							ignoredNoEntryNumber++;
						}
					}
					reconDeclaration.ReleaseReadFactory();
				}

				var decsImportedMsg = importCount == 0 ? BulkImportPopupConstants.NoDeclarationsImported : importCount == 1 ? BulkImportPopupConstants.DeclarationHasBeenImported : importCount + BulkImportPopupConstants.MultipleDeclarationsHaveBeenImported;
				ZStringBuilder informationBuilder = new ZStringBuilder(decsImportedMsg);
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

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}
	}
}
