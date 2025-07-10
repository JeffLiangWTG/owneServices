using System;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.IO.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class BIRDImportProcessorTest : TestCaseWithFactory
	{
		public void TestProcessFileWithoutExtension()
		{
			using (var dir = new TempDirectory())
				using (Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dir.DirectoryName))
				{
					var fileName = Path.Combine(dir.DirectoryName, "BIRDFileToImport");
					CopyBIRDTestTextFileIntoFile(fileName, "BIRDTestText.txt");
					var notifications = new NotificationCollection();
					AssertNoExceptionThrown("String cannot be of zero length.", () =>
					{
						new BIRDImportProcessorForTest().Execute(notifications);
					});
				}
		}

		public void TestProcessFile()
		{
			using (TempDirectory dir = new TempDirectory())
			{
				Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dir.DirectoryName);
				string fileName = Path.Combine(dir.DirectoryName, "BIRDFileToImport.txt");
				CopyBIRDTestTextFileIntoFile(fileName, "BIRDTestText.txt");
				NotificationCollection notifications = new NotificationCollection();
				new BIRDImportProcessor().Execute(notifications);
				AssertContains("Finished processing BIRDFileToImport.txt", notifications.ToUniqueMessageListString());
				Assert("should have been deleted", !File.Exists(fileName));
			}
		}

		public void TestNoMaxLengthExceptionThrownWhenProcessFileWithAMSRecord()
		{
			using (var dir = new TempDirectory())
			{
				Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dir.DirectoryName);
				var fileName = Path.Combine(dir.DirectoryName, "BIRDFileToImport.txt");
				CopyBIRDTestTextFileIntoFile(fileName, "BIRDWithAMSRecord.txt");
				var notifications = new NotificationCollection();
				AssertNoExceptionThrown(() => new BIRDImportProcessor().Execute(notifications));
			}
		}

		public void TestWhenExceptionHappens()
		{
			using (TempDirectory dir = new TempDirectory())
				using (TempDirectory dir2 = new TempDirectory())
				{
					Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dir.DirectoryName);
					Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dir2.DirectoryName);
					string fileName = Path.Combine(dir.DirectoryName, "BIRDError.txt");
					CopyBIRDTestTextFileIntoFile(fileName, "BIRDError.txt");
					NotificationCollection notifications = new NotificationCollection();
					new BIRDImportProcessor().Execute(notifications);
					Assert("should have been deleted", !File.Exists(fileName));
					fileName = Path.Combine(dir2.DirectoryName, "BIRDError.txt");
					Assert("it is not moved to the new destination", !File.Exists(fileName));
				}
		}

		public void TestProcessInOrderOfCreationTime()
		{
			using (TempDirectory dir = new TempDirectory())
			{
				Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dir.DirectoryName);
				string fileName1 = Path.Combine(dir.DirectoryName, "Test1.txt");
				CopyBIRDTestTextFileIntoFile(fileName1, "BIRDTestText.txt");
				FileInfo file1 = new FileInfo(fileName1);
				file1.CreationTime = new DateTime(2009, 1, 1, 13, 0, 0);
				string fileName2 = Path.Combine(dir.DirectoryName, "Test2.txt");
				CopyBIRDTestTextFileIntoFile(fileName2, "BIRDTestText.txt");
				FileInfo file2 = new FileInfo(fileName2);
				file2.CreationTime = new DateTime(2009, 1, 1, 12, 0, 0); //created earlier
				NotificationCollection notifications = new NotificationCollection();
				new BIRDImportProcessor().Execute(notifications);
				string warnings = notifications.ToUniqueMessageListString();
				int indexOfFileName1 = warnings.IndexOf("Test1.txt");
				int indexOfFileName2 = warnings.IndexOf("Test2.txt");
				Assert("Test2.txt should have been processed before Test1.txt", indexOfFileName1 > indexOfFileName2);
			}
		}

		public void TestCorrectExecuteByFtp()
		{
			FtpProcessor processor = new FtpProcessor(ftpTestHelper.ServerAddress.ToString(), ftpTestHelper.UserName, ftpTestHelper.Password, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
			using (TempDirectory dir = new TempDirectory())
			{
				Registry.Business.SystemDataRegistry.Instance.BIRDFTPFileExtension.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "txt");
				string fileName = Path.Combine(dir.DirectoryName, "BIRDFileToImport.txt");
				CopyBIRDTestTextFileIntoFile(fileName, "BIRDTestText.txt");
				processor.UploadFile(fileName, Path.Combine(remoteDirectoryName, Path.GetFileName(RemoteFile)));
				NotificationCollection notifications = new NotificationCollection();
				new BIRDImportProcessor().ExecuteByFtp(notifications);
				AssertEquals(true, notifications.Select(x => x.Message.Contains("Finished processing")).Any());
				string[] remoteFileNames = processor.ListDirectory(remoteDirectoryName);
				AssertEquals(0, remoteFileNames.Length);
			}
		}

		public void TestWhenFtpExcept()
		{
			FtpProcessor processor = new FtpProcessor(ftpTestHelper.ServerAddress.ToString(), ftpTestHelper.UserName, ftpTestHelper.Password, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
			using (TempDirectory dir = new TempDirectory())
			{
				string fileName = Path.Combine(dir.DirectoryName, "BIRDFileToImport.txt");
				CopyBIRDTestTextFileIntoFile(fileName, "BIRDTestText.txt");
				processor.UploadFile(fileName, Path.Combine(remoteDirectoryName, Path.GetFileName(RemoteFile)));
				Registry.Business.SystemDataRegistry.Instance.BIRDFTPUserName.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "FtpExceptionTest");
				NotificationCollection notifications = new NotificationCollection();
				new BIRDImportProcessor().ExecuteByFtp(notifications);
				AssertEquals(true, notifications.Select(x => x.Message.Contains("FTP Error")).Any());
			}
		}

		public void TestWhenRemoteDirectory()
		{
			TearDown();
			remoteDirectoryName = remoteDirectoryName + "/RemoteDirectory";
			SetUp();
			TestCorrectExecuteByFtp();
			TearDown();
			remoteDirectoryName += "/";
			SetUp();
			TestCorrectExecuteByFtp();
		}

		public void TestWhenFileErrorExecuteByFtp()
		{
			FtpProcessor processor = new FtpProcessor(ftpTestHelper.ServerAddress.ToString(), ftpTestHelper.UserName, ftpTestHelper.Password, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
			using (TempDirectory dir = new TempDirectory())
			{
				string fileName = Path.Combine(dir.DirectoryName, "BIRDError.txt");
				CopyBIRDTestTextFileIntoFile(fileName, "BIRDError.txt");
				processor.UploadFile(fileName, Path.Combine(remoteDirectoryName, Path.GetFileName(RemoteFile)));
				try
				{
					NotificationCollection notifications = new NotificationCollection();
					new BIRDImportProcessor().ExecuteByFtp(notifications);
					AssertEquals(true, notifications.Select(x => x.Message.Contains("Error")).Any());
				}
				catch
				{
					DeleteIfExists(Path.GetFileName(RemoteFile));
					DeleteIfExists(fileName);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ftpTestHelper = new FtpTestHelper();
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPPassword.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ftpTestHelper.Password);
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPUserName.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ftpTestHelper.UserName);
			ftpTestHelper.Start();
			Directory.CreateDirectory(Path.Combine(ftpTestHelper.LocalDirectory, remoteDirectoryName));
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "localhost:" + ftpTestHelper.Port);
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, remoteDirectoryName);
		}

		protected override void TearDown()
		{
			ftpTestHelper.Dispose();
			base.TearDown();
		}

		void CopyBIRDTestTextFileIntoFile(string fileName, string birdTestFileName)
		{
			using (Stream toFile = File.Open(fileName, FileMode.OpenOrCreate))
			{
				StreamWriter writer = new StreamWriter(toFile, System.Text.Encoding.UTF8);
				StreamReader reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(BIRDDataImporterTest.TestResource + birdTestFileName));
				writer.Write(reader.ReadToEnd());
				writer.Flush();
			}
		}
		FtpTestHelper ftpTestHelper;
		string remoteDirectoryName = "Test";

		string _remoteFile;
		string RemoteFile => _remoteFile ?? (_remoteFile = Path.Combine(Path.Combine(ftpTestHelper.LocalDirectory, remoteDirectoryName), Guid.NewGuid().ToString() + ".txt"));

		sealed class BIRDImportProcessorForTest : BIRDImportProcessor
		{
			internal override BIRDDataImporter Importer => new BIRDDataImporterForTest(new BusinessObjectFactory());
		}

		sealed class BIRDDataImporterForTest : BIRDDataImporter
		{
			public BIRDDataImporterForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public override bool CheckEnvironmentValid(BusinessObjectFactory factory, INotifications notifications) => throw new Exception("blabla");
		}
	}
}
