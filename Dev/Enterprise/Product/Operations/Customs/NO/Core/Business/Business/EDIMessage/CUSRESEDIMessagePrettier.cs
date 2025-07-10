using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Edifact.V902.Messages.CUSRES;
using Enterprise.Edifact.V902.Segments;

namespace Enterprise.Customs.NO.Business;

sealed class CUSRESEDIMessagePrettier : IEDIMessagePrettier
{
	public CUSRESEDIMessagePrettier(CUSRESEDIMessage message)
	{
		this.message = Argument.NotNull(message, nameof(message));
	}

	readonly CUSRESEDIMessage message;

	ICodeDescriptionPairList ErrorCodes => message.Lookups.ErrorCodes;

	public ZString MakeHumanReadable()
	{
		var ediMessage = EdiMessageFactory.GetMessage(message);
		return ediMessage switch
		{
			CUSRESMessage cusresMessage => MakeHumanReadable(cusresMessage),
			_ => ZString.Empty,
		};
	}

	ZString MakeHumanReadable(CUSRESMessage ediMessage)
	{
		var sb = new ZStringBuilder();
		AppendErrorCodes(sb, ediMessage);
		AppendFreeText(sb, ediMessage);
		return sb.ToString();
	}

	#region AppendErrorCodes

	void AppendErrorCodes(ZStringBuilder sb, CUSRESMessage ediMessage)
	{
		foreach (SegmentGroup1 sg1 in ediMessage.Group1)
		{
			AppendErrorCodes(sb, sg1.ERP[0]);
			AppendErrorCodes(sb, sg1.ERC);
			sb.AppendLine();
		}
	}

	static void AppendErrorCodes(ZStringBuilder sb, ERPSegment erpSegment)
	{
		if (erpSegment is { ErrorPointDetails.MessageItemNumber: { Length: > 0 } erpText })
		{
			sb.AppendLine(erpText);
		}
	}

	void AppendErrorCodes(ZStringBuilder sb, ERCSegmentMessageSection ercSection)
	{
		foreach (ERCSegment ercSegment in ercSection)
		{
			// TODO: fix EDIFACT impl ERC segment missing multiple elements
			AppendErrorCodes(sb, ercSegment.ApplicationErrorDetail);
		}
	}

	void AppendErrorCodes(ZStringBuilder sb, ApplicationErrorDetailElements errorDetail)
	{
		if (errorDetail is { ApplicationErrorCoded: { Length: > 0 } errorCode })
		{
			var errorDescription = ErrorCodes.GetDescriptionFromCode($"{UniversalReferenceConstants.ErrorCodesPrefix}{errorCode}");
			sb.AppendLine($"{errorCode} {errorDescription}");
		}
	}

	#endregion

	#region AppendFreeText

	static void AppendFreeText(ZStringBuilder sb, CUSRESMessage ediMessage)
	{
		foreach (FTXSegment ftxSegment in ediMessage.FTX)
		{
			var textSubject = ftxSegment.TextSubjectQualifier;
			if (textSubject == TextSubjectQualifierList.ResponseFreeText ||
				textSubject == TextSubjectQualifierList.CustomsClearanceInstructionImport)
			{
				AppendFreeText(sb, ftxSegment.TextLiteral);
			}
		}
	}

	static void AppendFreeText(ZStringBuilder sb, TextLiteralElements textLiteral)
	{
		sb.Append(textLiteral.FreeText1);
		sb.Append(" ");
		sb.Append(textLiteral.FreeText2);
		sb.Append(textLiteral.FreeText3);
		sb.Append(textLiteral.FreeText4);
		sb.AppendLine(textLiteral.FreeText5);
	}

	#endregion
}
