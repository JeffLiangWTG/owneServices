using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(InboundMessageCreatorLegacy))]
sealed class InboundMessageCreatorLegacyTest : PLInterchangeUnpackerTest
{
	public void TestTransmitMessageNotFound()
	{
		const string xmlNode = "<Test/>";
		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(xmlNode, ApplicationCode.PLCustoms);
		var unpackStrategyMock = new Mock<IXmlInterchangeUnpackingStrategy>();
		unpackStrategyMock.Setup(s => s.Unpack(
				It.IsAny<EDIInterchange>(),
				It.IsAny<EDIInterchange>(),
				It.IsAny<EnterpriseEDIMessage>(),
				It.IsAny<XmlReader>(),
				It.IsAny<ISimpleLogger>()))
			.Returns(() => new EDIInterchangeUnpackerResult(Array.Empty<EnterpriseEDIMessage>()));
		var unpackStrategy = unpackStrategyMock.Object;
		requestMessage.Interchange.EI_SessionGUID = ZGuid.NewZGuid();
		var (_, unpackingResult) = Unpack(unpackStrategy, responseInterchange, requestMessage.Interchange, requestMessage, new XmlQualifiedName("Test"));
		unpackingResult.Assert(ExpectedUnpackResult.Failure("Unable to find transmitted message!"));
	}

	protected override (Type UnpackStrategyType, IUniversalCustomsInterchangeUnpackerResult UnpackerResult)
		Unpack(
			IXmlInterchangeUnpackingStrategy unpackingStrategy,
			EDIInterchange interchange,
			EDIInterchange outgoingInterchange,
			EnterpriseEDIMessage outgoingMessage,
			XmlQualifiedName content)
	{
		var unpackerMock = new Mock<InboundMessageCreatorLegacy>(ServiceLogger) { CallBase = true };
		unpackerMock.Protected()
			.Setup<IXmlInterchangeUnpackingStrategy>("GetUnpackingStrategy", ItExpr.IsAny<XmlQualifiedName>())
			.Returns(unpackingStrategy);
		var unpacker = unpackerMock.Object;
		unpacker.CreateMessagesForInterchange(interchange);
		if (interchange.EI_Status == EDIInterchange.Status.Error)
		{
			var errorDescription = interchange.Logs.Find(x => x.Event.SE_Code == Events.ErrorReport.Code).First();
			return (UnpackStrategyType: null, new EDIInterchangeUnpackerResult(errorDescription.Reference));
		}
		var createdMessages = interchange.ContainedMessages.Cast<EnterpriseEDIMessage>().ToList();
		var actualStrategy = typeof(InboundMessageCreatorLegacy)
			.GetMethod("GetUnpackingStrategy", BindingFlags.NonPublic | BindingFlags.Instance)!
			.Invoke(new InboundMessageCreatorLegacy(ServiceLogger), [content]);
		var unpackResult = new EDIInterchangeUnpackerResult(createdMessages);
		return (actualStrategy?.GetType(), unpackResult);
	}
}
