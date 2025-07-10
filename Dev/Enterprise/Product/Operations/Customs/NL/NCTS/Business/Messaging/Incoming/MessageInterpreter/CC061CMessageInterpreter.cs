using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC061CMessageInterpreter : IMessageInterpreter<ICC061CDataProvider>
{
	public string Interpret(ICC061CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		note.Append((NoResString)"New Customs Status: 'Decision to Control Notification'");
		note.Append((NoResString)"Status granted on: " + dataProvider.ControlNotificationDateAndTime.ToString("dd/MM/yyyy HH:mm:ss"));
		note.Append((NoResString)"Date and time of Control: " + dataProvider.ControlNotificationDateAndTime.ToString("dd/MM/yyyy HH:mm:ss"));
		note.Append((NoResString)"Physical Control by Customs");

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
