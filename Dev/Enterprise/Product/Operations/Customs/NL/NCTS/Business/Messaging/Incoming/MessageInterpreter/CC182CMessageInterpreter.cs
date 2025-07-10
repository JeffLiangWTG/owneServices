using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC182CMessageInterpreter : IMessageInterpreter<ICC182CDataProvider>
{
	public string Interpret(ICC182CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();

		note.Append($"Incident Notification Forwarded");
		note.Append($"Status granted on {dataProvider.IncidentNotificationDateAndTime.ToString("dd/MM/yyyy 'at' HH:mm:ss")}");

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
