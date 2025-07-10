using System;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.IO.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2.Test
{
	public class TraxonCargoIMPPhase2InterchangeSenderTest : TestCaseWithFactory
	{
		public void TestSendInterchange_Success()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PIMAADDR");
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message1 = messages.AddNew();
			message1.EM_MessageText = "MESSAGE1" + EDIMessage.MessageNumberPlaceHolder;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
			message1.EM_MessageType = "RMI";
			message1.EM_MessageOwner = string.Empty;
			message1.MessageNumberStrategy = new TestMessageNumberStrategy();
			Factory.Save();
			fSender.ExecuteBatch();
			string fileName = "00000001.msg";
			AssertContains("UNB+TRXA:1+PIMAADDR:PIMA+REUAGT82YAS:PIMA+", GetDownloadedFileContent(fileName));
			AssertContains("Interchange #1 being sent.", fLogger.ToString());
			AssertContains("Interchange #1 successfully sent.", fLogger.ToString());
		}

		public void TestSendInterchange_NoFTPSettings()
		{
			EDIMessage message = fInterchange.ContainedMessages.AddNew();
			message.EM_MessageText = "ZZZZ";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			bool response = fSender.SendInterchange(fInterchange);
			AssertEquals(false, response);
			AssertContains("Traxon Server, Username, Password", fLogger.ToString());
		}

		[TestDate(2008, 2, 2, 10, 10, 10)]
		public void TestSendInterchange_IncorrectFTP()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ftp.example.corporate.cargowise.com");
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message1 = messages.AddNew();
			message1.EM_MessageText = "MESSAGE1" + EDIMessage.MessageNumberPlaceHolder;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
			message1.EM_MessageType = "RMI";
			message1.EM_MessageOwner = string.Empty;
			message1.MessageNumberStrategy = new TestMessageNumberStrategy();
			Factory.Save();
			ZQuery interchangeFilter = new ZQuery();
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.CargoIMPPhase2);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			EDIInterchange interchange = null;
			fLogger.ClearLog();
			fSender.ExecuteBatch();
			Assert(fLogger.Count > 3);
			AssertContains("Interchange #1 being sent.", fLogger[1]);
			AssertContains("Error message", "The remote name could not be resolved", fLogger[2]);
			AssertContains("Interchange #1 failed.", fLogger[3]);
			interchange = new BusinessObjectFactory().LoadTop1<EDIInterchange>(interchangeFilter);
			AssertNotNull("InterchangeCreated", interchange);
			AssertEquals("Interchange Status", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals(ZDateTime.Empty, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonLastFTPFailureTime.Value);
			AssertEquals(1, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value);
			for (int i = 0; i < ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPMaxFailureCount.Value - 2; i++)
			{
				fLogger.ClearLog();
				fSender.ExecuteBatch();
				AssertContains("Interchange #1 being sent.", fLogger[0]);
				AssertContains("Error message", "The remote name could not be resolved", fLogger[1]);
				AssertContains("Interchange #1 failed.", fLogger[2]);
				interchange = new BusinessObjectFactory().LoadTop1<EDIInterchange>(interchangeFilter);
				AssertNotNull("InterchangeCreated", interchange);
				AssertEquals("Interchange Status", EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals(ZDateTime.Empty, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonLastFTPFailureTime.Value);
				AssertEquals(i + 2, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value);
			}

			fLogger.ClearLog();
			fSender.ExecuteBatch();
			AssertContains("Interchange #1 being sent.", fLogger[0]);
			AssertContains("Error message", "The remote name could not be resolved", fLogger[1]);
			AssertContains("Interchange #1 failed.", fLogger[2]);
			interchange = new BusinessObjectFactory().LoadTop1<EDIInterchange>(interchangeFilter);
			AssertNotNull("InterchangeCreated", interchange);
			AssertEquals("Interchange Status", EDIInterchange.Status.Failed, interchange.EI_Status);
			AssertEquals(ZDateTime.Now, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonLastFTPFailureTime.Value);
			AssertEquals(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPMaxFailureCount.Value, ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPFailureCount.Value);
		}

		public void TestGetRemoteFilePath_FileNamePadding()
		{
			var message = fInterchange.ContainedMessages.AddNew();
			message.EM_MessageNum = "1";
			AssertEquals("00000001.msg", GetRemoteFileNamePathFromInterchange());
			message.EM_MessageNum = "0001";
			AssertEquals("00000001.msg", GetRemoteFileNamePathFromInterchange());
			message.EM_MessageNum = "as";
			AssertEquals("000000as.msg", GetRemoteFileNamePathFromInterchange());
		}

		public void TestGetRemoteFilePath_DirectoryName()
		{
			var message = fInterchange.ContainedMessages.AddNew();
			message.EM_MessageNum = "TEST0000";
			using (ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPInboundDirectory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MSGFolder"))
			{
				AssertEquals(@"MSGFolder/TEST0000.msg", GetRemoteFileNamePathFromInterchange());
			}
		}

		public void TestGetRemoteFilePath_DirectoryName_WhenDirectoryNameIsNull()
		{
			var message = fInterchange.ContainedMessages.AddNew();
			message.EM_MessageNum = "TEST0000";
			using (ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPInboundDirectory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AssertEquals("TEST0000.msg", GetRemoteFileNamePathFromInterchange());
			}
		}

		public void TestGetRemoteFilePath_DirectoryName_WhenDirectoryNameIsEmptySpace()
		{
			var message = fInterchange.ContainedMessages.AddNew();
			message.EM_MessageNum = "TEST0000";
			using (ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPInboundDirectory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "    "))
			{
				AssertEquals("TEST0000.msg", GetRemoteFileNamePathFromInterchange());
			}
		}

		string GetRemoteFileNamePathFromInterchange()
		{
			var method = typeof(TraxonCargoIMPPhase2InterchangeSender).GetMethod("GetRemoteFilePath", BindingFlags.NonPublic | BindingFlags.Instance);
			return method.Invoke(fSender, new object[] { fInterchange }).ToString();
		}

		[SnailTest]
		public void TestSendInterchangeWithRecentPODs()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PIMAADDR");
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			numberStrategy = new CargoIMPPhase2MessageNumberStrategy();
			CreateTestMessage(messages, "MESSAGE1" + EDIMessage.MessageNumberPlaceHolder);
			CreateTestMessage(messages, "MESSAGE2\r\nSTS,POD," + EDIMessage.MessageNumberPlaceHolder);
			CreateTestMessage(messages, "MESSAGE3" + EDIMessage.MessageNumberPlaceHolder);
			CreateTestMessage(messages, "MESSAGE4" + EDIMessage.MessageNumberPlaceHolder + "\r\nSTS,POD,bla");
			CreateTestMessage(messages, "MESSAGE5" + EDIMessage.MessageNumberPlaceHolder);
			Factory.Save();
			fSender.ExecuteBatch();
			ZString fileContent = GetDownloadedFileContent("00000001.msg");
			AssertContains("MESSAGE1", fileContent);
			try
			{
				fileContent = GetDownloadedFileContent("00000002.msg");
				Fail("Interchange should not be sent");
			}
			catch
			{
			}

			fileContent = GetDownloadedFileContent("00000003.msg");
			AssertContains("MESSAGE3", fileContent);
			try
			{
				fileContent = GetDownloadedFileContent("00000004.msg");
				Fail("Interchange should not be sent");
			}
			catch
			{
			}

			fileContent = GetDownloadedFileContent("00000005.msg");
			AssertContains("MESSAGE5", fileContent);
		}

		[SnailTest]
		public void TestSendInterchangeWithOldPODs()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PIMAADDR");
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			numberStrategy = new CargoIMPPhase2MessageNumberStrategy();
			CreateTestMessage(messages, "MESSAGE1" + EDIMessage.MessageNumberPlaceHolder);
			var pOD1 = CreateTestMessage(messages, "MESSAGE2\r\nSTS,POD," + EDIMessage.MessageNumberPlaceHolder);
			CreateTestMessage(messages, "MESSAGE3" + EDIMessage.MessageNumberPlaceHolder);
			var pOD2 = CreateTestMessage(messages, "MESSAGE4" + EDIMessage.MessageNumberPlaceHolder + "\r\nSTS,POD,bla");
			CreateTestMessage(messages, "MESSAGE5" + EDIMessage.MessageNumberPlaceHolder);
			Factory.Save();
			pOD1.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-8);
			pOD2.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-7);
			Factory.Save();
			fSender.ExecuteBatch();
			ZString fileContent = GetDownloadedFileContent("00000001.msg");
			AssertContains("MESSAGE1", fileContent);
			fileContent = GetDownloadedFileContent("00000002.msg");
			AssertContains("MESSAGE2", fileContent);
			fileContent = GetDownloadedFileContent("00000003.msg");
			AssertContains("MESSAGE3", fileContent);
			fileContent = GetDownloadedFileContent("00000004.msg");
			AssertContains("MESSAGE4", fileContent);
			fileContent = GetDownloadedFileContent("00000005.msg");
			AssertContains("MESSAGE5", fileContent);
		}

		public void TestNoExceptionOnMissingMessage()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ftp.example.corporate.cargowise.com");
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			fInterchange.EI_Status = EDIInterchange.Status.Queued;
			fInterchange.EI_ApplicationCode = "CI2";
			fInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			Factory.Save();
			AssertNoExceptionThrown("Exception should not be thrown", () => fSender.ExecuteBatch());
			AssertContains("Warning|Only Cargo2000 Phase 2 Interchanges containing messages can be sent using this sender.", fLogger[0]);
			AssertContains("Debug|Interchange #0001 is malformed and cannot be sent.", fLogger[1]);
			ZQuery interchangeFilter = new ZQuery();
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.CargoIMPPhase2);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			EDIInterchange interchange = new BusinessObjectFactory().LoadTop1<EDIInterchange>(interchangeFilter);
			AssertNotNull("InterchangeCreated", interchange);
			AssertEquals("Interchange status is set to 'Failed'", EDIInterchange.Status.Failed, interchange.EI_Status);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ftpTestHelper = new FtpTestHelper();
			ftpTestHelper.Start();
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ftpTestHelper.ServerAddress.ToString());
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ftpTestHelper.UserName);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonFTPPasswordEncrypted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ftpTestHelper.Password);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TPID");
			fLogger = new TestServiceLogger();
			fSender = new TraxonCargoIMPPhase2InterchangeSenderForTest(fLogger);
			fInterchange = Factory.New<EDIInterchange>();
			fInterchange.EI_InterchangeNum = "0001";
		}

		protected override void TearDown()
		{
			ftpTestHelper.Dispose();
			base.TearDown();
		}

		EDIMessage CreateTestMessage(NonDependentEDIMessageCollection messages, ZString messageText)
		{
			EDIMessage message = messages.AddNew();
			message.EM_MessageText = messageText;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
			message.EM_MessageType = "MSU";
			message.EM_MessageOwner = string.Empty;
			message.MessageNumberStrategy = numberStrategy;
			return message;
		}

		ZString GetDownloadedFileContent(string fileName)
		{
			FtpProcessor processor = new FtpProcessor(ftpTestHelper.ServerAddress.ToString(), ftpTestHelper.UserName, ftpTestHelper.Password, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
			try
			{
				using (TempFile localFile = TempFile.New())
				{
					processor.DownloadFile(localFile.Filename, fileName);
					return File.ReadAllText(localFile.Filename);
				}
			}
			finally
			{
				processor.DeleteRemoteFile(fileName);
			}
		}

		FtpTestHelper ftpTestHelper;
		EDIInterchange fInterchange;
		TraxonCargoIMPPhase2InterchangeSenderForTest fSender;
		TestServiceLogger fLogger;
		CargoIMPPhase2MessageNumberStrategy numberStrategy;

		class TraxonCargoIMPPhase2InterchangeSenderForTest : TraxonCargoIMPPhase2InterchangeSender
		{
			public TraxonCargoIMPPhase2InterchangeSenderForTest(ILogger logger) : base(logger)
			{
			}

			public new bool SendInterchange(EDIInterchange interchange)
			{
				return base.SendInterchange(interchange);
			}
		}

		public class CargoIMPPhase2MessageNumberStrategy : IMessageNumberStrategy
		{
			int sequence;

			#region IMessageNumberStrategy Members

			public string GetMessageReferenceNumber()
			{
				sequence++;
				return sequence.ToString();
			}

			#endregion IMessageNumberStrategy Members
		}

		#endregion Implementation
	}
}
