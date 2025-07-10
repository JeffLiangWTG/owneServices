using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public sealed class RFIMessageInterpreter : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var stringBuilder = new ZStringBuilder();
		stringBuilder.Append(NLEDIMessageInterpreterHelper.FontAndStyle);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.MRN, ((CusEntryHeader)responseMessage.EM_LinkedObject).EntryNumber);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.FunctionalReferenceID, ((CusEntryHeader)responseMessage.EM_LinkedObject).CH_BGMReference);
		foreach (var controlResult in dataProvider.ControlResults)
		{
			var controlResultControl = controlResult.Controls.FirstOrDefault();
			var controlResultTypeList = new ControlTypeList();
			var controlTypeDescription = controlResultTypeList.GetDescriptionFromCode(controlResultControl?.TypeCode);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlDate, DMSResponseMessageHelper.GetFormattedShortDateString(controlResultControl?.InspectionStartDate), NLEDIMessageInterpreterHelper.TableHeaderIndentation);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlType, controlResultControl?.TypeCode + ": " + controlTypeDescription, NLEDIMessageInterpreterHelper.TableHeaderIndentation2);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.AppliesToEntryLine, controlResult.GoodsItemNumeric.Value.ToString(), NLEDIMessageInterpreterHelper.TableHeaderIndentation2);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlStatement, controlResultControl?.AdditionalInfoStatementDescription, NLEDIMessageInterpreterHelper.TableHeaderIndentation2);

			if (controlResultControl.ControlDetails != null)
			{
				foreach (var controlDetail in controlResultControl.ControlDetails)
				{
					NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.CorrectedValue, controlDetail.CorrectedAttributeValue, NLEDIMessageInterpreterHelper.TableHeaderIndentation2);
					NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.NamePath, controlDetail.PointerLocation, NLEDIMessageInterpreterHelper.TableHeaderIndentation3);
					NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlStatement, controlDetail.AdditionalInfoStatementDescription, NLEDIMessageInterpreterHelper.TableHeaderIndentation3);
				}
			}
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, ZString.Empty, "&nbsp;");
		}
		stringBuilder.Append(NLEDIMessageInterpreterHelper.EndTable);

		return stringBuilder.ToString();
	}
}
