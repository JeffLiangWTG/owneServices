using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class ImportCustomsExchangeRatesForm : ImportFromCSVForm
	{
		public override string FormHeading => Res.GetString("ImportCustomsExchangeRatesForm|44ACC569-D895-42B9-A321-95B7B536152E", "Import SG Customs Currency Exchange Rates Data");

		public override string FileDialogFilter
		{
			get
			{
				{ return "Excel files (*.xls)|*.xls|All files (*.*)|*.*"; }
			}
		}

		public override void LoadSpecificDataType(string dataToLoad)
		{
			RefExchangeRateDataImport dataImporter = new RefExchangeRateDataImport();
			dataImporter.ProgressChanged += new ProgressChangedEventHandler(DataImporter_ProgressChanged);
			dataImporter.LogUpdated += new LogUpdatedEventHandler(DataImporter_LogUpdated);
			dataImporter.ImportData(dataToLoad);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void DataImporter_ProgressChanged(RefExchangeRateDataImport sender, ProgressChangedEventArgs e)
		{
			ProgressBar.Maximum = e.RecordsImporting;
			ProgressBar.Value = e.CurrentRow;
			Application.DoEvents();
		}

		#region Logging

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void DataImporter_LogUpdated(RefExchangeRateDataImport sender, LogUpdatedEventArgs e)
		{
			string logText = e.LogMessage;

			if (logText.StartsWith("\r\n"))
			{
				OutputListBox.Items.Add("");
				logText = logText.Remove(0, 2);
			}

			if (logText.EndsWith("\r\n"))
			{
				logText = logText.Replace("\r\n", "");
				OutputListBox.Items.Add(logText);
				OutputListBox.Items.Add("");
			}
			else
			{
				OutputListBox.Items.Add(logText);
			}

			Application.DoEvents();
		}

		#endregion
	}
}
