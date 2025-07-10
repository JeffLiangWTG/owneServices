using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC029CMessageInterpreter : IMessageInterpreter<ICC029CDataProvider>
{
	public string Interpret(ICC029CDataProvider dataProvider)
	{
		const string format = "dd/MM/yyyy";
		var note = new ZStringBuilder();
		note.Append((NoResString)"New detailed status: Goods Released for Transit at Departure.");
		note.Append((NoResString)"Status granted on " + dataProvider.ReleaseDate?.ToString(format));
		note.Append((NoResString)"Acceptance Date " + dataProvider.DeclarationAcceptanceDate.ToString(format));
		if (dataProvider.ControlResult != null)
		{
			note.Append((NoResString)"Control Result: " + dataProvider.ControlResult.Code + " " + new CC029CControlCodeDescriptions().GetDescriptionFromCode(dataProvider.ControlResult.Code));
			note.Append((NoResString)"Controlled by: " + dataProvider.ControlResult.ControlledBy);
			note.Append((NoResString)"Text: " + dataProvider.ControlResult.Text);
			note.Append((NoResString)"Date: " + dataProvider.ControlResult.Date.ToString(format));
		}

		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
