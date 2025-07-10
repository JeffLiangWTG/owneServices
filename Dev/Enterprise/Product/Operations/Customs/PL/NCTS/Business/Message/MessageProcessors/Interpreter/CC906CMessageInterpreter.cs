using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC906CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE906>(movementHeader)
{
	readonly FunctionalErrorInterpreter functionalErrorInterpreter = new(movementHeader.Factory);

	protected override void InterpretCore(IIE906 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteParamValueTable(
			caption: MessageTitles.IE906,
			paramValues: GetDeclarationIdentificationNumbers(dataProvider));

		var functionalErrorRows = functionalErrorInterpreter.GetRows(dataProvider.FunctionalErrors);
		if (!functionalErrorRows.IsNullOrEmpty())
		{
			htmlWriter.WriteParamValuesTableTranspose(
				caption: FunctionalError.Caption,
				paramValuesList: functionalErrorRows,
				titleColumns: functionalErrorInterpreter.GetColumnTitles(),
				rowIndex: true);
		}
	}

	ParamValueCollection GetDeclarationIdentificationNumbers(IIE906 dataProvider)
	{
		var paramValues = new ParamValueCollection();

		if (!string.IsNullOrEmpty(dataProvider.LRN))
		{
			paramValues.Add(CommonStrings.LRN, dataProvider.LRN);
		}

		if (!string.IsNullOrEmpty(dataProvider.MRN))
		{
			paramValues.Add(CommonStrings.MRN, dataProvider.MRN);
		}

		paramValues.Add(TransitOperation.MessageSentOn, dataProvider.PreparationDateAndTime);

		return paramValues;
	}
}
