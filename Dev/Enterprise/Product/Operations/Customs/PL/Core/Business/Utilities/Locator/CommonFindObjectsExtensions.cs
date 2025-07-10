using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using static CargoWise.EntityFramework.OrderByClause;
using static Enterprise.Messaging.Integration.ReceiveTransmitList.Codes;
using static Enterprise.ZArchitecture.Schema.EDIInterchangeSchema;
using static Enterprise.ZArchitecture.Schema.EDIMessageSchema;

namespace Enterprise.Customs.PL.Business;

public static class CommonFindObjectsExtensions
{
	public static BaseEDIMessage FindMessageByMessageNum(this BusinessObjectFactory factory, ZString applicationCodes, ZString messageNumber, ZString receiveTransmit, ZQuery filter = null)
	{
		if (messageNumber.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = EM_SystemCreateTimeUtc.Name + Descending }
			.AddToFilter_PossiblyCommaSeparated(EM_ApplicationCode, applicationCodes)
			.AddToFilter(EM_ReceiveTransmit, receiveTransmit)
			.AddToFilter(EM_MessageNum, messageNumber);

		if (filter != null)
		{
			query.AddToFilter(filter);
		}

		return factory.LoadTop1<BaseEDIMessage>(query);
	}

	public static bool MessageWithMessageNumExist(this BusinessObjectFactory factory, ZString applicationCodes, ZString messageNumber, ZString receiveTransmit, ZQuery filter = null)
	{
		if (messageNumber.IsEmpty)
		{
			return false;
		}

		var query = new ZQuery()
				.AddToFilter_PossiblyCommaSeparated(EM_ApplicationCode, applicationCodes)
				.AddToFilter(EM_ReceiveTransmit, receiveTransmit)
				.AddToFilter(EM_MessageNum, messageNumber);

		if (filter != null)
		{
			query.AddToFilter(filter);
		}

		return factory.Exists(typeof(BaseEDIMessage), query);
	}

	public static BaseEDIMessage FindSentTransmittedMessageByExternalSystemID(this BusinessObjectFactory factory, ZString applicationCodes, ZString externalSystemID)
	{
		if (externalSystemID.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = EM_SystemCreateTimeUtc.Name + Descending }
			.AddToFilter_PossiblyCommaSeparated(EM_ApplicationCode, applicationCodes)
			.AddToFilter(EM_ReceiveTransmit, Transmit)
			.AddToFilter(EM_ApplicationReference, externalSystemID)
			.AddToFilter(EM_Status, new [] { EDIMessage.Status.Sent, EDIMessage.Status.ProcessedOK });
		return factory.LoadTop1<BaseEDIMessage>(query);
	}

	public static bool MessageByExternalSystemIDExist(this BusinessObjectFactory factory, ZString applicationCodes, ZString externalSystemID, ZString receiveTransmit)
	{
		if (externalSystemID.IsEmpty)
		{
			return false;
		}

		var query = new ZQuery()
			.AddToFilter_PossiblyCommaSeparated(EM_ApplicationCode, applicationCodes)
			.AddToFilter(EM_ApplicationReference, externalSystemID)
			.AddToFilter(EM_ReceiveTransmit, receiveTransmit);
		return factory.Exists(typeof(BaseEDIMessage), query);
	}

	public static BaseEDIMessage FindTransmittedMessageBySessionGuid(this BusinessObjectFactory factory, ZString applicationCodes, ZGuid sessionGuid)
	{
		if (sessionGuid.IsEmpty || factory.FindTransmittedInterchangeBySessionGuid(applicationCodes, sessionGuid) is not { } interchange)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = EM_SystemCreateTimeUtc.Name + Descending }
			.AddToFilter_PossiblyCommaSeparated(EM_ApplicationCode, applicationCodes)
			.AddToFilter(EM_EI, interchange.PK)
			.AddToFilter(EM_ReceiveTransmit, Transmit);
		return factory.LoadTop1<BaseEDIMessage>(query);
	}

	public static BaseEDIMessage FindLastTransmittedMessage(this BusinessObjectFactory factory, ZString applicationCodes, BusinessObject businessObject)
	{
		var query = new ZQuery { OrderBy = EM_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(EM_ApplicationCode, applicationCodes)
			.AddToFilter(EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
			.AddToFilter(EM_Status, new[] { EDIMessage.Status.Sent, EDIMessage.Status.ProcessedOK, EDIMessage.Status.Queued })
			.AddToFilter(EM_LinkUniqueID, businessObject.PK);
		return factory.LoadTop1<BaseEDIMessage>(query);
	}

	public static EDIInterchange FindReceivedInterchangeBySessionGuid(this BusinessObjectFactory factory, ZString applicationCodes, ZGuid sessionGuid)
	{
		if (sessionGuid.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = EI_SystemCreateTimeUtc.Name + Descending }
			.AddToFilter(EI_SessionGUID, sessionGuid)
			.AddToFilter(EI_ReceiveTransmit, Receive)
			.AddToFilter_PossiblyCommaSeparated(EI_ApplicationCode, applicationCodes);
		return factory.LoadTop1<EDIInterchange>(query);
	}

	static EDIInterchange FindTransmittedInterchangeBySessionGuid(this BusinessObjectFactory factory, ZString applicationCodes, ZGuid sessionGuid)
	{
		if (sessionGuid.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = EI_SystemCreateTimeUtc.Name + Descending }
			.AddToFilter(EI_SessionGUID, sessionGuid)
			.AddToFilter(EI_ReceiveTransmit, Transmit)
			.AddToFilter(EI_Status, EDIInterchange.Status.Sent)
			.AddToFilter(EI_IsActive, true)
			.AddToFilter_PossiblyCommaSeparated(EI_ApplicationCode, applicationCodes);
		return factory.LoadTop1<EDIInterchange>(query);
	}
}
