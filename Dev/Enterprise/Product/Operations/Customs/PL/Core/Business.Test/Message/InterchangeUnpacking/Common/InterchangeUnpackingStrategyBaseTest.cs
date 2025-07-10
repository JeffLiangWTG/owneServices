using System.Collections.Generic;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class InterchangeUnpackingStrategyBaseTest : TestCaseWithFactory
{
	public void TestSupportedXmlNodes()
		=> AssertContainsExactElementsInAnyOrder(ExpectedSupportedXmlNodes, UnpackingStrategy.SupportedXmlNodes);

	public void TestTransmitMessageNotFound()
	{
		var receiveInterchange = Factory.CreateInterchangeForTest<EDIInterchange>(
			applicationCode: ApplicationCode.PLCustoms,
			receiveTransmit: EDIInterchange.Direction.Receive,
			interchangeStatus: EDIInterchange.Status.Queued,
			messageBody: "<Test/>");
		using var responseBodyXmlReader = receiveInterchange.GetBodyXmlReader();
		var unpackResult = UnpackingStrategy.Unpack(receiveInterchange, null, null, responseBodyXmlReader, ServiceLog);
		unpackResult.Assert(
			ExpectedUnpackResult.Failure("Interchange processing failed because related transmit message couldn't be located."),
			logExpectedOnByDefault: new(receiveInterchange, ServiceLog));
	}

	protected abstract IReadOnlyCollection<XmlQualifiedName> ExpectedSupportedXmlNodes { get; }

	protected abstract IXmlInterchangeUnpackingStrategy CreateUnpackingStrategy();

	protected override void SetUp()
	{
		ServiceLog = new LoggingInformation();
		UnpackingStrategy = CreateUnpackingStrategy();

		base.SetUp();
	}

	protected LoggingInformation ServiceLog { get; private set; }
	protected IXmlInterchangeUnpackingStrategy UnpackingStrategy { get; private set; }
}
