using System.Collections.Generic;
using System.Text;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC055CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE055>(movementHeader)
{
	protected override bool UseExtendedGlobalStyle => true;

	protected override void InterpretCore(IIE055 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE055);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			@class: CommonStrings.NoBorderBoldFont,
			paramValues: new ParamValueCollection {
				{ CommonStrings.LRN, NctsHeader.MovementHeader.BM_PaperlessInbondNum },
				{ CommonStrings.MRN, dataProvider.MRN },
				{ TransitOperation.MessageSentOn, dataProvider.PreparationDateAndTime },
				{ TransitOperation.AcceptanceDate, dataProvider.TransitOperation?.DeclarationAcceptanceDate },
				{ TransitOperation.CustomsOfficeOfDeparture, Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDeparture) },
			});
		htmlWriter.WriteThematicBreak();

		if (dataProvider.GuaranteeReferences is { Count: > 0 })
		{
			htmlWriter.WriteParamValueTable(@class: "fixed-table",
				caption: GuaranteeReferences.Caption,
				paramValues: GetGuaranteeReferences(dataProvider.GuaranteeReferences));
			htmlWriter.WriteThematicBreak();
		}

		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}

	static IEnumerable<IParamValue> GetGuaranteeReferences(IReadOnlyCollection<ICC055CGuaranteeReference> guaranteeReferences)
	{
		yield return new ParamValue(GuaranteeReferences.Number, GuaranteeReferences.InvalidationReason);

		foreach (var guaranteeReference in guaranteeReferences)
		{
			var referenceNumber = guaranteeReference.GRN;
			yield return new ParamValue(referenceNumber, GetGuaranteeReferenceInvalidationReasons(guaranteeReference.InvalidGuaranteeReasons));
		}
	}

	static string GetGuaranteeReferenceInvalidationReasons(IReadOnlyCollection<ICC055CInvalidGuaranteeReason> invalidGuaranteeReasons)
	{
		if (invalidGuaranteeReasons is null)
		{
			return null;
		}

		var result = new StringBuilder();
		foreach (var item in invalidGuaranteeReasons)
		{
			result.AppendLine($"{item.Code} : {item.Text}");
		}

		return result.ToString();
	}
}
