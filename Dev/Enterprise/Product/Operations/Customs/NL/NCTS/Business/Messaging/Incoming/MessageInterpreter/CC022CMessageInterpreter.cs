using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC022CMessageInterpreter : IMessageInterpreter<ICC022CDataProvider>
{
	public string Interpret(ICC022CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();

		note.Append((NoResString)"New declaration status: Amendment Requested");
		if (dataProvider.AmendmentNotificationDateAndTime is DateTime amendmentNotificationDateAndTime)
		{
			note.Append(ZString.Format((NoResString)"Declaration received a request to amend the declaration on {0}", amendmentNotificationDateAndTime.ToString("dd-MMM-y HH:mm:ss")));
		}
		else
		{
			note.Append((NoResString)"Declaration received a request to amend the declaration");
		}

		note.Append((NoResString)ZString.Empty);
		foreach (var functionalError in dataProvider.FunctionalErrors)
		{
			note.Append(ZString.Format((NoResString)"{0}. Functional error code: {1}", functionalError.SequenceNumeric, functionalError.ErrorCode));
			note.Append(ZString.Format((NoResString)"Reason: {0}", functionalError.ErrorReason));
			note.Append(ZString.Format((NoResString)"Attribute: {0}", functionalError.ErrorPointer));
			note.Append(ZString.Format((NoResString)"Element in declaration contains now the value: {0}", functionalError.OriginalAttributeValue));
			note.Append((NoResString)ZString.Empty);
		}

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
