using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	public class DrawbackBulkImportPopupOKButtonStrategy : IEmbeddedModulePopupOKButtonStrategy
	{
		public DrawbackBulkImportPopupOKButtonStrategy(JobDeclaration drawback)
		{
			this.drawback = drawback;
		}
		readonly JobDeclaration drawback;

		public IModuleDecisionProvider ModuleDecisionProvider
		{
			get { return provider ?? (provider = new DrawbackBulkImportModuleDecisionProvider(this)); }
		}
		IModuleDecisionProvider provider;

		#region ModuleDecisionProvider class

		class DrawbackBulkImportModuleDecisionProvider : IModuleDecisionProvider
		{
			public DrawbackBulkImportModuleDecisionProvider(DrawbackBulkImportPopupOKButtonStrategy strategy)
			{
				this.strategy = strategy;
			}
			readonly DrawbackBulkImportPopupOKButtonStrategy strategy;

			public bool AllowExcelExport
			{
				get { return true; }
			}

			public bool EnablePreviousNextSupport
			{
				get { return true; }
			}

			public void HandleDefaultAction(BusinessObject[] selectedBusinessObject)
			{
				HandleFindBoxOKButton(selectedBusinessObject);
			}

			public void InitialiseFindBoxControllerLink(ZController controller)
			{
			}

			public IBusinessObjectCollection List
			{
				get { return null; }
			}

			public void SetFindBoxCodeDescription(BusinessObject bizo)
			{
			}

			public bool ShouldDisplayNotifications
			{
				get { return false; }
			}

			public bool ShouldIgnoreAdditionalFilter
			{
				get { return false; }
			}

			public bool ShouldLoadFilterBizObj
			{
				get { return true; }
			}

			public bool ShouldSaveFilterBizObj
			{
				get { return true; }
			}

			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
			{
				strategy.HandleFindBoxOKButton(selectedBusinessObject);
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
			}
		}

		#endregion

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
		{
			if (selectedBusinessObject != null && selectedBusinessObject.Length > 0)
			{
				OnOKButtonClick(null);
				var ignoredLines = 0;
				var importedLines = 0;
				var drawbackInvoiceLines = drawback.FilteredInvoiceLines.Cast<JobComInvoiceLine>();
				foreach (CusEntryLine cusLine in selectedBusinessObject)
				{
					var header = cusLine.Header;
					if (!drawbackInvoiceLines.Any(x => x.US_ImportEntryNo + "-" + x.US_DRWImportEntryLine == cusLine.UniqueKey))
					{
						importedLines++;
						JobComInvoiceLine newDrawbackLine;
						if (drawback.Invoices.Count > 0)
						{
							newDrawbackLine = drawback.FilteredInvoiceLines.AddNew();
						}
						else
						{
							var invoice = drawback.Invoices.AddNew();
							newDrawbackLine = invoice.InvoiceLines.AddNew();
						}

						using (newDrawbackLine.GetValidationSuspender())
						{
							newDrawbackLine.US_DRWIsForImportSection = true;
							newDrawbackLine.US_ImportEntryNo = cusLine.UniqueKey;
						}
					}
					else
					{
						ignoredLines++;
					}
				}
				drawback.InvoiceLines.Load();

				Globals.Message.ShowInformation(string.Format(NoOfLinesImportedAndIgnored,
					importedLines, ignoredLines), "Bulk Import");
			}
			else
			{
				Globals.Message.ShowInformation(SelectAtLeaseOneLine);
			}
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}

		internal const string NoOfLinesImportedAndIgnored = "Imported {0} and ignored {1} line(s) which are already on the declaration.";
		internal const string SelectAtLeaseOneLine = "Please select at least one line.";

		public event EventHandler OKButtonClick;

		void OnOKButtonClick(EventArgs e)
		{
			if (OKButtonClick != null)
			{
				OKButtonClick(this, e);
			}
		}
	}
}
