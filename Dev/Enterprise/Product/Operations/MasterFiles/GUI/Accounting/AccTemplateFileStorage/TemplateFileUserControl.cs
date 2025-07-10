using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TemplateFileUserControl : ZUserControl
	{
		public TemplateFileUserControl()
		{
			InitializeComponent();
		}

		void btnNew_Click(object sender, EventArgs e)
		{
			TemplateCollection.AddNew();
		}

		void btnEditFile_Click(object sender, EventArgs e)
		{
			if (TemplateCollection != null)
			{
				Import(true);
			}
		}

		void btnDelete_Click(object sender, EventArgs e)
		{
			if (CurrentItem.CanDelete)
			{
				TemplateCollection.RemoveAndDelete(CurrentItem);
			}
			else
			{
				Globals.Message.ShowWarning(CurrentItem.ReasonForNotAbleToDelete);
			}
		}

		protected virtual void Import(bool changeCurrent = false)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;
				dialog.Filter = string.Format((NoResString)"{0} Files (*.{1})|*.{1}", FileType, FileExtension);
				dialog.Title = Res.GetString("TemplateFileUserControl|SelectFile", "Select an {0} file to import", FileType);

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					var fileFullPath = dialog.ForceLocalFiles().FirstOrDefault();
					var info = new FileInfo(fileFullPath);
					if (info.Exists)
					{
						ConvertFileDataToBytesAndAddToCollection(changeCurrent, fileFullPath);
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("TemplateFileUserControl|FileDoesNotExist", "File '{0}' Does Not Exist.", fileFullPath));
					}
				}
			}
		}

		protected void ConvertFileDataToBytesAndAddToCollection(bool changeCurrent, string fileFullPath)
		{
			try
			{
				var fileAsBytes = File.ReadAllBytes(fileFullPath);
				var fileName = Path.GetFileName(fileFullPath);

				if (changeCurrent)
				{
					ChangeCurrentItem(fileName, fileAsBytes);
				}
				else
				{
					AddFileToCollection(fileName, fileAsBytes);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(Res.GetString("TemplateFileUserControl|FileReadError", "Error while reading the file.'{0}'", fileFullPath), ex.Message);
			}
		}

		void AddFileToCollection(string fileName, byte[] fileAsBytes)
		{
			var item = TemplateCollection.AddNew();
			item.TFS_FileName = fileName;
			item.TFS_FileData = fileAsBytes;
		}

		void ChangeCurrentItem(string fileName, byte[] fileAsBytes)
		{
			CurrentItem.TFS_FileName = fileName;
			CurrentItem.TFS_FileData = fileAsBytes;
		}

		AccTemplateFileStorage CurrentItem => TemplateFileGrid.GetCurrent() as AccTemplateFileStorage;

		AccTemplateFileStorageCollection TemplateCollection => ((GlbCompany)TemplateFileGrid.DataSource).TemplateFiles;

		const string FileType = "XSLT";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension")]
		const string FileExtension = "xslt";
	}
}
