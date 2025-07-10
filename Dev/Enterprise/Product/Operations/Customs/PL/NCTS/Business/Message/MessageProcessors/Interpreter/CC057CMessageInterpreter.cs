using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;
using RefCusCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC057CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE057>(movementHeader)
{
	protected override bool UseExtendedGlobalStyle => true;

	protected override void InterpretCore(IIE057 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.IE057);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(@class: CommonStrings.NoBorderBoldFont,
			paramValues: new ParamValueCollection
			{
				{ CommonStrings.MRN, $": {dataProvider.MRN}" },
				{ TransitOperation.BusinessRejectionType, $": {GetBusinessRejectionType(dataProvider.TransitOperation)}" },
				{ TransitOperation.RejectionDateTime, $": {dataProvider.TransitOperation?.RejectionDateAndTime}" },
				{ TransitOperation.RejectionCode, $": {GetRejectionCode(dataProvider.TransitOperation)}" },
				{ TransitOperation.RejectionReason, $": {dataProvider.TransitOperation?.RejectionReason}" },
				{ TransitOperation.CustomsOfficeOfDestination, $": {Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDestinationActual)}" },
			});
		htmlWriter.WriteThematicBreak();

		var functionalErrorInterpreter = new FunctionalErrorInterpreter(Factory);
		htmlWriter.WriteParamValuesTableTranspose(caption: FunctionalError.Caption,
			titleColumns: functionalErrorInterpreter.GetColumnTitles(),
			paramValuesList: functionalErrorInterpreter.GetRows(dataProvider.FunctionalErrors),
			rowIndex: false);

		htmlWriter.WriteParamValueTable(@class: CommonStrings.NoBorderBoldFont,
			caption: TraderAtDestination.Caption,
			paramValues: new ParamValueCollection
			{
				{ TraderAtDestination.Eori, $": {dataProvider.TraderAtDestination}" },
			});
	}

	ZString GetBusinessRejectionType(ICC057CTransitOperation transitOperation)
		=> transitOperation != null
			? Factory.GetCodeWithDescription(RefCusCodes.Code_CL570, transitOperation.BusinessRejectionType, separator: "-")
			: ZString.Empty;

	ZString GetRejectionCode(ICC057CTransitOperation transitOperation)
		=> transitOperation != null
			? Factory.GetCodeWithDescription(RefCusCodes.Code_CL227, transitOperation.RejectionCode, separator: "-")
			: ZString.Empty;
}
