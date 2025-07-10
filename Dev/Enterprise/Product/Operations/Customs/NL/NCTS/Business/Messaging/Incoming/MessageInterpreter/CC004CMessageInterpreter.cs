using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC004CMessageInterpreter : IMessageInterpreter<ICC004CDataProvider>
{
	public string Interpret(ICC004CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();

		note.Append((NoResString)"New declaration status: Amendment acceptance");
		if (dataProvider.AmendmentAcceptanceDateTime is DateTime amendmentAcceptanceDateTime)
		{
			note.Append(ZString.Format((NoResString)"Status granted on {0}", amendmentAcceptanceDateTime.ToString("dd-MMM-y HH:mm:ss")));
		}

		if (dataProvider.AmendmentSubmissionDateTime is DateTime amendmentSubmissionDateTime)
		{
			note.Append(ZString.Format((NoResString)"Amendment submission date and time: {0}", amendmentSubmissionDateTime.ToString("dd-MMM-y HH:mm:ss")));
		}

		note.Append(ZString.Format((NoResString)"Correlation id: {0}", dataProvider.CorrelationIdentifier));

		note.Append((NoResString)ZString.Empty);

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
