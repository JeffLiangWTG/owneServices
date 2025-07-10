using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(BIRDDataTransferServiceTask))]
	sealed class BIRDDataTransferServiceTaskTest : ServiceTaskTestCase<BIRDDataTransferServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestMainwarning()
		{
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_Code = "t1";
			glbCompany1.GC_RN_NKCountryCode = "US";
			var branch = glbCompany1.Branches.AddNew();
			branch.GB_GC = glbCompany1.PK;
			branch.GB_Code = "Ts1";
			Factory.Save();
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch2 = glbCompany2.Branches.AddNew();
			branch2.GB_GC = glbCompany2.PK;
			branch2.GB_Code = "Ts2";
			Factory.Save();
			var serviceTask = new BIRDDataTransferServiceTask();
			InitialiseTaskSchedule(serviceTask, out _);
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertContains(BIRDDataTransferServiceTask.BIRDDataImport, log.ToString());
			log.ClearLog();
			SetFtp(glbCompany1);
			RunTaskSchedule(serviceTask);
			AssertNotContains(BIRDDataTransferServiceTask.BIRDDataImport, log.ToString());
		}

		public void TestValidateAroundImportDirectory()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "UST";
			Factory.Save();
			AssertEquals("PreCondition", "", Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			var serviceTask = new BIRDDataTransferServiceTask();
			InitialiseTaskSchedule(serviceTask, out _);
			var testDir = @"c:\random"; // This is a test
			Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testDir);
			RunTaskSchedule(serviceTask);
			TestServiceLogger log = InitialiseTaskSchedule(serviceTask);
			AssertContains(string.Format(BIRDDataTransferServiceTask.BIRDImportDirectoryIsNotSet, BIRDDataTransferServiceTask.BIRDImportDirectoryRegistry), log.ToString());
			log.ClearLog();
			Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testDir);
			RunTaskSchedule(serviceTask);
			var logs = log.ToString();
			AssertNotContains(string.Format(BIRDDataTransferServiceTask.BIRDImportDirectoryIsNotSet, BIRDDataTransferServiceTask.BIRDImportDirectoryRegistry), logs);
			AssertContains(string.Format(BIRDDataTransferServiceTask.BIRDImportDirectoryDoesNotExist, BIRDDataTransferServiceTask.BIRDImportDirectoryRegistry), logs);
		}

		public void TestValidateAroundImportBackupDirectory()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "UST";
			Factory.Save();
			AssertEquals("PreCondition", "", Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			var serviceTask = new BIRDDataTransferServiceTask();
			InitialiseTaskSchedule(serviceTask, out _);
			Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, @"C:\test"); // This is a test
			RunTaskSchedule(serviceTask);
			var log = InitialiseTaskSchedule(serviceTask);
			AssertContains(string.Format(BIRDDataTransferServiceTask.BIRDImportDirectoryIsNotSet, BIRDDataTransferServiceTask.BIRDImportBackupDirectoryRegistry), log.ToString());
			log.ClearLog();
			Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, @"c:\random"); // This is a test
			RunTaskSchedule(serviceTask);
			string logs = log.ToString();
			AssertNotContains(string.Format(BIRDDataTransferServiceTask.BIRDImportDirectoryIsNotSet, BIRDDataTransferServiceTask.BIRDImportBackupDirectoryRegistry), log.ToString());
			AssertContains(string.Format(BIRDDataTransferServiceTask.BIRDImportDirectoryDoesNotExist, BIRDDataTransferServiceTask.BIRDImportBackupDirectoryRegistry), logs);
		}

		public void TestRunTask()
		{
			using (TempDirectory dir = new TempDirectory())
			using (TempDirectory backupDir = new TempDirectory())
			{
				GlbCompany glbCompany = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany.GC_RN_NKCountryCode = "US";
				OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				glbCompany.GC_OH_OrgProxy = org.PK;
				GlbBranch branch = glbCompany.Branches.AddNew();
				branch.GB_GC = glbCompany.PK;
				branch.GB_Code = "DAN";
				Factory.Save();
				Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dir.DirectoryName);
				Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, backupDir.DirectoryName);
				string fileName = Path.Combine(dir.DirectoryName, "BIRDFileToImport.txt");
				using (Stream toFile = File.Open(fileName, FileMode.OpenOrCreate))
				{
					StreamWriter writer = new StreamWriter(toFile, System.Text.Encoding.UTF8);
					writer.Write(new EmbeddedResourceRetriever().GetString("Enterprise.Customs.US.ServiceTasks.Testing.BIRD.Testing.BIRDTestText.txt"));
					writer.Flush();
				}

				BIRDDataTransferServiceTask serviceTask = new BIRDDataTransferServiceTask();
				InitialiseTaskSchedule(serviceTask, out BusinessObject taskSchedule);
				RunTaskSchedule(serviceTask);
				Assert("The file should have been deleted", !File.Exists(fileName));
				JobDeclaration[] imxJobs = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.ImportByExternalBroker));
				AssertEquals("a declaration should have been created", 1, imxJobs.Length);
			}
		}

		public void TestValidateAroundFTPSetting()
		{
			var serviceTask = new BIRDDataTransferServiceTask();
			InitialiseTaskSchedule(serviceTask, out _);
			var log = InitialiseTaskSchedule(serviceTask);
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "UST";
			Factory.Save();
			AssertEquals(expected: false, serviceTask.CheckAnyEnvironments(new NotificationBuffer(), glbCompany));
			SetFtp(glbCompany);
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
			RunTaskSchedule(serviceTask);
			AssertContains(BIRDDataTransferServiceTask.BIRDFTPServerAddress, log.ToString());
			log.ClearLog();
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPUserName.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
			RunTaskSchedule(serviceTask);
			AssertContains(BIRDDataTransferServiceTask.BIRDFTPUserName, log.ToString());
			log.ClearLog();
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPUserName.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ftpuser");
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPPassword.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
			RunTaskSchedule(serviceTask);
			AssertContains(BIRDDataTransferServiceTask.BIRDFTPPassword, log.ToString());
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ftp://10.86.2.4/");
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Ftp/Test");
			Assert(serviceTask.CheckPath(new NotificationBuffer(), glbCompany));
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, @"\:\Ftp\Test");
			AssertEquals(expected: false, serviceTask.CheckPath(new NotificationBuffer(), glbCompany));
		}

		public void TestDirSetConflict()
		{
			var serviceTask = new BIRDDataTransferServiceTask();
			InitialiseTaskSchedule(serviceTask, out _);
			var log = InitialiseTaskSchedule(serviceTask);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_Code = "t1";
			glbCompany1.GC_RN_NKCountryCode = "US";
			var branch = glbCompany1.Branches.AddNew();
			branch.GB_GC = glbCompany1.PK;
			branch.GB_Code = "Ts1";
			Factory.Save();
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch2 = glbCompany2.Branches.AddNew();
			branch2.GB_GC = glbCompany2.PK;
			branch2.GB_Code = "Ts2";
			Factory.Save();
			using (var tempFolder1 = new TempDirectory(Path.Combine(Temp.TempPath, "test1")))
			using (var tempFolder2 = new TempDirectory(Path.Combine(Temp.TempPath, "test2")))
			using (var tempFolder3 = new TempDirectory(Path.Combine(Temp.TempPath, "test3")))
			{
				Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, tempFolder1);
				Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.SetValue(glbCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, tempFolder1);
				Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.SetValue(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, tempFolder2);
				Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.SetValue(glbCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, tempFolder3);
				log.ClearLog();
				RunTaskSchedule(serviceTask);
				AssertContains(string.Format(BIRDDataTransferServiceTask.BIRDDirAllSame, glbCompany1.GC_Code, glbCompany2.GC_Code), log.ToString());
			}
		}

		public void TestFtpSetConflict()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var serviceTask = new BIRDDataTransferServiceTask();
				InitialiseTaskSchedule(serviceTask, out _);
				var log = InitialiseTaskSchedule(serviceTask);
				var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany1.GC_Code = "t1";
				glbCompany1.GC_RN_NKCountryCode = "US";
				var branch = glbCompany1.Branches.AddNew();
				branch.GB_GC = glbCompany1.PK;
				branch.GB_Code = "Ts1";
				Factory.Save();
				SetFtp(glbCompany1);
				var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany2.GC_Code = "t2";
				glbCompany2.GC_RN_NKCountryCode = "US";
				var branch2 = glbCompany2.Branches.AddNew();
				branch2.GB_GC = glbCompany2.PK;
				branch2.GB_Code = "Ts2";
				Factory.Save();
				SetFtp(glbCompany2);
				var glbCompanys = serviceTask.GetActionCompanies();
				AssertEquals(2, glbCompanys.Length);
				RunTaskSchedule(serviceTask);
				AssertContains(string.Format(BIRDDataTransferServiceTask.BIRDFtpAllSame, glbCompany1.GC_Code, glbCompany2.GC_Code), log.ToString());
			}
		}

		public void TestFindCompanyWithBranch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var serviceTask = new BIRDDataTransferServiceTask();
				InitialiseTaskSchedule(serviceTask, out BusinessObject taskSchedule);
				var log = InitialiseTaskSchedule(serviceTask);
				var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany1.GC_Code = "t1";
				glbCompany1.GC_RN_NKCountryCode = "US";
				Factory.Save();
				SetFtp(glbCompany1);
				var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany2.GC_Code = "t2";
				glbCompany2.GC_RN_NKCountryCode = "US";
				var branch2 = glbCompany2.Branches.AddNew();
				branch2.GB_GC = glbCompany2.PK;
				branch2.GB_Code = "Ts2";
				Factory.Save();
				SetFtp(glbCompany2);
				var glbCompanys = serviceTask.GetActionCompanies();
				AssertEquals(1, glbCompanys.Length);
				AssertNoExceptionThrown(serviceTask.RunTask);
			}
		}

		public void TestFindCompanyWithDisabledBranch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var serviceTask = new BIRDDataTransferServiceTask();
				InitialiseTaskSchedule(serviceTask, out BusinessObject taskSchedule);
				var log = InitialiseTaskSchedule(serviceTask);
				var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany1.GC_Code = "t1";
				glbCompany1.GC_RN_NKCountryCode = "US";
				Factory.Save();
				SetFtp(glbCompany1);
				var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
				glbCompany2.GC_Code = "t2";
				glbCompany2.GC_RN_NKCountryCode = "US";
				var branch2 = glbCompany2.Branches.AddNew();
				branch2.GB_GC = glbCompany2.PK;
				branch2.GB_Code = "Ts2";
				branch2.GB_IsActive = false;
				Factory.Save();
				SetFtp(glbCompany2);
				var glbCompanys = serviceTask.GetActionCompanies();
				AssertEquals(0, glbCompanys.Length);
				AssertNoExceptionThrown(serviceTask.RunTask);
			}
		}

		public void TestExecutelockCompany()
		{
			var serviceTask = new Mock<BIRDDataTransferServiceTask>() { CallBase = true };
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_Code = "t1";
			glbCompany1.GC_RN_NKCountryCode = "US";
			var branch = glbCompany1.Branches.AddNew();
			branch.GB_GC = glbCompany1.PK;
			branch.GB_Code = "Ts1";
			Factory.Save();
			SetFtp(glbCompany1);
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.SetValue(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, "testfolder1");
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch2 = glbCompany2.Branches.AddNew();
			branch2.GB_GC = glbCompany2.PK;
			branch2.GB_Code = "Ts2";
			Factory.Save();
			SetFtp(glbCompany2);
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.SetValue(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, "testfolder2");
			var glbCompanys = new GlbCompany[2] { glbCompany1, glbCompany2 };
			serviceTask.Setup(m => m.GetActionCompanies()).Returns(glbCompanys);
			serviceTask.Setup(m => m.MutexPrefix).Returns("BIRD_");
			serviceTask.Setup(m => m.GetDbConnection()).Returns(Db.Connection);
			serviceTask.Setup(m => m.ProcessBRDFile(glbCompany1, glbCompanys))
				.Callback<GlbCompany, GlbCompany[]>((glbCompany, glbCompanys1) =>
				{
					AssertEquals(glbCompany1, glbCompany);
				});

			serviceTask.Setup(m => m.ProcessBRDFile(glbCompany2, glbCompanys))
				.Callback<GlbCompany, GlbCompany[]>((glbCompany, glbCompanys1) =>
				{
					AssertEquals(glbCompany2, glbCompany);
				});

			InitialiseTaskSchedule(serviceTask.Object);
			serviceTask.Object.RunTask();
			serviceTask.VerifyAll();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void SetFtp(GlbCompany glbCompany)
		{
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ftp://10.86.2.13/");
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "testfolder");
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPUserName.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ftpuser");
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPPassword.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ftppassword");
			Registry.Business.SystemDataRegistry.Instance.BIRDFTPFileExtension.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "BRD");
		}
	}
}
