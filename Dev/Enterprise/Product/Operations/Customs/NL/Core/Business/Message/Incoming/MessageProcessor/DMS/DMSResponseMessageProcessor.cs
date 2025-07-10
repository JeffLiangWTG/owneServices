using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.Business;

public abstract class DMSResponseMessageProcessor : NLBranchCustomsApplicationTypeMessageProcessor<IIncomingDataProvider>
{
	public DMSResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IIncomingDataProvider GetMessageDataProvider(EDIMessage message)
	{
		return _dataProvider ??= message.EM_MessageSubType == NLIncomingMessageSubTypeList.Codes.Control
								? DMSResponseMessageHelper.CreateControlIncomingDataProvider(message.EM_MessageText)
								: DMSResponseMessageHelper.CreateDMSIncomingDataProvider(message.EM_MessageText);
	}
	IIncomingDataProvider _dataProvider;

	protected override string MessageFriendlyNameCore => Res.GetString("2BE29C2A-DFD3-4594-93D3-2EF09684EFB0", "DMS Response");

	protected sealed override ZBool LinkMessageToParentJob(EDIMessage message)
	{
		var result = false;
		var header = FindParentOfMessage(message) as CusEntryHeader;
		if (header != null)
		{
			message.EM_LinkedObject = header;
			result = true;
			var branchPk = GetBranchPkFromJobBO(header);
			if (branchPk.IsValid)
			{
				message.EM_GB = branchPk;
			}
			Logger.Log(Res.GetString("CD01F510-AC8A-44D5-B9EB-60A33DC31A9F", "Message linked to entry header"));
		}
		else
		{
			var (lrn, mrn) = GetLrnMrnFromMessage(message);
			string messageNotFoundLog;
			if (!string.IsNullOrEmpty(lrn) && !string.IsNullOrEmpty(mrn))
			{
				messageNotFoundLog = Res.GetString("0F6C3A9A-48AA-4682-8B00-D92523892158", "Entry header not found with reference LRN '{0}' or MRN '{1}'.", lrn, mrn);
			}
			else if (!string.IsNullOrEmpty(lrn))
			{
				messageNotFoundLog = Res.GetString("FD3BB229-2DD8-4ED4-B36D-410E3860F9F7", "Entry header not found with reference LRN '{0}'.", lrn);
			}
			else if (!string.IsNullOrEmpty(mrn))
			{
				messageNotFoundLog = Res.GetString("3D529A67-CC87-4FE6-A4ED-B7F9786ACF76", "Entry header not found with reference MRN '{0}'.", mrn);
			}
			else
			{
				messageNotFoundLog = Res.GetString("AEE9FFEE-6A77-4EC8-BE60-028CBCB7B963", "Entry header not found. LRN and MRN not found in message.");
			}
			Logger.Log(messageNotFoundLog);
			message.Notes.AddNew(false, NLConstants.Notes.Descriptions.DataImportLogText, messageNotFoundLog);
		}
		return result;
	}

	protected sealed override ZString InterpretMessage(EDIMessage message)
	{
		var dataProvider = GetMessageDataProvider(message);
		if (dataProvider is IDMSIncomingDataProvider dmsDataProvider)
		{
			return InterpretMessage(message, dmsDataProvider);
		}
		else if (dataProvider is IControlIncomingDataProvider controlDataProvider)
		{
			return InterpretMessage(message, controlDataProvider);
		}
		return ZString.Empty;
	}

	protected virtual ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => ZString.Empty;

	protected virtual ZString InterpretMessage(EDIMessage message, IControlIncomingDataProvider dataProvider) => ZString.Empty;

	protected sealed override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => DMSResponseMessageHelper.GetBranchPkFromJobBO(linkedObject);

