using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public sealed class IE456And556MessageInterpreter : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var stringBuilder = new ZStringBuilder();
		var additionalInformation = dataProvider.AdditionalInformations.FirstOrDefault();
		var control = dataProvider.Controls.FirstOrDefault();

		stringBuilder.Append((NoResString)"<font size='2' face='Courier New' ><H1>");
		stringBuilder.Append(NLEDIMessageInterpreterHelper.ResStrings.ErrorInformationTitle);
		stringBuilder.Append("</H1><BR/>");
		stringBuilder.Append((NoResString)"<table style='margin-left: 10pt'>");
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.MRN, dataProvider.Declaration?.Id, ZString.Empty);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.FunctionalReferenceID, dataProvider.BOReference, ZString.Empty);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementType, GetValidStatementTypeCode(additionalInformation?.StatementTypeCode), ZString.Empty);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementDescription, control?.AdditionalInfoStatementDescription, ZString.Empty);
		stringBuilder.Append((NoResString)"</table>");

		stringBuilder.Append("<BR><H1>");
		stringBuilder.Append(NLEDIMessageInterpreterHelper.ResStrings.ErrorDetailsTitle);
		stringBuilder.Append("</H1>");

		foreach (var error in dataProvider.Errors)
		{
			stringBuilder.Append((NoResString)"<BR/><table style='margin-left: 10pt'>");
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ErrorValidation, error.ValidationCode, ZString.Empty);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ErrorDescription, error.Description, ZString.Empty);
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ValueThatIsRejected, error.OriginalAttributeValue, ZString.Empty);

			foreach (var location in error.PointerLocations)
			{
				NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.AppliesTo, location, ZString.Empty);
			}

			stringBuilder.Append((NoResString)"</table>");
		}

		return stringBuilder.ToString();
	}
}
