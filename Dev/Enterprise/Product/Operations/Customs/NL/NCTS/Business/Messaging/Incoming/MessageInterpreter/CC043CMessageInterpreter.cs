using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC043CMessageInterpreter : IMessageInterpreter<ICC043CDataProvider>
{
	public string Interpret(ICC043CDataProvider dataProvider)
	{
		var continueUnloadingText = dataProvider.CTLControlContinueUnloading > 0 ? (NoResString)"Continue" : (NoResString)"Started";
		var systemDateTime = ZDateTime.Now;

		var note = new ZStringBuilder();
		note.Append($"Unloading Permission: {continueUnloadingText}.");
		note.Append($"Status granted on: {systemDateTime:dd/MM/yyyy hh:mm:ss}.");
		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
