using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class NLResponseEDIMessagePrettier : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var stringBuilder = new ZStringBuilder();
		var entryHeader = (CusEntryHeader)responseMessage.EM_LinkedObject;
		var additionalInformation = dataProvider.AdditionalInformations.FirstOrDefault();
		var control = dataProvider.Controls.FirstOrDefault();

		stringBuilder.Append(NLEDIMessageInterpreterHelper.FontAndStyle);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.MRN, entryHeader.EntryNumber);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.FunctionalReferenceID, entryHeader.CH_BGMReference);
		if (additionalInformation?.StatementTypeCode != null)
		{
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementType, GetValidStatementTypeCode(additionalInformation.StatementTypeCode));
		}
		if (control?.ControlResultDescription != null)
		{
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlRemarks, control.ControlResultDescription);
		}

		foreach (var addInfo in dataProvider.AdditionalInformations)
		{
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlRemarks, addInfo.StatementDescription);
		}

		foreach (var controlResult in dataProvider.ControlResults)
		{
			var controlResultControl = controlResult.Controls.FirstOrDefault();
			if (controlResultControl != null)
			{
				if (controlResultControl.InspectionStartDate != null)
				{
					NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, (NoResString)"<li>" + NLEDIMessageInterpreterHelper.ResStrings.ControlDate, DMSResponseMessageHelper.GetFormattedShortDateString(controlResultControl.InspectionStartDate.Value));
				}

				if (controlResult.GoodsItemNumeric != null)
				{
					NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.AppliesToEntryLine, controlResult.GoodsItemNumeric.Value.ToString(), NLEDIMessageInterpreterHelper.TableHeaderIndentation);
				}

				if (controlResultControl.TypeCode != null)
				{
					var controlTypeDescription = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(responseMessage.Factory, "3" + controlResultControl.TypeCode, Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today)?.ZZD_Description;
					NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlType, controlResultControl.TypeCode + ": " + controlTypeDescription, NLEDIMessageInterpreterHelper.TableHeaderIndentation);
				}

				if (controlResultControl.AdditionalInfoStatementDescription != null)
				{
					NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlStatement, controlResultControl.AdditionalInfoStatementDescription, NLEDIMessageInterpreterHelper.TableHeaderIndentation);
				}
			}

			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, ZString.Empty, "&nbsp;");
		}
		stringBuilder.Append(NLEDIMessageInterpreterHelper.EndTable);

		return stringBuilder.ToString();
	}
}
