using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Workflow.MessageDelivery.Testing
{
	sealed class EDIMessageDeliveryTest : TestCaseWithFactory
	{
		public void TestFtpUploadWithBadProtocol()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			SetupEmail();
			var del = new EDIMessageDelivery();
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Destination = "http://bad.com";  // Http is not supported
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
			var result = del.Deliver(GetContext(), mode, new DeliveryStreamWrapperUXML(GetStreamForTest()));
			Factory.Save();

			Assert(!result.Succeeded);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Only FTP protocols are supported. URI must start with ftp://", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		// This test connects to a real FTP server and does nothing during the DAT runs. It is for developers only.
		[DeveloperOnlyTest]
		public void TestFtpUploadBadTarget_ConnectToRealFtpServer_DeveloperOnlyNotDat()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			SetupEmail();
			var del = new EDIMessageDelivery();
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Destination = "ftp://badUrl.XXX";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;

			del.Deliver(GetContext(), mode, new DeliveryStreamWrapperUXML(GetStreamForTest()));
			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("ftp://badUrl.XXX", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("Could not upload file after 10 tries with a 5 second pause between each. Giving up", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("Inner Exception:", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("Enterprise Code	: ", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("Machine			: ", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("Port				: ", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		static SubStreamableStream GetStreamForTest()
		{
			var bytesOfCrap = new byte[] { 65, 71, 78, 69, 83 };
			var stream = (SubStreamableStream)new MemoryStream(bytesOfCrap);

			return stream;
		}

		// This test connects to a real FTP server and does nothing during the DAT runs. It is for developers only.
		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTmpOrTempPath", Justification = "Testing")]
		public void TestFtpUploadGoodTarget_ConnectToRealFtpServer_DeveloperOnlyNotDat()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			const string username = "ftpupdates";
			const string password = "9yL$mA";
			const string destination = "ftp://ftp.cargowise.com/TEMP";

			try
			{
				using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes("test stream")))
				{
					var del = new EDIMessageDelivery("B1000000069");
					var mode = Factory.New<EDICommunicationsMode>();
					mode.EK_PortNumber = 21;
					mode.EK_Password = password;
					mode.EK_LoginName = username;
					mode.EK_Filename = "target name here for (*JobNumber*) woo";
					mode.EK_Destination = "ftp://ftp.cargowise.com/TEMP";
					mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
					del.Deliver(GetContext(), mode, new DeliveryStreamWrapperUXML(stream));
					Factory.Save();
					AssertEquals("No error notification emails should be sent out. Check the FTP settings above.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

					var ftpProcessor = new FtpProcessor(destination, username, password, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
					var expectedFileName = "target name here for B1000000069 woo";
					var downloadedFilePath = Path.Combine(Env.TempPath, expectedFileName);
					try
					{
						ftpProcessor.DownloadFile(downloadedFilePath, expectedFileName);
						AssertEquals("test stream", File.ReadAllText(downloadedFilePath));
					}
					finally
					{
						ftpProcessor.DeleteRemoteFile(expectedFileName);
						File.Delete(downloadedFilePath);
					}
				}
			}
			catch (FtpException ex)
			{
				Fail("Check the FTP settings above. Exception: " + ex.Message);
			}
		}

		GlbGroup SetupEmail()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "mickey.mouse@cargowise.com";
			staff.GS_Code = "ZAC";
			Registry.Business.NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();
			return group;
		}

		public void TestFtpUploadFailure()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			SetupEmail();
			var mock = new Mock<IFtpProcessor>();
			mock.Setup(m => m.UploadStreamSeveralAttempts(
				It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
				.Throws(new Exception("We want to see this in the email"));

			using (ObjectFactory.Substitute(mock.Object))
			{
				var del = new EDIMessageDelivery();
				var mode = Factory.New<EDICommunicationsMode>();
				mode.EK_Destination = "ftp://badUrl.XXX";
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				del.Deliver(GetContext(), mode, new DeliveryStreamWrapperUXML(GetStreamForTest()));
				Factory.Save();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("We want to see this in the email", Env.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("ftp://badUrl.XXX", Env.OutgoingMailManager.EmailsCreated[0].Body);
			}
		}

		public void TestFtpUploadSuccess()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			SetupEmail();
			var mock = new Mock<IFtpProcessor>();
			mock.Setup(m => m.UploadFileSeveralAttempts(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));
			mock.Setup(m => m.ListDirectory("")).Returns(new[] { "ediEnterprise Uploaded file.txt" });

			using (ObjectFactory.Substitute(mock.Object))
			{
				var del = new EDIMessageDelivery();
				var mode = Factory.New<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				mode.EK_Destination = "ftp://whatever.com";
				var result = del.Deliver(GetContext(), mode, new DeliveryStreamWrapperUXML(GetStreamForTest()));
				Factory.Save();

				Assert(result.Succeeded);
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestEmailAsAttachment()
		{
			var mode = GetEMAMode();
			var delivery = new EDIMessageDelivery();
			byte[] buffer;
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();

			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmailAsAttachment(Env.OutgoingMailManager.EmailsCreated[0], buffer, mode);
		}

		public void TestEmailAsText()
		{
			var mode = GetEMTMode();
			var delivery = new EDIMessageDelivery();
			byte[] buffer;
			var result = delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();
			AssertEquals(1, result.Length);
			Assert(result[0].Succeeded);
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmailAsBody(Env.OutgoingMailManager.EmailsCreated[0], buffer);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestEmailAsTextNTFMode()
		{
			var mode = GetEMTMode();
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;

			var delivery = new EDIMessageDelivery();
			byte[] buffer;
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();

			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmailAsBody(Env.OutgoingMailManager.EmailsCreated[0], buffer);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestFile()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var mode = GetFileMode(TmpDirectory, "");
			var delivery = new EDIMessageDelivery();
			byte[] buffer;
			var result = delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();
			AssertEquals(1, result.Length);
			Assert(result[0].Succeeded);
			AssertEquals("Shouldn't create an email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var fileName = Path.Combine(mode.EK_Destination, mode.EK_Filename);
			Assert("Should create the target file", File.Exists(fileName));
			AssertEquals("File content is not correct", buffer, File.ReadAllBytes(fileName));
		}

		public void TestFileFailInvalidatesMode()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var tempDir = new TempDirectory();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var mode = GetFileMode(tempDir, "");
			header.EDICommunicationsModes.Add(mode);
			var testTime = ZDateTime.Now.AddHours(-3);
			mode.EK_LastFailed = testTime;
			byte[] buffer;
			TempDirectory.DeleteDirectory(new DirectoryInfo(tempDir.DirectoryName), false);

			System.Threading.Thread.Sleep(100);
			var delivery = new EDIMessageDelivery();
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();
			Assert("Mode should have been invalidated", mode.EK_LastFailed != testTime);
		}

		public void TestFileFailSendsEmailOnlyEveryTwoHours()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var tempDir = new TempDirectory();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var mode = GetFileMode(tempDir, "");
			header.EDICommunicationsModes.Add(mode);
			byte[] buffer;
			TempDirectory.DeleteDirectory(new DirectoryInfo(tempDir.DirectoryName), false);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var stream = GetStream(out buffer);

			SetupEmail();

			var testTime = ZDateTime.Now.AddHours(-3);
			mode.EK_LastFailed = testTime;
			System.Threading.Thread.Sleep(100);
			var delivery = new EDIMessageDelivery();
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(stream));
			AssertEquals("1 email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(stream));
			AssertEquals("shouldnt' create a new email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			mode.EK_LastFailed = mode.EK_LastFailed.AddHours(-1);
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(stream));
			AssertEquals("shouldnt' create a new email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			mode.EK_LastFailed = mode.EK_LastFailed.AddHours(-2);
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(stream));
			AssertEquals("should create a new email", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			Factory.Save();
		}

		public void TestFileFailSendsEmailToRegisteryGroup()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OHD";
			orgHeader.OH_FullName = "FullName";

			var tempDir = new TempDirectory();
			var mode = GetFileMode(tempDir, "");
			TempDirectory.DeleteDirectory(new DirectoryInfo(tempDir.DirectoryName), false);
			orgHeader.EDICommunicationsModes.Add(mode);

			var group = SetupEmail();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			byte[] buffer;
			var delivery = new EDIMessageDelivery();
			Registry.Business.NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();
			mode.EK_LastFailed = ZDateTime.Now.AddHours(-3);
			System.Threading.Thread.Sleep(100);

			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();

			AssertEquals("It should create an email for edi delivery fail", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Workflow process delivery failed", email.Subject);
			AssertContains("Wrong email body", "File name\t\t\t: " + mode.EK_Filename, email.Body);
			AssertContains("Wrong email body", "Destination\t\t: " + mode.EK_Destination, email.Body);
			AssertContains("Wrong email body", "Organization		:  <a href=\"", email.Body);
			AssertContains("Wrong email body", mode.Organisation.OH_Code + " - " + mode.Organisation.OH_FullName, email.Body);
			AssertEquals("Wrong recipients", 1, email.Recipients.Count);
			AssertEquals("Wrong recipients", "mickey.mouse@cargowise.com", email.Recipients[0].Email);
		}

		public void TestFileWith_datetime_Replace()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var mode = GetFileMode(TmpDirectory, "_(*datetime*)");
			var delivery = new EDIMessageDelivery();
			byte[] buffer;
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();

			var files = Directory.GetFiles(TmpDirectory.DirectoryName, "*.*");
			AssertEquals("Shouldn't create an email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Should create only one file", 1, files.Length);
			AssertContains("Wrong file name", "SaveAsFileMode", files[0]);
			AssertEquals("File content is not correct", buffer, File.ReadAllBytes(Path.Combine(TmpDirectory, files[0])));
		}

		public void TestMultipleMessages()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			byte[] buffer;
			var stream = GetStream(out buffer);

			var modes = new[] { GetEMAMode(), GetEMTMode(), GetFileMode(TmpDirectory, "_(*DateTime*)"), GetFileMode(TmpDirectory, "") };
			var delivery = new EDIMessageDelivery();
			delivery.Deliver(GetContext(), modes, new DeliveryStreamWrapperUXML(stream));
			Factory.Save();

			AssertEquals("Should create 2 emails", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertEmailAsAttachment(Env.OutgoingMailManager.EmailsCreated[0], buffer, modes[0]);
			AssertEmailAsBody(Env.OutgoingMailManager.EmailsCreated[1], buffer);

			var files = Directory.GetFiles(TmpDirectory.DirectoryName, "*.*");

			AssertEquals("Should create two files", 2, files.Length);
			AssertEquals("Wrong file name", "SaveAsFileMode.txt", Path.GetFileName(files[0]));
			AssertContains("Wrong file name", "SaveAsFileMode", Path.GetFileName(files[1]));
			AssertEquals("File content is not correct", buffer, File.ReadAllBytes(files[1]));
			AssertEquals("File content is not correct", buffer, File.ReadAllBytes(files[0]));
		}

		public void TestFilesHaveDifferentNames()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			byte[] buffer;
			var stream = GetStream(out buffer);
			var modes = new[] { GetFileMode(TmpDirectory, "_(*DateTime*)"), GetFileMode(TmpDirectory, "_(*DateTime*)") };
			var delivery = new EDIMessageDelivery();
			delivery.Deliver(GetContext(), modes, new DeliveryStreamWrapperUXML(stream));
			Factory.Save();
			var files = Directory.GetFiles(TmpDirectory.DirectoryName, "*.*");
			AssertEquals("Should create two files", 2, files.Length);
			AssertNotEquals("Shouldn't have same name", Path.GetFileName(files[0]), Path.GetFileName(files[1]));
		}

		public void TestPartialStream()
		{
			var buffer1 = new byte[256];
			var buffer2 = new byte[1024];

			new Random(10).NextBytes(buffer1);
			new Random(123).NextBytes(buffer2);
			var stream = (SubStreamableStream)new MemoryStream();
			stream.Write(buffer1, 0, buffer1.Length);
			stream.Write(buffer2, 0, buffer2.Length);

			stream.Flush();
			stream.Position = buffer1.Length;

			var mode = GetEMAMode();
			var mode2 = GetEMAMode();

			var delivery = new EDIMessageDelivery();
			delivery.Deliver(GetContext(), new[] { mode, mode2 }, new DeliveryStreamWrapperUXML(stream));
			Factory.Save();
			AssertEquals("Should write stream from current position", buffer2, Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].Data);
			AssertEquals("Should write stream from current position", buffer2, Env.OutgoingMailManager.EmailsCreated[1].Attachments[0].Data);
			AssertEquals("Should leave stream at the end", stream.Position, stream.Length);
		}

		[TestDate(2007, 1, 1, 23, 43, 12)]
		public void TestAttachmentName_SubstitutesDateTime()
		{
			var mode = GetEMAMode();
			mode.EK_ServerAddressSubject = "subject";
			mode.EK_Filename = "(*datetime*).txt";

			var delivery = new EDIMessageDelivery();
			byte[] buffer;

			AssertEquals("No Email is created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();

			AssertEquals("Email is created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email has attachment", true, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count > 0);
			AssertEquals("Attachment name", "200701012343120000.txt", Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].DisplayName);
		}

		public void TestFileName_SubstitutesJobNumber()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var mode = GetFileMode(TmpDirectory, "(*JobNumber*)");
			var delivery = new EDIMessageDelivery("Guess_a_number");
			byte[] buffer;

			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();

			AssertEquals("Shouldn't create an email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var fileName = Path.Combine(mode.EK_Destination, "SaveAsFileModeGuess_a_number.txt");
			Assert("Should create the target file", File.Exists(fileName));
			AssertEquals("File content is not correct", buffer, File.ReadAllBytes(fileName));
		}

		public void TestFileName_ExtraSubstitution()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var mode = GetFileMode(TmpDirectory, "(*JobNumber*)");
			var delivery = new EDIMessageDelivery("Guess_a_number");
			delivery.ExtraDataSubstitution = (CommunicationModeSubstitutorProperty property, ZString data) => "Guess_a_text" + data;
			byte[] buffer;

			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();

			AssertEquals("Shouldn't create an email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			string fileName = Path.Combine(mode.EK_Destination, "Guess_a_textSaveAsFileModeGuess_a_number.txt");
			Assert("Should create the target file", File.Exists(fileName));
			AssertEquals("File content is not correct", buffer, File.ReadAllBytes(fileName));
		}

		public void TestSubject_SubstitutesJobNumberAndOrganisation()
		{
			var mode = GetEMTMode();
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			mode.EK_ServerAddressSubject = "subject(*jobnumber*) (*organisation*)";
			var delivery = new EDIMessageDelivery("Guess_a_number");
			byte[] buffer;

			delivery.Deliver(GetContext(), new[] { mode }, new DeliveryStreamWrapperUXML(GetStream(out buffer)));
			Factory.Save();

			AssertEmailAsBody(Env.OutgoingMailManager.EmailsCreated[0], buffer, string.Format("subjectGuess_a_number {0}", mode.Organisation.OH_FullName));
		}

		public void TestDeliverMessageToEHubAndEAdaptor()
		{
			using (Factory.AddDisposableService())
			{
				var entity = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var delivery = new EDIMessageDelivery();
				var interchangesBefore = Factory.Load<IEDIInterchange>(new ZQuery());

				var result = delivery.Deliver(new DeliveryContext(Factory) { ApplicationCode = "UDM", MessageTypeCode = EDIMessageTypeList.Codes.XDC, MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment, Notifications = new NotificationBuffer(), ParentInfo = new DummyEntityInfo(), TriggerObjectInfo = new DummyEntityInfo() },
								  new IEDICommunicationsMode[] { GetEHubMode(), GetEAdaptorMode() }, new DeliveryStreamWrapperUXML(GetStream("<Root>Some text</Root>")));

				AssertEquals(2, result.Length);
				AssertEquals(true, result[0].Succeeded);
				AssertEquals(true, result[1].Succeeded);

				var interchangesAfter = Factory.Load<IEDIInterchange>(new ZQuery());
				AssertEquals("2 Interchange should be created", interchangesBefore.Length + 2, interchangesAfter.Length);
				Factory.Save(); // Because save is tracked.
			}
		}

		public void TestDeliverMessageToEHubAndEAdaptor_Failure()
		{
			using (Factory.AddDisposableService())
			{
				var entity = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var delivery = new EDIMessageDelivery();
				var interchangesBefore = Factory.Load<IEDIInterchange>(new ZQuery());

				var result = delivery.Deliver(new DeliveryContext(Factory) { ApplicationCode = "UDM", MessageTypeCode = EDIMessageTypeList.Codes.XDC, MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment, Notifications = new NotificationBuffer(), ParentInfo = new DummyEntityInfo(), TriggerObjectInfo = new DummyEntityInfo() },
								  new IEDICommunicationsMode[] { GetEHubMode(), GetEAdaptorMode() }, new DeliveryStreamWrapperUXML(GetStream("Some text"), new DummyEntityInfo()));

				AssertEquals(2, result.Length);
				AssertEquals(false, result[0].Succeeded);
				AssertEquals(false, result[1].Succeeded);

				var interchangesAfter = Factory.Load<IEDIInterchange>(new ZQuery());
				AssertEquals("0 Interchange should be created", interchangesBefore.Length, interchangesAfter.Length);
				Factory.Save(); // Because save is tracked.
			}
		}

		public void TestDeliverBatch_EServices()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
				TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

				var entity1 = Factory.NewWithValidTestData<OrgHeader>();
				var entity2 = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var context = new DeliveryContext(Factory)
				{
					ApplicationCode = "UDM",
					MessageTypeCode = EDIMessageTypeList.Codes.XDC,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
					Notifications = new NotificationBuffer(),
					ParentInfo = new DummyEntityInfo(),
					TriggerObjectInfo = new DummyEntityInfo()
				};

				var steamWrapper1 = new DeliveryStreamWrapperUXML(GetStream("<Root>First</Root>"), EntityInfo.New(entity1));
				var steamWrapper2 = new DeliveryStreamWrapperUXML(GetStream("<Root>Second</Root>"), EntityInfo.New(entity2));
				var steamWrapper1Copy = new DeliveryStreamWrapperUXML(GetStream("<Root>First</Root>"), EntityInfo.New(entity1));
				var steamWrapper2Copy = new DeliveryStreamWrapperUXML(GetStream("<Root>Second</Root>"), EntityInfo.New(entity2));

				var delivery = new EDIMessageDelivery();
				Assert(delivery.DeliverBatch(context, GetEHubMode(), new[] { steamWrapper1, steamWrapper2 }).Succeeded);
				Factory.Save();
				Assert(delivery.DeliverBatch(context, GetEAdaptorMode(), new[] { steamWrapper1Copy, steamWrapper2Copy }).Succeeded);
				Factory.Save();

				var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
				var messages = Factory.Load<IEDIMessage>(new ZQuery());

				AssertEquals("2 interchanges should be created", 2, interchanges.Length);
				AssertEquals("4 messages should be created", 4, messages.Length);

				AssertEquals("2 messages for each interchange", 2, messages.Count(message => message.Interchange == interchanges[0]));
				AssertEquals("2 messages for each interchange", 2, messages.Count(message => message.Interchange == interchanges[1]));
			}
		}

		public void TestDeliverBatch_DeliveryMustImplementIBatchDelivery()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			var entity1 = Factory.NewWithValidTestData<OrgHeader>();
			var entity2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var context = new DeliveryContext(Factory)
			{
				ApplicationCode = "UDM",
				MessageTypeCode = EDIMessageTypeList.Codes.XDC,
				MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				Notifications = new NotificationBuffer(),
				ParentInfo = new DummyEntityInfo(),
				TriggerObjectInfo = new DummyEntityInfo()
			};

			var steamWrapper1 = new DeliveryStreamWrapperUXML(GetStream("<Root>First</Root>"), EntityInfo.New(entity1));
			var steamWrapper2 = new DeliveryStreamWrapperUXML(GetStream("<Root>Second</Root>"), EntityInfo.New(entity2));

			var supportedModes = new[]
				{
					EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface,
					EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
					EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface
				};

			var excludedModes = new[]
				{
					EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss,
					EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector
				};

			var notSupportedModes = new EDICommunicationsModeCommunicationsTransportList()
				.ToArray()
				.Where(m => !supportedModes.Contains(m.Code) && !excludedModes.Contains(m.Code))
				.Select(m => m.Code);

			foreach (string deliveryMode in notSupportedModes)
			{
				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = deliveryMode;

				try
				{
					var delivery = new EDIMessageDelivery();
					delivery.DeliverBatch(context, mode, new[] { steamWrapper1, steamWrapper2 });

					Fail("Exception should be thrown when calling DeliverBatch() for non-supported communication modes");
				}
				catch (InvalidOperationException ex)
				{
					AssertEquals(string.Format("Batch delivery is not supported for {0} mode", mode.EK_CommunicationsTransport), ex.Message);
				}
			}
		}

		public DeliveryContext GetContext() => new DeliveryContext(Factory);

		class DummyEntityInfo : IEntityInfo
		{
			public Guid InternalPK
			{
				get { return new Guid("0DFDDD16-A6D8-4422-8E02-77539815F6C8"); }
			}

			public string TableName
			{
				get { return "TestTable"; }
			}

			public Type Type
			{
				get { return typeof(OrgHeader); }
			}
		}

		SubStreamableStream GetStream(string text)
		{
			byte[] byteArray = Encoding.ASCII.GetBytes(text);
			return (SubStreamableStream)new MemoryStream(byteArray);
		}

		SubStreamableStream GetStream(out byte[] buffer)
		{
			MemoryStream stream = new MemoryStream();
			System.Threading.Thread.Sleep(10);
			buffer = new byte[Random.Next(10, 1024)];
			random.NextBytes(buffer);
			stream.Write(buffer, 0, buffer.Length);
			stream.Flush();
			stream.Position = 0;
			return (SubStreamableStream)stream;
		}

		EDICommunicationsMode GetEHubMode()
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			mode.EK_Destination = "HYEDAUIKB";
			return mode;
		}

		EDICommunicationsMode GetEAdaptorMode()
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			mode.EK_Destination = "HYEDAUIKB";
			return mode;
		}

		EDICommunicationsMode GetEMAMode()
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@ema.com";
			mode.EK_ServerAddressSubject = "EmailAsAttchMode";
			return mode;
		}

		EDICommunicationsMode GetEMTMode()
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "test@emb.com";
			mode.EK_ServerAddressSubject = "EmailAsBodyMode";
			mode.EK_ParentID = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return mode;
		}

		EDICommunicationsMode GetFileMode(TempDirectory tempDir, string replaceString)
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
			mode.EK_Destination = tempDir.DirectoryName;
			mode.EK_Filename = "SaveAsFileMode" + replaceString + ".txt";
			return mode;
		}

		void AssertEmailAsBody(EmailDef email, byte[] buffer, string subject)
		{
			AssertEquals("Should send it to correct recepient", true, email.Recipients.Contains("test@emb.com"));
			AssertEquals("Should send it to correct recepients", 1, email.Recipients.Count);
			Assert("Should not be sent for System Communication", !email.Recipients[0].IsForSystemCommunication);
			AssertEquals("Incorrect email subject", subject, email.Subject);
			if (email is HtmlEmailDef)
			{
				AssertContains("Should send correct email body", Encoding.UTF8.GetString(buffer), email.Body);
			}
			else
			{
				AssertEquals("Should send correct email body", Encoding.UTF8.GetString(buffer), email.Body);
			}
		}

		void AssertEmailAsBody(EmailDef email, byte[] buffer)
		{
			AssertEmailAsBody(email, buffer, "EmailAsBodyMode");
		}

		void AssertEmailAsAttachment(EmailDef email, byte[] buffer, EDICommunicationsMode mode)
		{
			AssertEquals("Should send it to correct recepient", true, email.Recipients.Contains("test@ema.com"));
			AssertEquals("Should send it to correct recepients", 1, email.Recipients.Count);
			Assert("Should not be sent for System Communication", !email.Recipients[0].IsForSystemCommunication);
			AssertEquals("Incorrect email subject", "EmailAsAttchMode", email.Subject);
			AssertEquals("Didn't create correct attachment", buffer, email.Attachments[0].Data);
			AssertEquals("Wrong attachment name", mode.EK_Filename, email.Attachments[0].DisplayName);
		}

		Random Random
		{
			get { return random ?? (random = new Random()); }
		}
		Random random;

		TempDirectory TmpDirectory
		{
			get { return directory ?? (directory = new TempDirectory()); }
		}
		TempDirectory directory;

		protected override void TearDown()
		{
			base.TearDown();
			if (directory != null)
			{
				directory.Dispose();
			}
		}
	}
}
