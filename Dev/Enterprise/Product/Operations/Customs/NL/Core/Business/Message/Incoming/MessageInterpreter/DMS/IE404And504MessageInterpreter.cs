using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public sealed class IE404And504MessageInterpreter : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var stringBuilder = new ZStringBuilder();
		stringBuilder.Append(NLEDIMessageInterpreterHelper.FontAndStyle);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.AmendmentDate, DMSResponseMessageHelper.GetFormattedShortDateString(dataProvider.Declaration?.IssueDateTime));
		stringBuilder.Append(NLEDIMessageInterpreterHelper.EndTable);
		return stringBuilder.ToString();
	}
}
