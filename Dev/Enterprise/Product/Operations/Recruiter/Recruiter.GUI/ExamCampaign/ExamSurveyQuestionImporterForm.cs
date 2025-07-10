using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	public partial class ExamSurveyQuestionImporterForm : ZChildForm, INotifications, INotificationSubscriberQueryUser
	{
		public ExamSurveyQuestionImporterForm(ExamSurveyQuestionImporterBizO importerBizO)
			: base(importerBizO)
		{
		}

		void BrowseButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.CheckFileExists = true;
				dialog.Filter = Res.GetString("8cf5877f-01a7-4d98-9d4a-b01efd4cbef6", "CSV files|*.CSV|XML files|*.XML");
				dialog.Title = Res.GetString("ec54ab48-fe69-483f-b3c5-5b7d7b21f09a", "Select a CSV file or XML file to import the questions from");

				DialogResult result = ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
				if (result == DialogResult.OK)
				{
					BusinessEntity.FileLocation = dialog.UnmappedFileName;
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			ValidateAll(ValidationType.Full);
			if (!BusinessEntity.HasErrors)
			{
				int noOfQuestionsImported;
				using (new ZWaitCursorChanger(this))
				{
					noOfQuestionsImported = BusinessEntity.ImportQuestions(this);
				}
				Globals.Message.ShowInformation(Res.GetString("8386b19a-50e8-4d80-af0a-f1d577fa8de1", "{0} questions successfully imported.", noOfQuestionsImported), Res.GetString("c3020dbc-8094-4c8c-a20d-18ca4289ed31", "Import"));
				Close();
			}
		}

		#region Download Template

		void DownloadTemplateButton_Click(object sender, EventArgs e)
		{
			using (var downloadDialog = GetNewDownloadTemplateDialog())
			{
				var downloadTemplateDialogShowingArgs = new ImportFromCSVForm.DownloadTemplateDialogShowingArgs(downloadDialog);
				DownloadTemplateDialogShowing?.Invoke(this, downloadTemplateDialogShowingArgs);

				var dialogResult = downloadTemplateDialogShowingArgs.DialogResultOverride ?? downloadDialog.ShowDialog(this);
				if (dialogResult == DialogResult.OK)
				{
					try
					{
						using (var stream = downloadDialog.OpenFile())
						{
							if (DownloadTemplate(stream))
							{
								Globals.Message.ShowInformation(Res.GetString("efb942f2-5e35-43ce-8c90-a606b6aefd7e", "Template has been saved to {0}", downloadDialog.UnmappedFileName));
							}
						}
					}
					catch (IOException ex)
					{
						Globals.Message.ShowError(Res.GetString("1ac33eb4-8c12-4c5b-bc90-d3b1031532ba", "Cannot write the file to the disk.\r\n{0}", ex.Message));
					}
				}
			}
		}

		protected bool DownloadTemplate(Stream stream)
		{
			using (var streamWriter = new StreamWriter(stream))
			{
				var templateHeader = string.Join(",", ValidFileHeaderColumns);
				var templateContent = string.Join(",", ValidFileContentColumns);
				streamWriter.Write(string.Join("\n", templateHeader, templateContent));
				streamWriter.Flush();
			}

			return true;
		}

		#region Download Template Example Content
		#region SuppressResourceStringsCheckRegion

		string[] validFileHeaderColumns;
		string[] ValidFileHeaderColumns
		{
			get { return validFileHeaderColumns ?? (validFileHeaderColumns = GetNewValidFileHeaderColumns()); }
		}

		string[] GetNewValidFileHeaderColumns()
		{
			return new string[] {
				"Question",
				"Country",
				"Randomisable",
				"Comment",
				"Correct Answer",
				"A1",
				"A2",
				"A3",
				"A4"
			};
		}

		string[] validFileContentColumns;
		string[] ValidFileContentColumns
		{
			get { return validFileContentColumns ?? (validFileContentColumns = GetNewValidFileContentColumns()); }
		}

		string[] GetNewValidFileContentColumns()
		{
			return new string[] {
				"How would you define that a Part Attribute is 'Mandatory' for a Client?",
				"AU",
				"Y",
				"Related Learning Material",
				"4",
				"An incorrect answer",
				"An incorrect answer",
				"An incorrect answer",
				"A correct answer"
			};
		}

		#endregion
		#endregion

		public event EventHandler<ImportFromCSVForm.DownloadTemplateDialogShowingArgs> DownloadTemplateDialogShowing;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension")]
		ZSaveFileDialog GetNewDownloadTemplateDialog()
		{
			const string templateFileExtension = "csv";

			var dialog = new ZSaveFileDialog();
			dialog.FileName = Res.GetString("5b30f844-732a-4551-af60-36e50493327f", "Questions Template File.{0}", templateFileExtension);
			dialog.Filter = Res.GetString("0e44664b-7468-4ad3-b965-0f68e0915738", "Comma delimited (*.{0})|*.{0}", templateFileExtension);
			return dialog;
		}

		#endregion

		public override string FormHeading
		{
			get { return Res.GetString("8f8a22e7-7d44-4324-ad53-6e83ddcfdf52", "Import Questions"); }
		}

		public new ExamSurveyQuestionImporterBizO BusinessEntity
		{
			get { return (ExamSurveyQuestionImporterBizO)base.BusinessEntity; }
		}

		#region INotifications Members

		void INotifications.Add(INotification @event)
		{
			if (@event is ErrorNotification)
			{
				Globals.Message.ShowError(@event.Message, ((ErrorNotification)@event).Type.Message);
			}
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
