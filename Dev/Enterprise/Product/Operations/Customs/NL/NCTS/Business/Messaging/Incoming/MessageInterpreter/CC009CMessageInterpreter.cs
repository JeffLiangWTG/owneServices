using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business
{
	public sealed class CC009CMessageInterpreter : IMessageInterpreter<ICC009CDataProvider>
	{
		public string Interpret(ICC009CDataProvider dataProvider)
		{
			var note = new ZStringBuilder();
			note.Append($"New declaration status: {((dataProvider.Invalidation?.Decision ?? false) ? (NoResString)"Cancellation accepted" : (NoResString)"Cancellation refused")}");
			note.Append($"Status granted on: {dataProvider.Invalidation?.DecisionDateAndTimeValue:dd/MM/yyyy hh:mm:ss}");
			note.Append($"Request date and time to invalidate/cancel: {dataProvider.Invalidation?.RequestDateAndTimeValue:dd/MM/yyyy hh:mm:ss}");
			note.Append($"Initiated by customs: {((dataProvider.Invalidation?.InitiatedByCustoms ?? false) ? (NoResString)"yes" : (NoResString)"no")}");
			note.Append($"Justification: {dataProvider.Invalidation?.Justification}");
			note.Append($"Correlation id: {dataProvider.CorrelationIdentifier}");

			return note.ToStringWithDelimiterBetweenAppends(NL.Business.Common.NLConstants.HtmlContent.Break);
		}
	}
}
