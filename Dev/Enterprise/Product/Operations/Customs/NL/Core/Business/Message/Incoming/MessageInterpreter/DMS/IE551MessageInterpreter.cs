using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public sealed class IE551MessageInterpreter : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var stringBuilder = new ZStringBuilder();
		stringBuilder.Append(NLEDIMessageInterpreterHelper.FontAndStyle);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.EventType, Res.GetString("C8405CEF-BF06-48DB-9ECE-BF726E33B150", "Customs Decision"));
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementDescription, NLEDIMessageInterpreterHelper.ResStrings.CancelledByCustoms);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.CancellationDate, DMSResponseMessageHelper.GetFormattedLocalLongTimeString(responseMessage.EM_MessageDateTime.ToDateTime()));
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.Remark, NLEDIMessageInterpreterHelper.ResStrings.CancelledDueToNoFollowUpOnOutstandingRequests);

		stringBuilder.Append(NLEDIMessageInterpreterHelper.EndTable);

		return stringBuilder.ToString();
	}
}
