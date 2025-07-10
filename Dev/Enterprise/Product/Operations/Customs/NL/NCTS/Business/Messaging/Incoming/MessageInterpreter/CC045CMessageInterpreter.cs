using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC045CMessageInterpreter : IMessageInterpreter<ICC045CDataProvider>
{
	public string Interpret(ICC045CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		note.Append((NoResString)$"Write-Off notification for NCTS departure received at {dataProvider.WriteOffDate:dd/MM/yyyy}");
		note.Append((NoResString)$"The guarantor for this declaration is {dataProvider.Guarantor?.Id} {dataProvider.Guarantor?.Name}");
		note.Append((NoResString)$"{dataProvider.Guarantor?.Address?.StreetAndNumber}");
		note.Append((NoResString)$"{dataProvider.Guarantor?.Address?.Postcode} {dataProvider.Guarantor?.Address?.City}");
		note.Append((NoResString)$"{dataProvider.Guarantor?.Address?.Country}");
		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
