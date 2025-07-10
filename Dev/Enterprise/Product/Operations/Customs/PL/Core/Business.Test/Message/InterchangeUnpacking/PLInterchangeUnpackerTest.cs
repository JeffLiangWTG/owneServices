using System;
using System.Reflection;
using System.Xml;
using CargoWise.Application;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(PLInterchangeUnpacker))]
public class PLInterchangeUnpackerTest : InterchangeUnpackerTest<PLInterchangeUnpacker>
{
	public override void TestCorrectSubscribeToUCUSubscribers() => CombineAssertions(() =>
	{
		foreach (var applicationCode in ApplicationCodes)
		{
			AssertType<PLInterchangeUnpacker>(ObjectFactory.Get<IUCUSubscribersProvider>().GetInterchangeUnpacker(applicationCode));
		}
	});

	public void TestUnpackUniversalEvent()
		=> TestSuccessfulUnpack(new XmlQualifiedName("UniversalEvent", "http://www.cargowise.com/Schemas/Universal/2012/11"),
			typeof(UniversalEventUnpackingStrategy));

	public void TestUnpackSoapFault1_1()
		=> TestSuccessfulUnpack(new XmlQualifiedName("Fault", "http://schemas.xmlsoap.org/soap/envelope/"),
			typeof(FaultInterchangeUnpackingStrategy));

	public void TestUnpackSoapFault1_2()
		=> TestSuccessfulUnpack(new XmlQualifiedName("Fault", "http://www.w3.org/2003/05/soap-envelope"),
			typeof(FaultInterchangeUnpackingStrategy));

	public void TestUnpackSoapBusinessErrorFault()
		=> TestSuccessfulUnpack(new XmlQualifiedName("businessErrorFault", "http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0"),
			typeof(FaultInterchangeUnpackingStrategy));

	public void TestUnpackSoapTechErrorFault()
		=> TestSuccessfulUnpack(new XmlQualifiedName("techErrorFault", "http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0"),
			typeof(FaultInterchangeUnpackingStrategy));

	public void TestUnpackAcceptDocumentResponse()
		=> TestSuccessfulUnpack(new XmlQualifiedName("AcceptDocumentResponse", "http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0"),
			typeof(AcceptDocumentResponseUnpackingStrategy));

	public void TestUnpackGetDocumentsResponse()
		=> TestSuccessfulUnpack(new XmlQualifiedName("GetDocumentsResponse", "http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0"),
			typeof(GetDocumentsResponseUnpackingStrategy));

	public void TestUnsupportedContent()
	{
		const string xmlNode = "<Test/>";
		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(xmlNode, ApplicationCode.PLCustoms);
		var (_, unpackResult) = Unpack(unpackingStrategy: null, responseInterchange, requestMessage.Interchange, requestMessage, new XmlQualifiedName("Test"));
		unpackResult.Assert(ExpectedUnpackResult.Failure("Content is not recognized!"));
	}

	public void TestUnpackingException()
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
			.Throws(() => new InvalidOperationException("TestException"));
		var unpackStrategy = unpackStrategyMock.Object;
		var (_, unpackResult) = Unpack(unpackStrategy, responseInterchange, requestMessage.Interchange, requestMessage, new XmlQualifiedName("Test"));
		unpackResult.Assert(ExpectedUnpackResult.Failure("TestException"));
	}

	void TestSuccessfulUnpack(XmlQualifiedName content, Type expectedUnpackStrategy) => CombineAssertions(() =>
	{
		foreach (var applicationCode in ApplicationCodes)
		{
			TestSuccessfulUnpack(applicationCode, content, expectedUnpackStrategy);
		}
	});

	void TestSuccessfulUnpack(string applicationCode, XmlQualifiedName content, Type expectedUnpackStrategy)
	{
		const string testExpectedMessageNum = "TestNum";
		var xmlNode = $"""<{content.Name} xmlns="{content.Namespace}"/>""";

		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(xmlNode, applicationCode);
		var expectedMessage = new ExpectedMessage { Num = testExpectedMessageNum };

		var unpackStrategyMock = new Mock<IXmlInterchangeUnpackingStrategy>();
		unpackStrategyMock.Setup(s => s.Unpack(
				It.IsAny<EDIInterchange>(),
				It.IsAny<EDIInterchange>(),
				It.IsAny<EnterpriseEDIMessage>(),
				It.IsAny<XmlReader>(),
				It.IsAny<ISimpleLogger>()))
			.Returns(() => new EDIInterchangeUnpackerResult([UnpackMessage()]));
		var unpackStrategy = unpackStrategyMock.Object;
		var (unpackStrategyType, unpackResult) = Unpack(unpackStrategy, responseInterchange, requestMessage.Interchange, requestMessage, content);
		AssertEquals("Original UnpackStrategyType", expectedUnpackStrategy, unpackStrategyType);
		unpackResult.Assert(ExpectedUnpackResult.Success([expectedMessage]));
		responseInterchange.AssertHasExactLogMessage("Log message", Events.InterchangeInProgress, $"Found {content.Name}");
		return;

		EnterpriseEDIMessage UnpackMessage()
		{
			var result = responseInterchange.ContainedMessages.AddNew();
			result.EM_MessageNum = testExpectedMessageNum;
			return result;
		}
	}

	protected virtual (Type UnpackStrategyType, IUniversalCustomsInterchangeUnpackerResult UnpackerResult)
		Unpack(
			IXmlInterchangeUnpackingStrategy unpackingStrategy,
			EDIInterchange interchange,
			EDIInterchange outgoingInterchange,
			EnterpriseEDIMessage outgoingMessage,
			XmlQualifiedName content)
	{
		var unpackerMock = new Mock<PLInterchangeUnpacker> { CallBase = true };
		unpackerMock.Protected()
			.Setup<IXmlInterchangeUnpackingStrategy>("GetUnpackingStrategy", ItExpr.IsAny<XmlQualifiedName>())
			.Returns(unpackingStrategy);
		var unpacker = unpackerMock.Object;
		var unpackResult = unpacker.Unpack(interchange, outgoingInterchange, outgoingMessage, ServiceLogger);
		var actualStrategy = typeof(PLInterchangeUnpacker)
			.GetMethod("GetUnpackingStrategy", BindingFlags.NonPublic | BindingFlags.Instance)!
			.Invoke(new PLInterchangeUnpacker(), [content]);
		return (actualStrategy?.GetType(), unpackResult);
	}

	protected override string[] ApplicationCodes => [
		ApplicationCode.PLCustoms,
		ApplicationCode.PLCustomsNCTS,
		ApplicationCode.PLCustomsExitControl,
	];

	protected override void SetUp()
	{
		base.SetUp();
		ServiceLogger = new LoggingInformation();
	}

	protected LoggingInformation ServiceLogger { get; private set; }
}
