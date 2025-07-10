using System;
using System.Globalization;
using System.ServiceModel;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor;
using CargoWise.RefDbRepo.USReferenceData.CmdLine;
using CargoWise.RefDbRepo.USReferenceData.Services;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.USReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class USIncomingMessageQueryTest
	{
		[Test]
		public void TestCreateNewAdapter()
		{
			using (var adapter = USIncomingMessageProgram.CreateNewAdapter())
			{
				Assert.That(adapter, Is.Not.Null);
			}
		}

		[Test]
		public void TestGenerateMessageContent()
		{
			var dateTime = DateTime.Parse("2022-11-30 17:30", CultureInfo.InvariantCulture);
			string expected =
"<USCustoms><Header><![CDATA[A3910SV9      113022     TS]]></Header>" +
"<Body><![CDATA[B  3910SV9TS                                               USCREPO_202211301730 " +
"F101                                                                            " +
"Y  3910SV9TS                                                                    ]]></Body>" +
"<Footer><![CDATA[Z3910SV9      113022]]></Footer></USCustoms>";
			var message = USIncomingMessageQuery.GenerateMessageContent(dateTime, "TS", "SV9", "3910", "F101");
			Assert.That(message, Is.EqualTo(expected));

			expected =
"<USCustoms><Header><![CDATA[A3910SV9      113022     TS]]></Header>" +
"<Body><![CDATA[B  3910SV9TS                                               USCREPO_202211301730 " +
"F104                                                                            " +
"Y  3910SV9TS                                                                    ]]></Body>" +
"<Footer><![CDATA[Z3910SV9      113022]]></Footer></USCustoms>";
			message = USIncomingMessageQuery.GenerateMessageContent(dateTime, "TS", "SV9", "3910", "F104");
			Assert.That(message, Is.EqualTo(expected));

			expected =
"<USCustoms><Header><![CDATA[A3910SV9      113022     TS]]></Header>" +
"<Body><![CDATA[B  3910SV9TS                                               USCREPO_202211301730 " +
"F111                                         010100                             " +
"Y  3910SV9TS                                                                    ]]></Body>" +
"<Footer><![CDATA[Z3910SV9      113022]]></Footer></USCustoms>";
			new LocalFileStorage(FirmsCode.LastUpdateDate).ClearData();
			message = USIncomingMessageQuery.GenerateMessageContent(dateTime, "TS", "SV9", "3910", "F111");
			Assert.That(message, Is.EqualTo(expected));
		}

		[Test]
		public void TestFillInQueryMessage()
		{
			IeHubMessage mockMessage = null;
			var adapterMock = new Mock<IeHubAdapter>();
			var outboxMock = new Mock<IMessageOutbox>();
			adapterMock.Setup(x => x.Outbox).Returns(outboxMock.Object);
			adapterMock.Setup(x => x.SendMessages()).Callback(() => { });
			outboxMock.Setup(x => x.AddMessage(It.IsAny<IeHubMessage>())).Callback((IeHubMessage x) => { mockMessage = x; });

			var adapterMockOB = adapterMock.Object;

			USIncomingMessageQuery.SendMessages(adapterMockOB, "F101");

			outboxMock.Verify(x => x.AddMessage(It.IsAny<IeHubMessage>()), Times.Exactly(1));
			adapterMock.Verify(x => x.SendMessages(), Times.Exactly(1));

			Assert.That(mockMessage.ApplicationCode, Is.EqualTo(ApplicationConfig.Instance.USIncomingMessageQueryMessageApplicationCode));
			Assert.That(mockMessage.RecipientID, Is.EqualTo(ApplicationConfig.Instance.USIncomingMessageQueryMessageRecipientID));
			Assert.That(mockMessage.SenderID, Is.EqualTo(ApplicationConfig.Instance.USIncomingMessageEHubClientID));
			Assert.That(mockMessage.SchemaName, Is.EqualTo(ApplicationConfig.Instance.USIncomingMessageQueryMessageType));
		}

		[Test]
		public void TestSendMessagesWithEndpointNotFoundException()
		{
			var adapterMock = new Mock<IeHubAdapter>();
			var outboxMock = new Mock<IMessageOutbox>();
			adapterMock.Setup(x => x.Outbox).Returns(outboxMock.Object);
			adapterMock.Setup(x => x.SendMessages()).Throws(new EndpointNotFoundException(""));
			var adapterMockOB = adapterMock.Object;

			Assert.DoesNotThrow(() => USIncomingMessageQuery.SendMessages(adapterMockOB, "F101"));
		}

		[SetUp]
		public void Setup()
		{
			var checker = new LocalFileStorage(FirmsCode.LastUpdateDate);
			checker.ClearData();
		}
	}
}
