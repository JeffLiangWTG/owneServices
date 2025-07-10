using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC004CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE004>(movementHeader)
{
	protected override void InterpretCore(IIE004 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE004);
		htmlWriter.WriteThematicBreak();

		var transitOperation = dataProvider.TransitOperation;
		htmlWriter.WriteParamValueTable(paramValues: new ParamValueCollection {
			{ CommonStrings.LRN, dataProvider.LRN },
			{ CommonStrings.MRN, dataProvider.MRN },
			{ TransitOperation.AmendmentSubmissionDateAndTime, transitOperation.AmendmentSubmissionDateAndTime },
			{ TransitOperation.AmendmentAcceptanceDateAndTime, transitOperation.AmendmentAcceptanceDateAndTime },
			{ TransitOperation.CustomsOfficeOfDeparture, NctsHeader.Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDeparture) },
		});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}
}
