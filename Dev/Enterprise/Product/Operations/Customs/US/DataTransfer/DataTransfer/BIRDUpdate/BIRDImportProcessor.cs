using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer
{
	/// <summary>
	/// This class is responsible for scanning a directory in registry and processing all the text files.
	/// They are processed in an order of file creation time.
	/// When a file is successfully processed, it is deleted.
	/// </summary>
	public class BIRDImportProcessor
	{
		const string failedFile = ".failed";
		const string processingFile = ".processing";

		public void Execute(INotifications notifications)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			List<FileInfo> allFilenames = new List<FileInfo>(dirInfo.GetFiles());

			allFilenames.Sort((FileInfo x, FileInfo y) => x.CreationTime.CompareTo(y.CreationTime));

			foreach (FileInfo file in allFilenames)
			{
				ImportFile(file, notifications);
			}
		}

		internal virtual BIRDDataImporter Importer => new BIRDDataImporter(new BusinessObjectFactory());

		void ImportFile(FileInfo file, INotifications notifications)
		{
			bool shouldDeleteFile = true;

			try
			{
				notifications.Notify(new InfoNotification("Starting processing " + file.Name + "..."));

				Importer.ImportData(file.FullName, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.BIRDImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, file.Name));
			}
			catch (IOException)
			{
				shouldDeleteFile = false;  //skip the file and the files will be processed on the next cycle
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				shouldDeleteFile = false;

				MoveToBackupDirectoryAndLogException(file, ex.Message + "\r\n" + ex.StackTrace);

				notifications.Notify(new WarningNotification("There were problems processing this file, " + file.Name + ". It is moved to the backup file. Please check the directory."));
			}
			finally
			{
				notifications.Notify(new InfoNotification("Finished processing " + file.Name + "..."));

				if (shouldDeleteFile)
				{
					DeleteFile(file);
				}
			}
		}

		void MoveToBackupDirectoryAndLogException(FileInfo inputFile, string exceptionDetails)
		{
			string backUpDir = Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			string newName = Path.Combine(backUpDir, inputFile.Name);
			if (File.Exists(newName))
			{
				File.Delete(newName);
			}
			inputFile.MoveTo(newName);

			var exceptionFileName = (string.IsNullOrEmpty(inputFile.Extension) ? newName : newName.Replace(inputFile.Extension, "")) + "_Exception.txt";
			using (var toFile = File.Open(exceptionFileName, FileMode.Create))
			{
				var writer = new StreamWriter(toFile, System.Text.Encoding.UTF8);
				writer.Write(exceptionDetails);
				writer.Flush();
			}
		}

		void DeleteFile(FileInfo file)
		{
			try
			{
				file.Delete();
			}
			catch (IOException) { }
		}

		#region ExecuteByFtp

		public void ExecuteByFtp(INotifications notifications)
		{
			var processor = new FtpProcessor(Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty),
				Registry.Business.SystemDataRegistry.Instance.BIRDFTPUserName.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty),
				Registry.Business.SystemDataRegistry.Instance.BIRDFTPPassword.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), x => notifications.AddError(x),
				new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0),
				new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
			var remoteDirectoryName = Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).Trim();
			if (!string.IsNullOrEmpty(remoteDirectoryName) && !remoteDirectoryName.EndsWith("/"))
			{
				remoteDirectoryName += "/";
			}
			var brdFileExtension = "." + Registry.Business.SystemDataRegistry.Instance.BIRDFTPFileExtension.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			try
			{
				var remoteFileNames = processor.ListDirectory(remoteDirectoryName);
				if (remoteFileNames != null)
				{
					foreach (var remoteFileName in remoteFileNames)
					{
						try
						{
							var extension = Path.GetExtension(remoteFileName);
							if (!extension.Equals(failedFile) && extension.Equals(brdFileExtension))
							{
								var newFilename = remoteFileName + processingFile;
								if (!extension.Equals(processingFile))
								{
									processor.RenameRemoteFile(remoteDirectoryName + "\\" + remoteFileName, newFilename);
								}
								var remoteFilePath = remoteDirectoryName + "\\" + newFilename;
								if (ProcessingFile(newFilename, processor, notifications, remoteFileName))
								{
									processor.DeleteRemoteFile(remoteFilePath);
								}
								else
								{
									processor.RenameRemoteFile(remoteFilePath, remoteFileName + failedFile);
								}
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							notifications.AddError("Processing Error file [" + remoteFileName + "]. Error [" + ex.Message + "]");
						}
					}
				}
			}
			catch (FtpException ex1)
			{
				notifications.AddError("FTP Error Listing directory [" + remoteDirectoryName + "]. Error [" + ex1.FullMessage + "]");
				return;
			}
		}

		bool ProcessingFile(string newFilename, FtpProcessor processor, INotifications notifications, string originalFileName)
		{
			bool isSuccess = false;
			string remoteFileAndPathName = Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) + "\\" + newFilename;
			try
			{
				using (TempFile localFile = TempFile.New())
				{
					processor.DownloadFile(localFile.Filename, remoteFileAndPathName);
					isSuccess = ImportRemoteFile(localFile.Filename, notifications, originalFileName);
				}
			}
			catch (FtpException ex)
			{
				notifications.AddError("FTP Error Downloading file [" + remoteFileAndPathName + "]. Error [" + ex.Message + "]");
				isSuccess = false;
			}
			return isSuccess;
		}

		bool ImportRemoteFile(string localfileName, INotifications notifications, string originalFileName)
		{
			bool succeeded = true;
			try
			{
				notifications.Notify(new InfoNotification("Starting processing " + originalFileName + "..."));
				Importer.ImportData(localfileName, notifications,
					new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.BIRDImport, ZGuid.Empty, ZGuid.Empty,
						ZString.Empty, localfileName));
				notifications.Notify(new InfoNotification("Finished processing" + originalFileName + "..."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				succeeded = false;
				notifications.AddError(ex.Message);
			}
			return succeeded;
		}
		#endregion
	}
}
