using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC006CMessageInterpreter : IMessageInterpreter<ICC006CDataProvider>
{
	public string Interpret(ICC006CDataProvider dataProvider)
	{
		var note = new ZStringBuilder();
		note.Append((NoResString)"Arrival advice for NCTS departure received.");
		note.Append($"The movement arrived on {dataProvider.ArrivalDateAndTimeActual:dd/MM/yyyy hh:mm:ss} at office of destination {dataProvider.CustomsOfficeOfDestinationActualReferenceNumber}.");
		note.Append((NoResString)"The guarantee for this movement is credited and be used for a new movement.");
		return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
	}
}
