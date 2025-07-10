using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC055CMessageInterpreter : IMessageInterpreter<ICC055CDataProvider>
{
	public string Interpret(ICC055CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		var reasons = new NCTS5InvalidGuaranteeReason();
		note.Append((NoResString)"Guarantee invalid.");
		foreach (var reference in dataProvider.GuaranteeReferences)
		{
			note.Append((NoResString)$"GRN: {reference.GRN}");
			foreach (var reason in reference.InvalidGuaranteeReasons)
			{
				note.Append((NoResString)$"Reason: Code: {reason.Code} {reasons.GetDescriptionFromCode(reason.Code)}");
				note.Append((NoResString)$"        Text: {reason.Text}");
			}
		}
		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
