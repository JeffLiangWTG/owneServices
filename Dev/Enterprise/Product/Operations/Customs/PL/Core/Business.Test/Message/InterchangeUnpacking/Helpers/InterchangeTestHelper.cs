using System.Data;
using System.Reflection;
using System.Threading;
using System.Xml;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.Business.Testing;

public static class InterchangeTestHelper
{
	public static string GetTestFile(string testFileName) => Assembly.GetCallingAssembly().GetTestFile(testFileName);

	public static MessageExchange PrepareRequestWithResponse(
		this BusinessObjectFactory factory,
		string responseText,
		string applicationCode = null,
		string interchangeType = null,
		CusPollingTransaction cusPollingTransaction = null,
		string requestText = null)
	{
		var requestInterchangeSessionGuid = ZGuid.NewZGuid();
		var (requestInterchange, requestMessage) = factory.CreateInterchangeAndMessageForTest<EDIInterchange, EDIMessageForTest>(
			applicationCode: applicationCode ?? string.Empty,
			sessionGuid: requestInterchangeSessionGuid,
			receiveTransmit: EDIInterchange.Direction.Transmit,
			messageApplicationReference: "TST",
			interchangeStatus: EDIMessageStatusList.Codes.Sent,
			messageStatus: EDIMessageStatusList.Codes.Sent);

		if (cusPollingTransaction != null)
		{
			requestMessage.EM_LinkedObject = cusPollingTransaction;
		}
		if (!string.IsNullOrEmpty(requestText))
		{
			requestMessage.EM_MessageText = requestText;
		}

		var receiveInterchange = factory.CreateInterchangeForTest<EDIInterchange>(
			sessionGuid: requestInterchange.EI_SessionGUID,
			applicationCode: applicationCode,
			receiveTransmit: EDIInterchange.Direction.Receive,
			interchangeStatus: EDIInterchange.Status.Queued,
			interchangeType: interchangeType,
			messageBody: responseText);

		return new(requestMessage, receiveInterchange);
	}

	public static XmlReader GetBodyXmlReader(this EDIInterchange interchange)
	{
		XmlReader xmlReader = null;
		try
		{
			xmlReader = XmlHelper.CreateReaderAndGotoRootNode(interchange.GetEI_BodyTextReader(), closeInput: true);
			if (xmlReader.IsSoap())
			{
				xmlReader.MoveToSoapBody();
			}
			return xmlReader;
		}
		catch
		{
			xmlReader?.Dispose();
			throw;
		}
	}

	sealed class EDIMessageForTest(BusinessObjectFactory factory, DataRow row) : EnterpriseEDIMessage(factory, row)
	{
		static int messageReferenceNumber;

		protected override string GetMessageReferenceNumber()
			=> Interlocked.Increment(ref messageReferenceNumber).ToString();
	}
}
