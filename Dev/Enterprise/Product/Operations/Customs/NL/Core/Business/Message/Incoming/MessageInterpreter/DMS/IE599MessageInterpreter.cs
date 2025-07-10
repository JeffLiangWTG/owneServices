using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class IE599MessageInterpreter : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage responseMessage)
	{
		var stringBuilder = new ZStringBuilder();
		var control = dataProvider.Controls.FirstOrDefault();
		var statementDescription = control?.ControlResultExitTime != null ? Res.GetString("A2499553-26E4-43A3-AE93-565B1FAA4D47", "Exit confirmed") : Res.GetString("E9A4703B-C48B-49B5-B9E6-207309DB0B17", "Exit stopped");

		stringBuilder.Append(NLEDIMessageInterpreterHelper.FontAndStyle);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.EventType, Res.GetString("2ED1E1AE-9CC1-410F-89E3-669E911A794E", "Exit information"));

		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.StatementDescription, statementDescription);
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ExitDate, DMSResponseMessageHelper.GetFormattedShortDateString(control?.ControlResultExitTime));
		NLEDIMessageInterpreterHelper.AppendTableRow(stringBuilder, NLEDIMessageInterpreterHelper.ResStrings.ControlResults, control?.ControlResultID);
		stringBuilder.Append(NLEDIMessageInterpreterHelper.EndTable);

		return stringBuilder.ToString();
	}
}
