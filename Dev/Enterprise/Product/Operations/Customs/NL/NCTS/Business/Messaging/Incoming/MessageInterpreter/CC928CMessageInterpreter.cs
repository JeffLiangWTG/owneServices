using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC928CMessageInterpreter : IMessageInterpreter<ICC928CDataProvider>
{
	public string Interpret(ICC928CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		note.Append((NoResString)"New declaration status: 'Declaration accepted'");
		note.Append((NoResString)"Correlation id: " + dataProvider.CorrelationIdentifier);

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
