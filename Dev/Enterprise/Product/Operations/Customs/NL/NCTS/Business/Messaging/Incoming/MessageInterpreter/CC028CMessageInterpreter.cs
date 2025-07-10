using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC028CMessageInterpreter : IMessageInterpreter<ICC028CDataProvider>
{
	public string Interpret(ICC028CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		note.Append((NoResString)"New declaration status: Declaration MRN Allocated");
		note.Append((NoResString)"Status granted on: " + dataProvider.DeclarationAcceptanceDate);
		note.Append((NoResString)"Correlation id: " + dataProvider.CorrelationIdentifier);

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
