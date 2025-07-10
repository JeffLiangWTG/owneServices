using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.PL;
using static Enterprise.Messaging.Business.EDIInterchange.Direction;

namespace Enterprise.Customs.PL.Business;

[Immutable]
public abstract class LocatorBase : IMessageLocator
{
	public abstract ZString ApplicationCodes { get; }

	public BusinessObject FindBusinessObjectForIncomingMessage(BusinessObjectFactory factory, IIncomingMessage incomingMessage) => incomingMessage switch
	{
		IJobIdentification jobIdentification
			when FindJobByIdentification(factory, jobIdentification) is { } job
			=> job,

		ICorrelationProvider correlationProvider
			when factory.FindMessageByMessageNum(ApplicationCodes, messageNumber: correlationProvider.CorrelationIdentifier, Transmit) is { } correlatedMessage
			=> correlatedMessage.EM_LinkedObject,

		IExternalSystemIdReference externalSystemIdReference
			when factory.FindSentTransmittedMessageByExternalSystemID(ApplicationCodes, externalSystemIdReference.ReferenceToExternalSystemID) is { } seapDocumentMessage
			=> seapDocumentMessage.EM_LinkedObject,

		_ => null,
	};

	IEDIMessage IMessageLocator.FindTransmitMessage(BusinessObjectFactory factory, object dataProvider)
		=> FindTransmitMessage(factory, (IIncomingMessage)dataProvider);

	public BaseEDIMessage FindTransmitMessage(BusinessObjectFactory factory, IIncomingMessage dataProvider) => dataProvider switch
	{
		ICorrelationProvider correlationProvider
			when factory.FindMessageByMessageNum(ApplicationCodes, messageNumber: correlationProvider.CorrelationIdentifier, Transmit) is { } correlatedMessage
			=> correlatedMessage,

		IExternalSystemIdReference externalSystemIdReference
			when factory.FindSentTransmittedMessageByExternalSystemID(ApplicationCodes, externalSystemIdReference.ReferenceToExternalSystemID) is { } seapDocumentMessage
			=> seapDocumentMessage,

		IJobIdentification jobIdentification
			when (FindTransmittedMessageByJobIdentification(factory, jobIdentification)) is { } jobMessage
			=> jobMessage,

		_ => null,
	};

	BaseEDIMessage FindTransmittedMessageByJobIdentification(BusinessObjectFactory factory, IJobIdentification jobIdentification)
	{
		if (FindJobByIdentification(factory, jobIdentification) is not { } job)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter_PossiblyCommaSeparated(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes)
			.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Transmit)
			.AddToFilter(EDIMessageSchema.EM_Status, new [] { EDIMessage.Status.Sent, EDIMessage.Status.ProcessedOK })
			.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, job.PK);
		return factory.LoadTop1<BaseEDIMessage>(query);
	}

	BusinessObject FindJobByIdentification(BusinessObjectFactory factory, IJobIdentification jobIdentification) =>
		FindBusinessObjectByMRN(factory, jobIdentification.MRN) ?? FindBusinessObjectByLRN(factory, jobIdentification.LRN);

	protected virtual BusinessObject FindBusinessObjectByMRN(BusinessObjectFactory factory, ZString mrn)
	{
		if (mrn.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber)
			.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Poland)
			.AddToFilter(CusEntryNumSchema.CE_EntryNum, mrn);
		var mrnCusEntryNumber = factory.LoadTop1<CusEntryNumber>(query);
		return mrnCusEntryNumber?.Parent;
	}

	protected abstract BusinessObject FindBusinessObjectByLRN(BusinessObjectFactory factory, ZString lrn);
}
