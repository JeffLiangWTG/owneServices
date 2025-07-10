using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC019CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE019>(movementHeader)
{
	protected override void InterpretCore(IIE019 ie019, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.IE019);
		htmlWriter.WriteThematicBreak();

		var transitOperation = ie019.TransitOperation;
		htmlWriter.WriteParamValueTable(
			paramValues: new ParamValueCollection
			{
				{ CommonStrings.MRN, ie019.MRN },
				{ TransitOperation.MessageSentOn, ie019.PreparationDateAndTime },
				{ TransitOperation.DiscrepanciesNotificationDate, transitOperation.DiscrepanciesNotificationDate },
				{ TransitOperation.DiscrepanciesNotificationText, transitOperation.DiscrepanciesNotificationText },
				{ TransitOperation.CustomsOfficeOfDeparture, Factory.GetOfficeCodeWithDescription(ie019.CustomsOfficeOfDeparture) },
			});
		htmlWriter.WriteThematicBreak();

		htmlWriter.Interpret(ie019.Guarantor).WriteThematicBreak();

		htmlWriter.Interpret(NctsHeader, ie019.HolderOfTheTransitProcedure);
	}
}
