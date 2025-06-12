using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common;
using CargoWise.eHub.BizTalkAdapters.FtpEx;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class TransferrerTransmitterEndpointTest : TestBase
	{
		[TestMethod()]
		public void TransferrerTransmitterEndpoint_ProcessMessageTest()
		{
			string propertyNamespace = "http://properties";
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("Log", "LOG"),
								new XElement("TargetFileName", "%OverrideFilename%"),
								new XElement("TemporaryPath", String.Empty)
								);
			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			context.Expect(x => x.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("OVERRIDEFILENAME");
			context.Expect(x => x.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PORTNAME");
			var stubFtpTransferrer = MockRepository.GenerateMock<ITransferrer, IDisposable>();
			var mockTransferrerFactory = MockRepository.GenerateMock<ITransferrerFactory>();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(stubFtpTransferrer);
			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			var messageData = new MemoryStream();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(messageData);
			var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
			stubMessage.Context = context;
			var stubEndpointParms = MockRepository.GenerateStub<EndpointParameters>("FtpEx://USER@SERVER:21/PATH/TARGETFILENAME");
			stubEndpointParms.Stub(x => x.SessionKey).Return("SESSIONKEY");
			AsyncTransmitter asyncTransmitter = new FtpExTransmitter();
			IBaseMessage actual;

			var target = new FtpExTransmitterEndpoint(asyncTransmitter, mockTransferrerFactory);
			target.Open(stubEndpointParms, null, propertyNamespace);
			actual = target.ProcessMessage(stubMessage);

			Assert.IsNull(actual);
			stubFtpTransferrer.AssertWasCalled(x => x.Open());
			stubFtpTransferrer.AssertWasCalled(x => x.PutFile("FOLDER/OVERRIDEFILENAME", messageData));
			stubFtpTransferrer.AssertWasNotCalled(X => X.Close());
		}

		[TestMethod()]
		public void TransferrerTransmitterEndpoint_ProcessMessageRenameTest()
		{
			string propertyNamespace = "http://properties";
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("Log", "LOG"),
								new XElement("TargetFileName", "%DestinationPartyQualifier%"),
								new XElement("TemporaryFolder", "TEMP"),
								new XElement("TemporaryFileName", "%DestinationPartyQualifier%")
								);
			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			context.Expect(x => x.Read("DestinationPartyQualifier", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("DESTINATIONPARTYQUALIFIER");
			context.Expect(x => x.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PORTNAME");
			var stubFtpTransferrer = MockRepository.GenerateMock<ITransferrer, IDisposable>();
			var mockTransferrerFactory = MockRepository.GenerateMock<ITransferrerFactory>();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(stubFtpTransferrer);
			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			var messageData = new MemoryStream();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(messageData);
			var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
			stubMessage.Context = context;
			var stubEndpointParms = MockRepository.GenerateStub<EndpointParameters>("FtpEx://USER@SERVER:21/PATH/TARGETFILENAME");
			stubEndpointParms.Stub(x => x.SessionKey).Return("SESSIONKEY");
			AsyncTransmitter asyncTransmitter = new FtpExTransmitter();
			IBaseMessage actual;

			var target = new FtpExTransmitterEndpoint(asyncTransmitter, mockTransferrerFactory);
			target.Open(stubEndpointParms, null, propertyNamespace);
			actual = target.ProcessMessage(stubMessage);

			Assert.IsNull(actual);
			stubFtpTransferrer.AssertWasCalled(x => x.Open());
			stubFtpTransferrer.AssertWasCalled(x => x.PutFile("TEMP/DESTINATIONPARTYQUALIFIER", messageData));
			stubFtpTransferrer.AssertWasCalled(x => x.RenameFile("TEMP/DESTINATIONPARTYQUALIFIER", "FOLDER/DESTINATIONPARTYQUALIFIER"));
			stubFtpTransferrer.AssertWasNotCalled(x => x.Close());
		}

		[TestMethod()]
		public void TransferrerTransmitterEndpoint_MessageIDMacro()
		{
			string propertyNamespace = "http://properties";
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("Log", "LOG"),
								new XElement("TargetFileName", "%MessageID%")
								);
			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			context.Expect(x => x.Read("InterchangeID", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("{00000000-0000-0000-0000-000000000000}");
			context.Expect(x => x.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PORTNAME");
			var stubFtpTransferrer = MockRepository.GenerateMock<ITransferrer, IDisposable>();
			var mockTransferrerFactory = MockRepository.GenerateMock<ITransferrerFactory>();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(stubFtpTransferrer);
			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			var messageData = new MemoryStream();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(messageData);
			var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
			stubMessage.Context = context;
			stubMessage.Stub(x => x.MessageID).Return(new Guid("11111111-1111-1111-1111-111111111111"));
			var stubEndpointParms = MockRepository.GenerateStub<EndpointParameters>("FtpEx://USER@SERVER:21/PATH/TARGETFILENAME");
			stubEndpointParms.Stub(x => x.SessionKey).Return("SESSIONKEY");
			AsyncTransmitter asyncTransmitter = new FtpExTransmitter();
			IBaseMessage actual;

			var target = new FtpExTransmitterEndpoint(asyncTransmitter, mockTransferrerFactory);
			target.Open(stubEndpointParms, null, propertyNamespace);
			actual = target.ProcessMessage(stubMessage);

			Assert.IsNull(actual);
			stubFtpTransferrer.AssertWasCalled(x => x.Open());
			stubFtpTransferrer.AssertWasCalled(x => x.PutFile("FOLDER/{11111111-1111-1111-1111-111111111111}", messageData));
			stubFtpTransferrer.AssertWasNotCalled(x => x.Close());
		}

		[TestMethod()]
		public void TransferrerTransmitterEndpoint_ExceptionShouldNotIncludeMessageID()
		{
			string propertyNamespace = "http://properties";
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("Log", "LOG"),
								new XElement("TargetFileName", "%MessageID%")
								);
			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			context.Expect(x => x.Read("InterchangeID", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("{00000000-0000-0000-0000-000000000000}");
			context.Expect(x => x.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("PORTNAME");
			var stubFtpTransferrer = MockRepository.GenerateMock<ITransferrer, IDisposable>();
			var mockTransferrerFactory = MockRepository.GenerateMock<ITransferrerFactory>();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(stubFtpTransferrer);
			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			var messageData = new MemoryStream();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(messageData);
			var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
			stubMessage.Context = context;
			stubMessage.Stub(x => x.MessageID).Return(new Guid("11111111-1111-1111-1111-111111111111"));
			var stubEndpointParms = MockRepository.GenerateStub<EndpointParameters>("FtpEx://USER@SERVER:21/PATH/TARGETFILENAME");
			stubEndpointParms.Stub(x => x.SessionKey).Return("SESSIONKEY");
			AsyncTransmitter asyncTransmitter = new FtpExTransmitter();
			IBaseMessage actual;
			stubFtpTransferrer.Stub(x => x.PutFile("FOLDER/{11111111-1111-1111-1111-111111111111}", messageData)).Throw(new Exception("Unexpected exception.")).Repeat.Once();
			stubFtpTransferrer.Stub(x => x.PutFile("FOLDER/{11111111-1111-1111-1111-111111111111}", messageData)).Throw(new OperationCanceledException("OperationCanceledException exception.")).Repeat.Once();

			var target = new FtpExTransmitterEndpoint(asyncTransmitter, mockTransferrerFactory);
			target.Open(stubEndpointParms, null, propertyNamespace);
			try
			{
				actual = target.ProcessMessage(stubMessage);
				Assert.Fail("Exception should be thrown.");
			}
			catch(AdapterException ex)
            {
				Assert.IsTrue(ex.Message.StartsWith("Error in send port 'PORTNAME': System.Exception: Unexpected exception."));
            }

			try
			{
				actual = target.ProcessMessage(stubMessage);
				Assert.Fail("Exception should be thrown.");
			}
			catch (AdapterException ex)
			{
				Assert.AreEqual("Canceled send in send port 'PORTNAME'. It will be retried when host instance and/or send port is restarted.", ex.Message);
			}

			stubFtpTransferrer.AssertWasCalled(x => x.Open());
			stubFtpTransferrer.AssertWasNotCalled(x => x.Close());
		}
	}
}
