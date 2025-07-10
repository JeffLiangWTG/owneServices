using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Integration;
using CargoWise.xTMessaging.Shared;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using Xware.Xt.Grpc.Application;
using Constants = CargoWise.RefDbRepo.ILReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Services
{
	[TestFixture]
	sealed class DirectXtConnectorProxyTest
	{
		[Test]
		public void TestSending_SendingValidationXtMessageInfo()
		{
			var msgClientProvider = new BasicMsgClientProvider(
				connect: "MsgClientConnect",
				caBundle: "MsgClientCaBundle",
				uri: "MsgClientURI",
				password: "MsgClientPassword",
				timeout: TimeSpan.FromSeconds(1),
				token: new CancellationToken());

			using (var connector = new DirectXtConnectorProxy(
				msgClientProvider: msgClientProvider,
				xTMessagingConfig: new DirectXtMessagingConfig(),
				logger: null,
				msgAttributeModifier: null,
				receiveHandler: null
				))
			{
				Assert.CatchAsync<ArgumentException>(() => connector.SendAsync(null), "ArgumentException raise when xtMessageInfo is null");
			}
		}

		[Test]
		public async Task TestSending_SendingFailure()
		{
			var logger = new ILReferenceData.Services.Logger();
			var systemTableRequest_2012Xml = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableRequest_2012SoapEnvelope.xml");

			var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
			callRequestStream.Setup(m => m.CompleteAsync()).Returns(Task.CompletedTask);
			callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>())).Returns((ByteChunk _) => Task.CompletedTask);
			var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });
			using (var asyncCall = new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(callRequestStream.Object, callResponseAsync, null, null, null, () => { }))
			{
				var msgClientMock = new Mock<IMsgClient>();
				msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.Returns((SubmitMsgMessage msg, DateTime? dateTime, CancellationToken? cancellationToken) =>
					{
						var submitMsgReply = new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrInternal };
						return Task.FromResult(submitMsgReply);
					});

				msgClientMock.Setup(m => m.StartTransactionAsync(It.IsAny<StartTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.ReturnsAsync(new StartTransactionReply());

				msgClientMock.Setup(m => m.EndTransactionAsync(It.IsAny<EndTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.ReturnsAsync(new EndTransactionReply());

				msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>())).Returns(asyncCall);

				var clientProvider = new Mock<IMsgClientProvider>();
				clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);

				using (var connector = new DirectXtConnectorProxy(clientProvider.Object, new DirectXtMessagingConfig(), logger, null, null))
				{
					var (sendResult, msgId, errMessage) = await connector.SendAsync(new XtMessageInfo(systemTableRequest_2012Xml, Constants.MessageSubType.CustomCodes));
					Assert.AreEqual(false, sendResult, "Sending failed when not ErrorCode.OK returned");
					Assert.AreEqual(0, msgId, "MsgId is 0 when not ErrorCode.OK returned");
					Assert.AreEqual("Sending Direct Xt Message 901 failed:Rejected by xT Server. Error 'ErrInternal - Internal unexpected error (6)' returned.", errMessage, "The error message");
				}
			}
		}

		[Test]
		public async Task TestSending_SendOk()
		{
			var logger = new ILReferenceData.Services.Logger();
			var systemTableRequest_2012Xml = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableRequest_2012SoapEnvelope.xml");

			var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
			callRequestStream.Setup(m => m.CompleteAsync()).Returns(Task.CompletedTask);
			callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>())).Returns((ByteChunk _) => Task.CompletedTask);
			var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });
			using (var asyncCall = new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(callRequestStream.Object, callResponseAsync, null, null, null, () => { }))
			{
				var msgClientMock = new Mock<IMsgClient>();
				msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(),
						It.IsAny<CancellationToken?>()))
					.Returns((SubmitMsgMessage msg, DateTime? dateTime, CancellationToken? cancellationToken) =>
					{
						var submitMsgReply = new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk };
						submitMsgReply.Ids.Add(new MsgIdUri { Msgid = 12345UL });
						return Task.FromResult(submitMsgReply);
					});

				msgClientMock.Setup(m => m.StartTransactionAsync(It.IsAny<StartTransactionMessage>(),
						It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.ReturnsAsync(new StartTransactionReply());

				msgClientMock.Setup(m => m.EndTransactionAsync(It.IsAny<EndTransactionMessage>(), It.IsAny<DateTime?>(),
						It.IsAny<CancellationToken?>()))
					.ReturnsAsync(new EndTransactionReply());

				msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.Returns(asyncCall);

				var clientProvider = new Mock<IMsgClientProvider>();
				clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
				using (var connector = new DirectXtConnectorProxy(clientProvider.Object, new DirectXtMessagingConfig(), logger, null, null))
				{
					var (sendResult, msgId, errMessage) = await connector.SendAsync(new XtMessageInfo(systemTableRequest_2012Xml, Constants.MessageSubType.CustomCodes));

					Assert.AreEqual(true, sendResult, "Sending failed when not ErrorCode.OK returned");
					Assert.AreEqual("A message of type 901, with XtReference", errMessage.Substring(0, 39), "The error message");
				}
			}
		}
	}
}
