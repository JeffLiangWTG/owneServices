using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class InvoiceImporter
	{
		public InvoiceImporter(BaseJobDeclaration declaration, DataTransferImpl dataTransferImpl)
		{
			Declaration = declaration;
			DataTransferImpl = dataTransferImpl;
		}

		public void ShowInvoiceImporterForm()
		{
			if (ImporterForm == null || ImporterForm.IsDisposed)
			{
				ImporterForm = new InvoiceImporterForm();
				ImporterForm.DialogFilter = (NoResString)"Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|Excel files (*.xls)|*.xls|Xml files (*.xml)|*.xml|All files (*.*)|*.*";
				ImporterForm.StartProcess += new ProcessFileEventHandler(ImporterForm_StartProcess);
				ImporterForm.ProcessCancelled += new EventHandler(ImporterForm_Cancelled);
			}

			ZFormModaliser.ShowDialogAndDispose(ImporterForm);
		}

		#region Event Handlers

		protected internal void DataTransferImpl_FileRowProcessed(object sender, ProcessedEventArgs e)
		{
			if (ImporterForm != null && (e.ProcessedCount % 50 == 0 || e.PercentageComplete == 100))
			{
				ImporterForm.SetProcessProgress(e.PercentageComplete, e.ProcessedCount, e.FailureCount, e.LogEntry);
			}
		}

		protected internal void DataTransferImpl_ProcessCompleted(object sender, EventArgs e)
		{
			ImporterForm?.FinishProcess();
		}

		protected void DataTransferImpl_UnknownOrganisationCodeFound(object sender, UnknownOrganisationCodeEventArgs e)
		{
			if (ImporterForm == null || ImporterForm.SkipUnknownSupplierRecords)
			{
				return;
			}

			var tempCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
			var supplierFinder = new OrganisationFinder(e, Declaration.Factory);
			supplierFinder.UnknownOrg.OH_IsConsignor = true;
			supplierFinder.FindSimilarOrganisations();
			using (var popUp = new FindSimilarSupplierForm(supplierFinder))
			{
				ZFormModaliser.ShowDialogWithoutDispose(popUp);
				e.Code = popUp.OrganisationCode;
				ImporterForm.SkipUnknownSupplierRecords = popUp.SkipAll;
			}
			Cursor.Current = tempCursor;
		}

		protected void ImporterForm_Cancelled(object sender, EventArgs e)
		{
			DataTransferImpl.CancelImport();
		}

		protected internal void ImporterForm_StartProcess(object sender, ProcessFileEventArgs e)
		{
			if (ImporterForm != null)
			{
				if (!importerEventsAttached)
				{
					DataTransferImpl.Processed += new ProcessedEventHandler(DataTransferImpl_FileRowProcessed);
					DataTransferImpl.ProcessCompleted += new EventHandler(DataTransferImpl_ProcessCompleted);
					DataTransferImpl.UnknownOrganisationCodeFound += new UnknownOrganisationCodeEventHandler(DataTransferImpl_UnknownOrganisationCodeFound);
					importerEventsAttached = true;
				}

				string fileName = e.UnmappedFileName;
				using (ZOpenFileDialog.ForceLocalFile(ref fileName))
				{
					if (!DataTransferImpl.ImportInvoices(Declaration, fileName))
					{
						Globals.Message.ShowError(DataTransferImpl.ErrorMessage, Res.GetString("ccd0ff59-06c1-47aa-ad13-8715bb2e1307", "Error"));
						ImporterForm.Close();
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected internal InvoiceImporterForm ImporterForm { get; private set; }

		protected internal readonly BaseJobDeclaration Declaration;
		protected internal bool importerEventsAttached;
		protected internal readonly DataTransferImpl DataTransferImpl;

		#endregion

	}
}
