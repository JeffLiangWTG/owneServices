using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;
using RefCusCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC056CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE056>(movementHeader)
{
	protected override bool UseExtendedGlobalStyle => true;

	protected override void InterpretCore(IIE056 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.IE056);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(paramValues: new ParamValueCollection {
			{ CommonStrings.LRN, dataProvider.LRN },
			{ CommonStrings.MRN, dataProvider.MRN },
			{ TransitOperation.MessageSentOn, dataProvider.PreparationDateAndTime },
			{ TransitOperation.CustomsOfficeOfDeparture, Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDeparture) },
			{ TransitOperation.BusinessRejectionType, GetBusinessRejectionType(dataProvider?.TransitOperation) },
			{ TransitOperation.RejectionDateTime, dataProvider.TransitOperation?.RejectionDateAndTime },
			{ TransitOperation.RejectionCode, GetRejectionCode(dataProvider?.TransitOperation) },
			{ TransitOperation.RejectionReason, dataProvider.TransitOperation?.RejectionReason },
		});
		htmlWriter.WriteThematicBreak();

		var functionalErrorInterpreter = new FunctionalErrorInterpreter(Factory);
		htmlWriter.WriteParamValuesTableTranspose(caption: FunctionalError.Caption,
			titleColumns: functionalErrorInterpreter.GetColumnTitles(),
			paramValuesList: functionalErrorInterpreter.GetRows(dataProvider.FunctionalErrors),
			rowIndex: true);

		htmlWriter.WriteThematicBreak();
		if (dataProvider.Representative is not null)
		{
			var representativeCollection = new ParamValueCollection
			{
				{ Representative.EORI, dataProvider.Representative.IdentificationNumber },
				{ Representative.Status, string.Equals(dataProvider.Representative.Status, "2")
					? dataProvider.Representative.Status + " : " + Representative.Status2Description
					: dataProvider.Representative.Status },
			};

			htmlWriter.WriteParamValueTable(
				caption: Representative.Caption,
				paramValues: representativeCollection);
			htmlWriter.WriteThematicBreak();
		}

		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}

	ZString GetBusinessRejectionType(ICC056CTransitOperation transitOperation)
		=> transitOperation != null
			? Factory.GetCodeWithDescription(RefCusCodes.Code_CL560, transitOperation.BusinessRejectionType, separator: "=")
			: ZString.Empty;

	ZString GetRejectionCode(ICC056CTransitOperation transitOperation)
		=> transitOperation != null
			? Factory.GetCodeWithDescription(RefCusCodes.Code_CL226, transitOperation.RejectionCode, separator: "=")
			: ZString.Empty;
}
