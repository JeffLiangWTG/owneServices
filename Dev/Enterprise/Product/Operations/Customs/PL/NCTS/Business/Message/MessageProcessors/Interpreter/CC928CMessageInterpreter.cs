using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC928CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE928>(movementHeader)
{
	protected override bool UseExtendedGlobalStyle => true;

	protected override void InterpretCore(IIE928 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.IE928);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(@class: CommonStrings.NoBorderBoldFont,
			paramValues: new ParamValueCollection {
				{ CommonStrings.LRN, ": " + dataProvider.LRN },
				{ TransitOperation.MessageSentOn, ": " + dataProvider.PreparationDateAndTime },
				{ TransitOperation.CustomsOfficeOfDeparture, ": " + MessageInterpreterHelper.GetOfficeCodeWithDescription(Factory, dataProvider.CustomsOfficeOfDeparture) },
			});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}
}