	protected sealed override void ProcessMessage(NLEDIMessage message)
	{
		var successful = false;
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			var dataProvider = GetMessageDataProvider(message);

			if (dataProvider is IDMSIncomingDataProvider dmsDataProvider)
			{
				SetEntryInfos(entryHeader, dmsDataProvider);
			}
			else if (dataProvider is IControlIncomingDataProvider controlDataProvider)
			{
				SetEntryInfos(entryHeader, controlDataProvider);
			}
			successful = true;
		}
		message.EM_Status = successful ? EDIMessage.Status.ProcessedOK : EDIMessage.Status.Failed;
	}

	protected virtual void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
	}

	protected virtual void SetEntryInfos(CusEntryHeader entryHeader, IControlIncomingDataProvider dataProvider)
	{
	}

	protected void SetEntryStatuses(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider, string status = null, string phaseStatus = null)
	{
		var entryStatus = DMSResponseMessageHelper.GetMessageEntryStatus(entryHeader, dataProvider);
		SetEntryStatuses(entryHeader, entryStatus, status, phaseStatus);
	}

	protected void SetEntryStatuses(CusEntryHeader entryHeader, string entryStatus = null, string status = null, string phaseStatus = null)
	{
		if (!string.IsNullOrEmpty(entryStatus))
		{
			entryHeader.CH_EntryStatus = entryStatus;
		}
		if (!string.IsNullOrEmpty(status))
		{
			entryHeader.CH_Status = status;
		}
		if (!string.IsNullOrEmpty(phaseStatus))
		{
			entryHeader.CH_PhaseStatus = phaseStatus;
		}
	}

	protected void SetEntryNumberExpiryDate(CusEntryNumber mrnEntryNumber, DateTime? expirationDate)
	{
		SetDateTimeValue(x => mrnEntryNumber.CE_ExpiryDate = x, expirationDate);
	}

	protected void SetEntryHeaderSubmittedDate(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		SetDateTimeValue(x => entryHeader.CH_EntrySubmittedDate = x, dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);
	}

	protected void SetEntryNumberIssueDateAndNum(CusEntryNumber mrnEntryNumber, IDMSIncomingDataProvider dataProvider)
	{
		var issueDate = dataProvider.Declaration?.AcceptanceDate is DateTime acceptanceDate ? acceptanceDate : dataProvider.Declaration?.RejectionDateTime?.Date;
		SetDateTimeValue(x => mrnEntryNumber.CE_IssueDate = x, issueDate);

		mrnEntryNumber.CE_EntryNum = dataProvider.Declaration?.Id;
	}

	protected void SetEntryHeaderReleaseDate(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		SetDateTimeValue(x => entryHeader.CH_EntryReleaseDate = x, dataProvider.Statuses.FirstOrDefault()?.ReleaseDate);
	}

	protected CusEntryNumber GetMRNCusEntryNumber(CusEntryHeader entryHeader) => CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);

	protected sealed override BusinessObject FindParentOfMessage(EDIMessage message)
	{
		var (lrn, mrn) = GetLrnMrnFromMessage(message);
		var sessionID = message.Interchange?.EI_SessionGUID;
		BusinessObject entryHeader = null;

		if (!string.IsNullOrEmpty(lrn))
		{
			entryHeader = FindParentOfMessageBasedOnLRN(message.Factory, lrn);
		}
		if (entryHeader == null && !string.IsNullOrEmpty(mrn))
		{
			entryHeader = FindParentOfMessageBasedOnMRN(message.Factory, mrn);
		}
		if (entryHeader == null && sessionID is ZGuid sessionIDGuid && sessionIDGuid.IsValid)
		{
			entryHeader = FindParentOfMessageBasedOnSessionID(message.Factory, sessionIDGuid);
		}

		return entryHeader;
	}

	(string lrn, string mrn) GetLrnMrnFromMessage(EDIMessage message)
	{
		var dataProvider = GetMessageDataProvider(message);
		var lrn = string.Empty;
		var mrn = string.Empty;
		if (dataProvider is IDMSIncomingDataProvider dmsDataProvider)
		{
			lrn = dmsDataProvider.Declaration?.FunctionalReference;
			mrn = dmsDataProvider.Declaration?.Id;
		}
		else if (dataProvider is IControlIncomingDataProvider controlDataProvider)
		{
			lrn = controlDataProvider.Response.FunctionalReferenceId;
		}
		return (lrn, mrn);
	}

	BusinessObject FindParentOfMessageBasedOnLRN(BusinessObjectFactory factory, string lrn)
	{
		BusinessObject entryHeader = null;

		if (!string.IsNullOrEmpty(lrn))
		{
			var entryQuery = new ZQuery();
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, lrn);
			entryHeader = factory.LoadTop1<CusEntryHeader>(new ZQuery(entryQuery));
		}

		return entryHeader;
	}

	BusinessObject FindParentOfMessageBasedOnMRN(BusinessObjectFactory factory, string mrn)
	{
		BusinessObject entryHeader = null;

		if (!string.IsNullOrEmpty(mrn))
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_EntryNum, mrn)
				 .AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber)
				 .AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
			var cusEntryNum = factory.LoadTop1<CusEntryNumber>(new ZQuery(query));
			entryHeader = cusEntryNum?.Parent;
		}

		return entryHeader;
	}

	BusinessObject FindParentOfMessageBasedOnSessionID(BusinessObjectFactory factory, ZGuid sessionID)
	{
		BusinessObject entryHeader = null;

		if (sessionID.IsValid)
		{
			var interchangeSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Sent)
							   .AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
							   .AddToFilter(EDIInterchangeSchema.EI_IsActive, true)
							   .AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.NLCustoms)
							   .AddToFilter(EDIInterchangeSchema.EI_SessionGUID, sessionID)
							   .OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeSubQuery, JoinCondition.And);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
						.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent)
						.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;

			var originalOutgoingMessage = factory.LoadTop1<EDIMessage>(messageQuery);
			entryHeader = originalOutgoingMessage?.EM_LinkedObject;
		}

		return entryHeader;
	}

	static void SetDateTimeValue(Action<ZDateTime> action, DateTime? dateTime)
	{
		if (dateTime != null)
		{
			action(new ZDateTime(dateTime.Value));
		}
		else
		{
			action(ZDateTime.Empty);
		}
	}
}
