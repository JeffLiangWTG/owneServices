using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public sealed class IE509MessageInterpreter : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var stringBuilder = new ZStringBuilder();
		var additionalInformation = dataProvider.AdditionalInformations.FirstOrDefault();
		var status = dataProvider.Statuses.FirstOrDefault();

		stringBuilder.Append(NLEDIMessageInterpreterHelper.FontAndStyle);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.EventType, Res.GetString("73FC25B3-B72B-4A50-A8C3-B720AF356D6E", "Customs Statement"));
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementType, GetValidStatementTypeCode(additionalInformation?.StatementTypeCode));
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementDescriptionType, additionalInformation?.StatementDescription);
		if (status?.EffectiveDateTime != null)
		{
			NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.InvalidationDate, DMSResponseMessageHelper.GetFormattedLocalLongTimeString(status.EffectiveDateTime.Value));
		}
		stringBuilder.Append(NLEDIMessageInterpreterHelper.EndTable);

		return stringBuilder.ToString();
	}
}
