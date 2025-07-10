using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC022CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE022>(movementHeader)
{
	protected override void InterpretCore(IIE022 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.IE022);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(@class: CommonStrings.NoBorderBoldFont,
			paramValues: new ParamValueCollection {
				{ CommonStrings.MRN, ": " + dataProvider.MRN },
				{ TransitOperation.MessageSentOn, ": " + dataProvider.PreparationDateAndTime },
				{ TransitOperation.AmendmentNotificationDate, ": " + dataProvider.AmendmentNotificationDateAndTime },
				{ TransitOperation.CustomsOfficeOfDeparture, ": " + Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDeparture) },
			});
		htmlWriter.WriteThematicBreak();

		var functionalErrorInterpreter = new FunctionalErrorInterpreter(Factory);
		htmlWriter.WriteParamValuesTableTranspose(caption: FunctionalError.Caption,
			titleColumns: functionalErrorInterpreter.GetColumnTitles(),
			paramValuesList: functionalErrorInterpreter.GetRows(dataProvider.FunctionalErrors),
			rowIndex: true);

		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}
}
