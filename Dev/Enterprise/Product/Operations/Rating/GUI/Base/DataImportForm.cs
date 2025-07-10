using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class DataImportForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public DataImportForm()
		{
			InitializeComponent();
		}

		public DataImportForm(RateDataImporter importer, Action<string, Stream> importAction)
			: base(importer)
		{
			InitializeComponent();
			this.ImportAction = importAction;

			if (!importer.IsStandardTACTImport || !importer.ShowClearRatesOptions)
			{
				ClearTACTCheckBox.Visible = false;
				ClearStandardCheckBox.Visible = false;
				IsJobLevelChargeCheckBox.Visible = false;
				RoundingDropEdit.Visible = false;
				ExcludeFromAutocostingCheckBox.Visible = false;
			}
		}

		RateDataImporter Importer
		{
			get { return (RateDataImporter)BusinessEntity; }
		}

		public RatingForm ParentRatingFormToClose { get; set; }
		readonly Action<string, Stream> ImportAction;
		ZString LastImportedFileName;
		bool isImporting;

		#region Overridden Methods

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);

			if (openFileDialog != null)
			{
				openFileDialog.Dispose();
			}
		}

		#endregion

		#region Open File Dialog

		protected virtual ZOpenFileDialog OpenFileDialog
		{
			get
			{
				if (openFileDialog == null)
				{
					openFileDialog = new ZOpenFileDialog();
					openFileDialog.Filter = (NoResString)"All files (*.*)|*.*"; // Filter related.
					openFileDialog.CheckFileExists = true;
				}

				return openFileDialog;
			}
		}
		ZOpenFileDialog openFileDialog;

		protected virtual DialogResult OpenFileDialogResult
		{
			get { return OpenFileDialog.ShowDialog(this); }
		}

		ZString OpenedFilePath
		{
			get { return OpenFileDialog.UnmappedFileName; }
		}

		#endregion

		#region Event Handlers

		void ChooseFileButton_Click(object sender, EventArgs e)
		{
			if (OpenFileDialogResult == DialogResult.OK)
			{
				FilePathLabel.Text = OpenedFilePath;
			}
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			if (OpenedFilePath.IsEmpty)
			{
				Globals.Message.ShowError(Res.GetString("1d3cd5c2-c75d-41f2-9930-337563f63fd7", "Please select a file to import."),
					Res.GetString("87258110-b833-469f-a5c1-357484038ff2", "Cannot Import"));
			}
			else if (LastImportedFileName == OpenedFilePath)
			{
				Globals.Message.ShowError(Res.GetString("aad291e6-6cfd-4157-b3f3-0a138ff6be88", "The selected file has been imported already. Please choose another file."),
					Res.GetString("87258110-b833-469f-a5c1-357484038ff2", "Cannot Import"));
			}
			else if (!Importer.IsDefaultFreightChargeCodeValid)
			{
				Globals.Message.ShowError(Res.GetString("9a4bb52e-08aa-4dbd-93c0-5048bd86cd4c", "Default freight charge code is not valid. Please set it in registry AutoRating -> Charge Codes -> Freight -> Freight Charge Code."),
					Res.GetString("87258110-b833-469f-a5c1-357484038ff2", "Cannot Import"));
			}
			else if (Importer.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("59252ba4-2900-452f-bcbb-3fd8cdc265f8", "Please fix all errors before importing."),
					Res.GetString("87258110-b833-469f-a5c1-357484038ff2", "Cannot Import"));
			}
			else
			{
				if (ParentRatingFormToClose != null)
				{
					Globals.Message.ShowInformation(Res.GetString("6afaf581-dcae-4721-a474-8303762a1cc9", "The Costing form will close and re-open after rate import finishes. Press OK to start import."),
						Res.GetString("1d415c37-390a-431c-9270-08bf5fadee39", "Important"));
					ParentRatingFormToClose.Close();
					BringToFront();
				}

				cursorBefore = this.Cursor;

				SetImportingState();
				ImportData();
			}
		}
		Cursor cursorBefore;

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = isImporting;
			base.OnClosing(e);
		}

		#endregion

		#region Import Data

		void UpdateGUI(Action action)
		{
			if (InvokeRequired)
			{
				Invoke(action);
			}
			else
			{
				action();
			}
		}

		void ImportData()
		{
			try
			{
				Importer.RatingHeader.ProgressChanged += Importer_ProgressChanged;
				Importer.ProgressHandler = Importer_ProgressChanged;
				Importer.ImportCompletedHandler = SetImportCompletedState;

				ImportAction(OpenedFilePath, GetStream());
			}
			catch (IOException ex)
			{
				InfoTextBox.Text = Res.GetString("1bb53641-8b1e-4ade-8c4c-e5701693ef4a", "The file could not be read: {0}", ex.Message);
				Importer.ImportCompletedHandler();
			}
			finally
			{
				Importer.RatingHeader.ProgressChanged -= Importer_ProgressChanged;
			}
		}

		Stream GetStream()
		{
			var clientStream = OpenFileDialog.OpenFile();
#if WINZOR
			return clientStream;
#else
			if (!ZOpenFileDialog.IsRemote)
			{
				return clientStream;
			}

			// The RDP connection may be slow, especially in the sandbox and the stream may be read several times (TACT import).
			// So, better to copy the file to the server and then import it from there. 
			var file = Env.GetTempFileName();

			using (var serverStream = File.Open(file, FileMode.Create, FileAccess.Write))
			{
				clientStream.CopyTo(serverStream);
			}

			var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
			return stream;
#endif
		}

		bool Importer_ProgressChanged(int percentComplete, string status)
		{
			UpdateGUI(() =>
			{
				InfoTextBox.Text = status;
				ImportProgressBar.Value = percentComplete;
			});

			return true;
		}

		void SetImportingState()
		{
			foreach (var control in ControlsToBeDisabledDuringImport)
			{
				control.Enabled = false;
			}
			this.Cursor = Cursors.WaitCursor;
			isImporting = true;
		}

		void SetImportCompletedState()
		{
			UpdateGUI(() =>
			{
				if (ParentRatingFormToClose != null)
				{
					var controllerID = ParentRatingFormToClose.ControllerID;

					//CLEAR THE CACHE
					var reloadedParent = new BusinessObjectFactory().Load<RatingHeader>(Importer.RatingHeader.PK);
					ParentRatingFormToClose = (RatingForm)Activator.CreateInstance(ParentRatingFormToClose.GetType(), reloadedParent);
					ParentRatingFormToClose.ControllerID = controllerID;

					ParentRatingFormToClose.Show();
					ParentRatingFormToClose.SendToBack();
					Importer.SortEntriesWithError();
					ParentRatingFormToClose.BringToFront();
				}

				if (ImportProgressBar.Value < 100)
				{
					ImportProgressBar.Value = 100;
				}

				if (!string.IsNullOrEmpty(Importer.ImportReport))
				{
					InfoTextBox.Text += System.Environment.NewLine + Importer.ImportReport;
				}

				foreach (var control in ControlsToBeDisabledDuringImport)
				{
					control.Enabled = true;
				}

				LastImportedFileName = OpenedFilePath;
				isImporting = false;

				this.Cursor = cursorBefore;
				BringToFront();
				CloseButton.Focus();
			});
		}

		IEnumerable<Control> ControlsToBeDisabledDuringImport => new Control[] { ImportButton, ChooseFileButton, ClearTACTCheckBox, ClearStandardCheckBox, CloseButton, IsJobLevelChargeCheckBox, RoundingDropEdit };

		#endregion
	}
}

