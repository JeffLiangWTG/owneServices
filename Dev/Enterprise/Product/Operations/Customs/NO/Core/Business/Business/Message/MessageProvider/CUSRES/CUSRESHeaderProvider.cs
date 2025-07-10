using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Edifact.V902.Messages.CUSRES;
using Enterprise.Edifact.V902.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

sealed class CUSRESHeaderProvider : NonPersistentBusinessObject, ICUSRESHeader
{
	internal CUSRESHeaderProvider(BusinessObjectFactory factory, CUSRESMessage message) : base(Argument.NotNull(factory, nameof(factory)))
	{
		cusresMessage = Argument.NotNull(message, nameof(message));
	}

	readonly CUSRESMessage cusresMessage;

	public static CUSRESHeaderProvider New(EDIMessage message)
	{
		if (message is null)
		{
			return null;
		}

		var segmentGroup = EdiMessageFactory.GetMessage(message);

		return EdiMessageFactory.GetMessage(message) is CUSRESMessage responseMessage
			? new (message.Factory, responseMessage)
			: null;
	}

	ZString ICUSRESHeader.DeclarationID => declarationID ??= GetDeclarationID();
	ZString? declarationID;

	ZString GetDeclarationID()
	{
		return cusresMessage.UNH[0].CommonAccessReference ?? ZString.Empty;
	}

	ZString ICUSRESHeader.MessageType => GetMessageType();

	ZString GetMessageType()
	{
		return (cusresMessage.BGM.Count != 0) ? cusresMessage.BGM[0].DocumentMessageName.DocumentMessageNameCoded.ToString() : ZString.Empty;
	}

	ZString ICUSRESHeader.MessageFunction => GetMessageFunction();

	ZString GetMessageFunction()
	{
		return (cusresMessage.BGM.Count != 0) ? cusresMessage.BGM[0].MessageFunctionCoded.ToString() : ZString.Empty;
	}

	ImmutableHashSet<ZString> ICUSRESHeader.MessageTextCodes => messageTextCodes ??= GetMessageTextCodes();
	ImmutableHashSet<ZString> messageTextCodes;

	ImmutableHashSet<ZString> GetMessageTextCodes()
	{
		return Enumerable.Range(0, cusresMessage.FTX.Count)
			.Select(index => cusresMessage.FTX[index])
			.Select(f => f.TextLiteral.FreeText1)
			.Where(f => !string.IsNullOrWhiteSpace(f))
			.Select(f => new ZString(f))
			.Distinct()
			.ToImmutableHashSet();
	}

	ZString ICUSRESHeader.RequestedControlAction => requestedControlAction ??= GetRequestedControlAction();
	ZString? requestedControlAction;

	ZString GetRequestedControlAction()
	{
		foreach (GISSegment gis in cusresMessage.GIS)
		{
			var processingIndicator = gis.ProcessingIndicator;
			if (processingIndicator.ProcessingIndicatorCoded.In(
				ProcessingIndicatorCodedList.GoodsRequiredForExamination,
				ProcessingIndicatorCodedList.AllDocumentsOrAsSpecifiedToBeProduced))
			{
				return processingIndicator.ProcessingIndicatorCoded.ToString();
			}
		}

		return ZString.Empty;
	}

	ZString ICUSRESHeader.ReleaseNumber => releaseNumber ??= GetReleaseNumber();
	ZString? releaseNumber;

	ZString GetReleaseNumber()
	{
		foreach (SegmentGroup4 group4 in cusresMessage.Group4)
		{
			var reference = group4.RFF[0].Reference;
			if (reference.ReferenceQualifier == ReferenceQualifierList.CustomsDeclarationNumber)
			{
				return reference.ReferenceNumber;
			}
		}

		return ZString.Empty;
	}

	ZDateTime ICUSRESHeader.ReleaseDate => releaseDate ??= GetReleaseDate();
	ZDateTime? releaseDate;

	ZDateTime ICUSRESHeader.LimitDate => limitDate ??= GetLimitDate();
	ZDateTime? limitDate;

	ZDateTime ICUSRESHeader.CreationDate => creationDate ??= GetCreationDate();
	ZDateTime? creationDate;

	ZDateTime GetReleaseDate()
	{
		foreach (DTMSegment dtm in cusresMessage.DTM)
		{
			var dateTimePeriod = dtm.DateTimePeriod;
			if (dateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.ClearanceDateCustoms
				&& dateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmdd)
			{
				ZDateTime.TryParseExact(dateTimePeriod.DateTimePeriod, out var result, CustomsDateTimeExtension.DateFormat);
				return result;
			}
		}

		return ZDateTime.Invalid;
	}

	ZDateTime GetLimitDate()
	{
		foreach (DTMSegment dtm in cusresMessage.DTM)
		{
			var dateTimePeriod = dtm.DateTimePeriod;
			if (dateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.AvailabilityDueDate
				&& dateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmdd)
			{
				ZDateTime.TryParseExact(dateTimePeriod.DateTimePeriod, out var result, CustomsDateTimeExtension.DateFormat);
				return result;
			}
		}

		return ZDateTime.Invalid;
	}

	ZDateTime GetCreationDate()
	{
		foreach (BGMSegment bgm in cusresMessage.BGM)
		{
			var dateTimePeriod = bgm.DateTimePeriod1;
			if (dateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.DocumentMessageDateTime
				&& dateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmdd)
			{
				ZDateTime.TryParseExact(dateTimePeriod.DateTimePeriod, out var result, CustomsDateTimeExtension.DateFormat);
				return result;
			}
		}

		return ZDateTime.Invalid;
	}
}
