using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Customs.SG.MHUB.MHX.Testing
{
	sealed class Mhx4ProfileCreatorTester : TestCaseWithFactory
	{
		public void TestCreateInputScript_Upload()
		{
			var creator = new Mhx4ProfileCreator(credentials);
			creator.CreateInputScript_Upload("Example.edi", "DCST401", new LoggingInformation());
			var inputScriptContent = File.ReadAllText(creator.InputFile.FullName);
			profileFileToDelete = creator.InputFile;
			AssertContains(@"Command=Submit|cont_type=E|filename=Example.edi|notifn=N", inputScriptContent);
			var attachmentsDir = Temp.TempPath + "\\v00001\\Example.edi" + Mhx4ProfileCreator.AttachmentsFolderName;
			var dir = new DirectoryInfo(attachmentsDir);
			dir.Create();
			using (File.Create(Path.Combine(dir.FullName, "a.jpg")))
			{
			}

			using (File.Create(Path.Combine(dir.FullName, "b.jpg")))
			{
			}

			creator.CreateInputScript_Upload("Example.edi", "DCST401", new LoggingInformation());
			inputScriptContent = File.ReadAllText(creator.InputFile.FullName);
			AssertContains(@"Command=Submit|cont_type=E|filename=Example.edi|notifn=N|attachment=true|attachment_files=" + dir.FullName + "\\a.jpg," + dir.FullName + "\\b.jpg", inputScriptContent);
		}

		public void TestCreateInputScriptForXMLMessage_Upload()
		{
			var creator = new Mhx4ProfileCreator(credentials);
			creator.CreateInputScript_Upload("Example_2005.xml", "DCST401", new LoggingInformation());
			var inputScriptContent = File.ReadAllText(creator.InputFile.FullName);
			profileFileToDelete = creator.InputFile;
			AssertContains(@"Command=Submit|cont_type=B|filename=Example_2005.xml|recip_id=DCST401|subj=2005|notifn=N", inputScriptContent);
			var attachmentsDir = Temp.TempPath + "\\v00001\\Example_2005.xml" + Mhx4ProfileCreator.AttachmentsFolderName;
			var dir = new DirectoryInfo(attachmentsDir);
			dir.Create();
			using (File.Create(Path.Combine(dir.FullName, "a.jpg")))
			{
			}

			using (File.Create(Path.Combine(dir.FullName, "b.jpg")))
			{
			}

			creator.CreateInputScript_Upload("Example_2005.xml", "DCST401", new LoggingInformation());
			inputScriptContent = File.ReadAllText(creator.InputFile.FullName);
			AssertContains(@"Command=Submit|cont_type=B|filename=Example_2005.xml|recip_id=DCST401|subj=2005|notifn=N|attachment=true|attachment_files=" + dir.FullName + "\\a.jpg," + dir.FullName + "\\b.jpg", inputScriptContent);
		}

		public void TestCreateInputScript_Retrieve()
		{
			var creator = new Mhx4ProfileCreator(credentials);
			creator.CreateInputScript_Retrieve();
			var content = File.ReadAllText(creator.InputFile.FullName);
			profileFileToDelete = creator.InputFile;
			Assert(creator.DownloadFile.Directory.Exists);
			AssertContains(@"Command=Retrieve|filename=" + creator.DownloadFile + "|loc=I", content);
		}

		public void TestCredentials()
		{
			var settings = new Mock<IMHUBSettings>();
			settings.SetupGet(x => x.WebAddress).Returns("https://trial.tradenet.gov.sg");
			settings.SetupGet(x => x.EncryptionKey).Returns("UdJCd/zLFmybL6HgjSD08TkJKA9tGcis");
			var creator = new Mhx4ProfileCreator(credentials);
			creator.CreateProfileFile(settings.Object);
			profileFileToDelete = creator.ProfileFile;
			AssertContains("v00001", profileFileToDelete.Name);
			var content = File.ReadAllText(profileFileToDelete.FullName);
			AssertContains("UserID=v00001", content);
			AssertContains("Password=X+FFoaKwogU=", content);
		}

		public void TestProfileProduction()
		{
			var webAddress = new StringEffectiveDate();
			webAddress.PreviousValue = "https://www.live.com";
			webAddress.NewValue = "https://www.tradenet.gov.sg";
			webAddress.EffectiveDate = ZDateTime.Now.AddDays(5);
			using (SGCustomsDataRegistry.Instance.WebAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, webAddress))
			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creator = new Mhx4ProfileCreator(credentials);
				creator.CreateProfileFile(new MHUBSettingsProvider());
				profileFileToDelete = creator.ProfileFile;
				var content = File.ReadAllText(profileFileToDelete.FullName);
				CombineAssertions(() =>
				{
					AssertContains("trustStorePath=MHX_ProdTruststore.db", content);
					AssertContains("ServerIP=www.live.com", content);
				});
			}
		}

		public void TestProfileTrial()
		{
			var webAddressTrial = new StringEffectiveDate();
			webAddressTrial.PreviousValue = "https://www.test.com";
			webAddressTrial.NewValue = "https://trial.tradenet.gov.sg";
			webAddressTrial.EffectiveDate = ZDateTime.Now.AddDays(5);
			using (SGCustomsDataRegistry.Instance.WebAddressTrial.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, webAddressTrial))
			using (SGCustomsDataRegistry.Instance.SendTestMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var creator = new Mhx4ProfileCreator(credentials);
				creator.CreateProfileFile(new MHUBSettingsProvider());
				profileFileToDelete = creator.ProfileFile;
				var content = File.ReadAllText(profileFileToDelete.FullName);
				CombineAssertions(() =>
				{
					AssertContains("trustStorePath=MHX_TrialTruststore.db", content);
					AssertContains("ServerIP=www.test.com", content);
				});
			}
		}

		public void TestFilePaths()
		{
			var tempDirRoot = Temp.TempPath;
			tempDirRoot = tempDirRoot.Replace("\\", "\\\\").Replace(":", @"\:");
			var creator = new Mhx4ProfileCreator(credentials);
			var settings = new Mock<IMHUBSettings>();
			settings.SetupGet(x => x.WebAddress).Returns("https://www.tradenet.gov.sg");
			settings.SetupGet(x => x.EncryptionKey).Returns("UdJCd/zLFmybL6HgjSD08TkJKA9tGcis");
			creator.CreateProfileFile(settings.Object);
			profileFileToDelete = creator.ProfileFile;
			var suffix = creator.RandomSuffix;
			var content = File.ReadAllText(profileFileToDelete.FullName);
			AssertContains("WorkingDir=" + tempDirRoot + @"v00001", content);
			AssertContains("ResponsePath=" + tempDirRoot + @"v00001\\Output_v00001_" + suffix + ".txt", content);
			AssertContains("StatusPath=" + tempDirRoot + @"v00001\\StatusLog_v00001_" + suffix + ".txt", content);
			AssertContains("InputScriptPath=" + tempDirRoot + @"v00001\\Input_v00001_" + suffix + ".txt", content);
			AssertContains("LogPath=" + tempDirRoot + @"v00001\\TraceLog_v00001_" + suffix + ".txt", content);
			AssertContains("HistoryPath=" + tempDirRoot + @"v00001\\History_v00001_" + suffix + ".txt", content);
			creator = new Mhx4ProfileCreator(credentials);
			creator.CreateProfileFile(settings.Object);
			AssertNotEquals("Different instances use different file names", profileFileToDelete, creator.ProfileFile.FullName);
		}

		public void TestProxyEntered()
		{
			var creator = new Mhx4ProfileCreator(credentials);
			var settings = new Mock<IMHUBSettings>();
			settings.SetupGet(x => x.WebAddress).Returns("https://www.tradenet.gov.sg");
			settings.SetupGet(x => x.EncryptionKey).Returns("UdJCd/zLFmybL6HgjSD08TkJKA9tGcis");
			settings.SetupGet(x => x.WebProxyAddress).Returns("SYD-POXY-1:888"); // that is not a typo :)
			creator.CreateProfileFile(settings.Object);
			profileFileToDelete = creator.ProfileFile;
			var content = File.ReadAllText(profileFileToDelete.FullName);
			AssertContains("proxyHost=SYD-POXY-1", content);
			AssertContains("ProxyConnect=true", content);
			AssertContains("proxyPort=888", content);
		}

		public void TestProxyEmpty()
		{
			var creator = new Mhx4ProfileCreator(credentials);
			var settings = new Mock<IMHUBSettings>();
			settings.SetupGet(x => x.WebAddress).Returns("https://www.tradenet.gov.sg");
			settings.SetupGet(x => x.EncryptionKey).Returns("UdJCd/zLFmybL6HgjSD08TkJKA9tGcis");
			settings.SetupGet(x => x.WebProxyAddress).Returns(string.Empty);
			creator.CreateProfileFile(settings.Object);
			profileFileToDelete = creator.ProfileFile;
			var content = File.ReadAllText(profileFileToDelete.FullName);
			AssertContains("proxyHost=\r\n", content);
			AssertContains("ProxyConnect=false", content);
			AssertContains("proxyPort=\r\n", content);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var broker = Factory.New<GlbStaff>();
			broker.GS_IsActive = true;
			var sgStaffWrapper = broker.GetSGWrapper();
			sgStaffWrapper.Tradenetv4Password.GP_UserID = "v00001";
			sgStaffWrapper.Tradenetv4Password.CurrentDecryptedPassword = "BiteMe";
			sgStaffWrapper.Tradenetv4Password.GP_PasswordStatus = Core.Constants.PasswordOK;
			broker.GS_Code = "ZAC";
			broker.GS_EmailAddress = "test1@hotmail.com";
			credentials = sgStaffWrapper.Tradenetv4Password;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (profileFileToDelete != null)
			{
				profileFileToDelete.Directory.Delete(true);
			}
		}

		FileInfo profileFileToDelete;
		IGlbExternalPassword credentials;
	}
}
