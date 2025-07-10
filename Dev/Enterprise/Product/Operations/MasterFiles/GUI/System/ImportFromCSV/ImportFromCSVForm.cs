using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportFromCSVForm : ZChildForm
	{
		public ImportFromCSVForm(NonPersistentBusinessObject business) : base(business)
		{
			Init();
		}

		public ImportFromCSVForm()
		{
			Init();
		}

		void Init()
		{
			SelectFileButton.Click += SelectFileButton_Click;
			StartButton.Click += StartButton_Click;
			MessageStatusBarPanel.Text = Res.GetString("ImportFromCSVForm|MessageStatusBarPanel", "Select a file and click the \"Start Import\" button");
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				DownloadTemplateButton.Visible = CanDownloadTemplate;
			}
		}

		#region Select File

		void SelectFileButton_Click(object sender, EventArgs e)
		{
			SelectFile();
		}

		protected virtual void SelectFile()
		{
			if (OpenFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				FileNameTextBox.Text = OpenFileDialog.UnmappedFileName;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is not for DAT or DAT clients, but for real user clients who will generally have a C drive, on which their document is most likely to reside")]
		public ZOpenFileDialog OpenFileDialog
		{
			get
			{
				if (fOpenFileDialog == null)
				{
					fOpenFileDialog = new ZOpenFileDialog();
					fOpenFileDialog.Filter = FileDialogFilter;
					fOpenFileDialog.InitialDirectory = "C:\\";
					fOpenFileDialog.CheckFileExists = true;
				}

				return fOpenFileDialog;
			}
		}

		public virtual string FileDialogFilter
		{
			get { return Res.GetString("84faaade-aee7-46dd-8913-f46a9524931c", "Excel files (*.CSV)|*.CSV|All files (*.*)|*.*"); }
		}

		ZOpenFileDialog fOpenFileDialog;

		#endregion

		#region Download Template

		protected virtual bool CanDownloadTemplate
		{
			get { return false; }
		}

		void DownloadTemplateButton_Click(object sender, EventArgs e)
		{
			using (var downloadDialog = GetNewDownloadTemplateDialog())
			{
				var downloadTemplateDialogShowingArgs = new DownloadTemplateDialogShowingArgs(downloadDialog);
				if (DownloadTemplateDialogShowing != null)
				{
					DownloadTemplateDialogShowing(this, downloadTemplateDialogShowingArgs);
				}

				var dialogResult = downloadTemplateDialogShowingArgs.DialogResultOverride ?? downloadDialog.ShowDialog(this);
				if (dialogResult == DialogResult.OK)
				{
					try
					{
						using (var stream = downloadDialog.OpenFile())
						{
							if (DownloadTemplate(stream))
							{
								Globals.Message.ShowInformation(Res.GetString("40592f13-2b5c-47a2-99e6-71e1d10c0f45", "Template has been saved to {0}", downloadDialog.UnmappedFileName));
							}
						}
					}
					catch (IOException ex)
					{
						Globals.Message.ShowError(Res.GetString("6d6140b4-9365-490b-ae23-283debc77bca", "Cannot write the file to the disk.\r\n{0}", ex.Message));
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension")]
		ZSaveFileDialog GetNewDownloadTemplateDialog()
		{
			const string templateFileExtension = "csv";

			var dialog = new ZSaveFileDialog();
			dialog.FileName = Res.GetString("bde63663-cf08-4813-ba83-2b7ae8266423", "Template File.{0}", templateFileExtension);
			dialog.Filter = Res.GetString("a9895403-c737-4a57-bbb7-3db61b46cb96", "Comma delimited (*.{0})|*.{0}", templateFileExtension);
			return dialog;
		}

		protected virtual bool DownloadTemplate(Stream stream)
		{
			return false;
		}

		public event EventHandler<DownloadTemplateDialogShowingArgs> DownloadTemplateDialogShowing;

		public class DownloadTemplateDialogShowingArgs : EventArgs
		{
			public DownloadTemplateDialogShowingArgs(ZSaveFileDialog downloadTemplateDialog)
			{
				DownloadTemplateDialog = downloadTemplateDialog;
			}

			public readonly ZSaveFileDialog DownloadTemplateDialog;
			public DialogResult? DialogResultOverride;
		}

		#endregion

		#region Import Data

		void StartButton_Click(object sender, EventArgs e)
		{
			string dataLocation = FileNameTextBox.Text.Trim();
			if (dataLocation.IndexOfAny(Path.GetInvalidPathChars()) > -1)
			{
				Globals.Message.ShowError(Res.GetString("16f5faf5-2a4e-492c-8e28-fa447cf634e7", "File path {0} contains some illegal characters. Please edit file name.", dataLocation));
			}

			if (string.IsNullOrEmpty(dataLocation))
			{
				Globals.Message.ShowError(Res.GetString("d5b258d7-a959-4fbc-8eb9-70770d921525", "Please enter the location of the file you wish to import data from."));
			}
			else
			{
				using (ZOpenFileDialog.ForceLocalFile(ref dataLocation))
				{
					if (!File.Exists(dataLocation))
					{
						Globals.Message.ShowError(Res.GetString("345661ea-53c2-469d-bfa6-78134d94f7e3", "File {0} doesn't exist!", dataLocation));
					}
					else if (ConfirmLoadData())
					{
						LoadSelectedData(dataLocation);
					}
				}
			}
		}

		public virtual bool ConfirmLoadData()
		{
			return true;
		}

		protected void LoadSelectedData(string dataToLoad)
		{
			InitialiseRun();

			try
			{
				LoadSpecificDataType(dataToLoad);
			}
			catch (IOException fileException)
			{
				Globals.Message.ShowError(Res.GetString("49b22ed3-0528-4ef3-ab00-2fb6351bc91a", "Cannot import data! The detailed exception is:\r\n\r\n{0}", fileException.Message));
			}
			finally
			{
				FinaliseRun();
			}
		}

		public virtual void LoadSpecificDataType(string dataToLoad)
		{
			throw new NotImplementedException("Override Required");
		}

		#endregion

		#region Update Form

		void InitialiseRun()
		{
			SetButtons(false);
			OutputListBox.Items.Clear();
			MainStatusBar.Text = Res.GetString("ImportFromCSVForm|InitialiseRun", "Importing data... Please wait...");
		}

		void FinaliseRun()
		{
			SetButtons(true);
			MainStatusBar.Text = Res.GetString("6df2c30c-f8d0-41c3-bbaf-3793529dd24d", "Import Complete.");
		}

		void SetButtons(bool isEnabled)
		{
			FileNameTextBox.Enabled = isEnabled;
			SelectFileButton.Enabled = isEnabled;
			StartButton.Enabled = isEnabled;
			CopyLogToClipboardButton.Enabled = isEnabled;
			CopyLogToClipboardButton.Visible = isEnabled;
			CloseButton.Enabled = isEnabled;
			CloseButton.Visible = isEnabled;
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Logging

		void CopyLogToClipboardButton_Click(object sender, EventArgs e)
		{
			string clipboardData = GetLog();

			if (!SafeClipboard.SetDataObject(clipboardData))
			{
				string fileCreated = CreateLogInDataDirectory(clipboardData);
				Globals.Message.ShowInformation(Res.GetString("a59f7558-e3dc-4746-b57d-b456ce476226", "Copy to clipboard failed.\r\nThe log has been copied to a temporary file.\r\nLog File is: {0}", fileCreated));
			}
		}

		internal protected string GetLog()
		{
			StringBuilder builder = new StringBuilder();
			foreach (string line in OutputListBox.Items)
			{
				builder.Append(line);
				builder.Append(System.Environment.NewLine);
			}

			return builder.ToString();
		}

		internal protected string CreateLogInDataDirectory(string logData)
		{
			ZDateTime currentDateTime = ZDateTime.Now;
			ZString currentTimeString = currentDateTime.ToString("HHmmss");
			DirectoryInfo directory = new DirectoryInfo(FileNameTextBox.Text.Trim());

			if (directory.FullName.EndsWith(".csv") || directory.FullName.EndsWith(".CSV"))
			{
				directory = directory.Parent;
			}

			try
			{
				if (!directory.Exists)
				{
					directory.Create();
				}

				string fileName = Path.Combine(directory.FullName, "ProductDataImportLog." + currentTimeString + ".txt");
				using (StreamWriter sw = new StreamWriter(fileName, true))
				{
					sw.WriteLine(logData);
					sw.Flush();
				}
				return fileName;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error saving product update log file.", ex);
				return Res.GetString("b6615bef-9d67-4369-9af1-1f4d9bc0f32c", "Not Created");
			}
		}

		#endregion

		#region Test
#if DEBUG

		#region Test Form
		public class TestCsvLoadForm : ImportFromCSVForm
		{
			public TestCsvLoadForm()
			{
			}

			public override bool ConfirmLoadData()
			{
				string loadingData = "Please Note: Only test records will be loaded.";

				DialogResult result = Globals.Message.Show(loadingData, "Confirm Product Load", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
				return (result == DialogResult.OK);
			}

			public override void LoadSpecificDataType(string dataToLoad)
			{
				OrgSupplierPartDataLoad dataLoader = OrgSupplierPartDataLoad.New();
				dataLoader.ImportProductData(dataToLoad, false, false);
			}

			protected override bool CanDownloadTemplate
			{
				get { return true; }
			}

			protected override bool DownloadTemplate(Stream stream)
			{
				return true;
			}
		}

		#endregion

#endif
		#endregion

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (fOpenFileDialog != null)
				{
					fOpenFileDialog.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}
	}
}
