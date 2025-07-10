using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Message;

[TestFixture]
sealed class MessageSenderTest
{
	[Test]
	public void TestSendTariffUpdateRequest()
	{
		puescServiceMock.Setup(_ => _.AcceptDocument(It.IsAny<AcceptDocumentRequest>())).Returns(new AcceptDocumentResponse()
		{
			result = new() { sysRef = "sysRef" }
		});

		var result = messageSender.SendTariffUpdateRequest(new XDocument());
		puescServiceMock.Verify(_ => _.AcceptDocument(It.Is<AcceptDocumentRequest>(a =>
			a.document.content.filename == Constants.Soap.Puesc.TariffUpdateRequestFileName
			&& a.document.targetSystems.First() == systemType.ISZTAR4
			&& a.document.content.mime == mimeType.applicationxml)));
		Assert.AreEqual("sysRef", result);
	}

	[Test]
	public void TestSendTariffUpdateRequest_EmptyResponse()
	{
		var result = messageSender.SendTariffUpdateRequest(new XDocument());
		Assert.AreEqual(null, result);
	}

	[Test]
	public void TestGetTariffUpdate()
	{
		var expectedByteContent = Encoding.UTF8.GetBytes("<Empty></Empty>");
		puescServiceMock.Setup(_ => _.GetDocuments(It.IsAny<GetDocumentsRequest>())).Returns(new GetDocumentsResponse
		{
			document = [
				new()
				{
					content = new()
					{
						Value = expectedByteContent,
						filename = Constants.Soap.Puesc.TariffUpdateResponseFilename
					}
				},
				new()
				{
					content = new()
					{
						Value = Encoding.UTF8.GetBytes("<Other></Other>"),
						filename = "OtherName.xml"
					}
				}
			]
		});

		var result = messageSender.GetTariffUpdate("sysRef");
		puescServiceMock.Verify(_ => _.GetDocuments(It.Is<GetDocumentsRequest>(a =>
			a.korelacjaSysref == "sysRef"
			&& a.targetSystem == systemType.ISZTAR4
			&& a.pobrany == "0")));
		Assert.AreEqual(expectedByteContent, result);
	}

	[Test]
	public void TestGetTariffUpdate_EmptyResponse()
	{
		var result = messageSender.GetTariffUpdate("sysRef");
		Assert.AreEqual(null, result);
	}

	[SetUp]
	public void SetUp()
	{
		puescServiceMock = new Mock<IPuescService>();
		var factory = Mock.Of<IPuescServiceFactory>( x => x.Create() == puescServiceMock.Object);
		messageSender = new MessageSender(factory);
	}

	MessageSender messageSender;
	Mock<IPuescService> puescServiceMock;
}
