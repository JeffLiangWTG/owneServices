using CargoWise.eHub.BizTalkAdapters.WSHttpEx;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;
using System.IO;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass]
	public class WSHttpExTransmitterEndpointTest
	{
		[TestMethod]
		public void WSHttpExTransmitterEndpoint_ProcessMessage()
		{
			var stubMessage = WSHttpExPropertiesTest.CreateMessage(null, true);
			var transmitter = new WSHttpExTransmitter();
			var stubTransferrer = MockRepository.GenerateMock<IHttpExTransferrer, IDisposable>();
			var responseMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubTransferrer.Expect(x => x.SendRequest(stubMessage)).Return(responseMessage);
			var messageFactory = MockRepository.GenerateStub<IBaseMessageFactory>();
			var mockTransferrerFactory = MockRepository.GenerateMock<IHttpExTransferrerFactory>();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer(stubMessage, WSHttpExPropertiesTest.PropertyNamespace, messageFactory)).Return(stubTransferrer);
			var target = new WSHttpExTransmitterEndpoint(transmitter, mockTransferrerFactory);
			var transportProxy = MockRepository.GenerateStub<IBTTransportProxy>();
			transportProxy.Expect(x => x.GetMessageFactory()).Return(messageFactory);

			transmitter.Initialize(transportProxy);
			target.Open(null, null, WSHttpExPropertiesTest.PropertyNamespace);
			var returnValue = target.ProcessMessage(stubMessage);
			Assert.AreEqual(responseMessage, returnValue);

			mockTransferrerFactory.VerifyAllExpectations();
			stubTransferrer.VerifyAllExpectations();
			transportProxy.VerifyAllExpectations();
			messageFactory.VerifyAllExpectations();
		}
	}
}
