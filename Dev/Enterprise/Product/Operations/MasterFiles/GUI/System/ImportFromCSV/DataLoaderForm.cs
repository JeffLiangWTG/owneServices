using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class DataLoaderForm : ImportFromCSVForm
	{
		public DataLoaderForm()
			: base()
		{
		}

		public DataLoaderForm(NonPersistentBusinessObject business)
			: base(business)
		{
		}

		#region Template

		protected override bool CanDownloadTemplate
		{
			get
			{
				var dataLoader = GetNewDataLoader();
				return dataLoader.HasCSVTemplateHeading;
			}
		}

		protected override bool DownloadTemplate(Stream stream)
		{
			var dataLoader = GetNewDataLoader();
			dataLoader.WriteCSVTemplate(stream);

			return true;
		}

		#endregion

		#region Import Data

		public override void LoadSpecificDataType(string dataToLoad)
		{
			var dataLoader = GetNewDataLoader();
			dataLoader.ProgressChanged += DataLoader_ProgressChanged;
			dataLoader.LogUpdated += DataLoader_LogUpdated;
			dataLoader.InvalidFileHeaderProcessed += DataLoader_InvalidFileHeaderProcessed;

			LoadSpecificDataType(dataLoader, dataToLoad);
		}

		protected virtual void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			throw new NotImplementedException("Override Required");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void DataLoader_ProgressChanged(DataLoad sender, ProgressChangedEventArgs e)
		{
			try
			{
				ProgressBar.Maximum = e.RecordsImporting;
				ProgressBar.Value = e.CurrentRow;
			}
			catch (ArgumentOutOfRangeException ex)
			{
				ErrorReporter.ReportOnce("E9B75CDC-A9D5-4BD7-B6B0-47F16E23E089", string.Format(CultureInfo.InvariantCulture, "Import CSV log : {0}", e.Log.ToString()), ex);
			}
			Application.DoEvents();
		}

		void DataLoader_LogUpdated(DataLoad sender, LogUpdatedEventArgs e)
		{
			AddLogMessage(e.LogMessage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void AddLogMessage(string logText)
		{
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

			OutputListBox.SelectedIndex = OutputListBox.Items.Count - 1;
			OutputListBox.SelectedIndex = -1;

			Application.DoEvents();
		}

		void DataLoader_InvalidFileHeaderProcessed(DataLoad sender, EventArgs e)
		{
			var clipboardText = ConvertCommaToTabSeparated(sender.CSVTemplateHeading);
			var logText = SetClipboardText(clipboardText) ?
				Res.GetString("ec42b4d5-1537-4e9f-aaee-67109fd4acd8", "Please check the format in the template file copied to the clipboard, and also available for Download above this message.") :
				Res.GetString("eac68aeb-5b1b-4a0b-9bbe-0809ba830ee4", "Template file could not be copied to the clipboard as it is not accessible at the moment. It is available for Download above this message.");

			sender.DisplayLogMessage(logText);
		}

		protected virtual bool SetClipboardText(string text)
		{
			return SafeClipboard.SetText(text);
		}

		static string ConvertCommaToTabSeparated(string csvString)
		{
			return string.Join("\t", new OCsvLine(csvString).FieldValues);
		}

		#endregion

		#region DataLoader

		protected virtual DataLoad GetNewDataLoader()
		{
			throw new NotImplementedException("Override Required");
		}

#if DEBUG
		internal DataLoad GetNewDataLoader_ExposedForTesting()
		{
			return GetNewDataLoader();
		}
#endif

		#endregion
	}
}
