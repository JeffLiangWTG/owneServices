using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public sealed class IE460And560MessageInterpreter : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var nlResponseMessage = (NLEDIMessage)responseMessage;
		var stringBuilder = new ZStringBuilder();
		var additionalInformation = dataProvider.AdditionalInformations.FirstOrDefault();
		var control = dataProvider.Controls.FirstOrDefault();
		var controlTypeList = new ControlTypeList();

		stringBuilder.Append((NoResString)"<font size='2' face='Courier New' ><H1>");
		stringBuilder.Append(NLEDIMessageInterpreterHelper.ResStrings.ControlStatementTitle);
		stringBuilder.Append("</H1><BR/>");
		stringBuilder.Append((NoResString)"<table style='margin-left: 10pt'>");
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.MRN, dataProvider.Declaration?.Id, ZString.Empty);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.FunctionalReferenceID, dataProvider.Declaration?.FunctionalReference, ZString.Empty);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementType, GetValidStatementTypeCode(additionalInformation?.StatementTypeCode), ZString.Empty);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementDescription, control?.AdditionalInfoStatementDescription, ZString.Empty);
		stringBuilder.Append((NoResString)"</table>");

		stringBuilder.Append("<BR><H1>");
		stringBuilder.Append(NLEDIMessageInterpreterHelper.ResStrings.ControlDetailsTitle);
		stringBuilder.Append("</H1>");

		foreach (var controlItem in dataProvider.Controls)
		{
			stringBuilder.Append((NoResString)"<BR/><table style='margin-left: 10pt'>");
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlDate, DMSResponseMessageHelper.GetFormattedShortDateString(controlItem.InspectionStartDate), ZString.Empty);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlType, controlTypeList.GetDescriptionFromCode(controlItem.TypeCode), ZString.Empty);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlRemarks, controlItem.AdditionalInfoStatementDescription, ZString.Empty);
			stringBuilder.Append((NoResString)"</table>");
		}

		stringBuilder.Append("<BR><H1>");
		stringBuilder.Append(NLEDIMessageInterpreterHelper.ResStrings.RequestedDocuments);
		stringBuilder.Append("</H1>");

		foreach (var requestedDocument in dataProvider.RequestedDocuments)
		{
			stringBuilder.Append((NoResString)"<BR/><table style='margin-left: 10pt'>");
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.RequestedDocument, requestedDocument.Description, ZString.Empty);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.AppliesTo, DMSResponseMessageHelper.GetRefNumbersForRequestedDocument((CusEntryHeader)responseMessage.EM_LinkedObject, requestedDocument.TypeCode), ZString.Empty);
			stringBuilder.Append((NoResString)"</table>");
		}

		return stringBuilder.ToString();
	}
}
