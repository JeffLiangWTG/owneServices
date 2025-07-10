using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC019CMessageInterpreter : IMessageInterpreter<ICC019CDataProvider>
{
	public string Interpret(ICC019CDataProvider dataProvider)
	{
		const string format = "dd/MM/yyyy";
		var note = new ZStringBuilder();
		note.Append((NoResString)"Discrepancies for NCTS departure received at: " + dataProvider.DiscrepanciesNotificationDate.ToString(format));
		note.Append((NoResString)(dataProvider.DiscrepanciesNotificationText.Length > 512 ? dataProvider.DiscrepanciesNotificationText.Substring(0, 512) : dataProvider.DiscrepanciesNotificationText));
		note.Append((NoResString)"The guarantor for this declaration is :");
		note.Append((NoResString)dataProvider.Guarantor.Id);
		note.Append((NoResString)dataProvider.Guarantor.Name);
		note.Append((NoResString)dataProvider.Guarantor.Address.StreetAndNumber);
		note.Append((NoResString)dataProvider.Guarantor.Address.Postcode + " " + dataProvider.Guarantor.Address.City);
		note.Append((NoResString)dataProvider.Guarantor.Address.Country);
		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
