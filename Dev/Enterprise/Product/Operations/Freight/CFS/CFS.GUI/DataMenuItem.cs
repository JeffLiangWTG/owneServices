using System;
using System.Windows.Forms;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.Business.Data;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public class DataMenuItem : ZMenuItem
	{
		public DataMenuItem()
			: base(ResString.GetMultilingualString("Freight.CFS.DataMenuItem", "&Data"))
		{
			MenuItem importPackLine = new ZMenuItem(ResString.GetMultilingualString("Freight.CFS.DataMenuItem.ImportPackingData", "Import Packing Data"), new EventHandler(ImportCSVMenuItem_Clicked));
			MenuItems.Add(0, importPackLine);
		}

		public CFSShipment Shipment
		{
			get { return fShipment; }
			set
			{
				if (fShipment != value)
				{
					fShipment = value;
				}
			}
		}

		#region Implementation

		#region Data Importer User Interface Interaction

		protected void ImportCSVMenuItem_Clicked(object sender, EventArgs e)
		{
			if (!Shipment.JS_UniqueConsignRef.IsEmpty)
			{
				ImporterForm = new DataTransferForm();
				ImporterForm.DialogFilter = (NoResString)"Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|Excel files (*.xls)|*.xls|All files (*.*)|*.*";
				ImporterForm.Text = Res.GetString("5281f298-8a56-498c-a06e-805d9100e662", "Import Pack Lines");
				ImporterForm.StartProcess += new ProcessFileEventHandler(ImporterForm_StartProcess);
				ImporterForm.ProcessCancelled += new EventHandler(ImporterForm_Cancelled);
				ZFormModaliser.ShowDialogAndDispose(ImporterForm);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("72fb01b6-67dd-472b-bda6-e3f9d8c5bc0e", "Please create a new shipment before importing pack lines."), Res.GetString("ea50ea65-75b3-43c7-94bf-4f995bd8b255", "Import"));
			}
		}

		protected void DataImporter_FileRowProcessed(object sender, ProcessedEventArgs e)
		{
			ImporterForm.SetProcessProgress(e.PercentageComplete, e.ProcessedCount, e.FailureCount, e.LogEntry);
		}

		protected void DataImporter_ProcessCompleted(object sender, EventArgs e)
		{
			ImporterForm.FinishProcess();
		}

		protected void ImporterForm_Cancelled(object sender, EventArgs e)
		{
			DataImporter.CancelImport();
		}

		protected void ImporterForm_StartProcess(object sender, ProcessFileEventArgs e)
		{
			if (!ImporterEventsAttached)
			{
				DataImporter.Processed += new ProcessedEventHandler(DataImporter_FileRowProcessed);
				DataImporter.ProcessCompleted += new EventHandler(DataImporter_ProcessCompleted);
				ImporterEventsAttached = true;
			}

			string fileName = e.UnmappedFileName;
			using (ZOpenFileDialog.ForceLocalFile(ref fileName))
			{
				if (!DataImporter.ImportPackLinesFromFile(fileName))
				{
					Globals.Message.ShowError(DataImporter.ErrorMessage, Res.GetString("1404d0c7-630d-4a38-888a-5147c3b619bd", "Error"));
					ImporterForm.Close();
				}
			}
		}

		protected virtual DataImporter DataImporter
		{
			get
			{
				if (fDataImporter == null)
				{
					fDataImporter = new DataImporter(Shipment);
				}

				return fDataImporter;
			}
		}

		bool ImporterEventsAttached;
		DataImporter fDataImporter;
		protected DataTransferForm ImporterForm;

		#endregion

		protected CFSShipment fShipment;

		#endregion
	}
}
