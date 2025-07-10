using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC051CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE051>(movementHeader)
{
	protected override bool UseExtendedGlobalStyle => true;

	protected override void InterpretCore(IIE051 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.IE051);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(@class: CommonStrings.NoBorderBoldFont,
			paramValues: new ParamValueCollection {
				{ CommonStrings.LRN, ": " + dataProvider.LRN },
				{ TransitOperation.LRNSubmissionDate, ": " + dataProvider.TransitOperation?.DeclarationSubmissionDateAndTime },
				{ CommonStrings.MRN, ": " + dataProvider.MRN },
				{ TransitOperation.MessageSentOn, ": " + dataProvider.PreparationDateAndTime },
				{ TransitOperation.CustomsOfficeOfDeparture, ": " + Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDeparture) },
				{ TransitOperation.CustomsOfficeOfDestination, ": " + Factory.GetOfficeCodeWithDescription(dataProvider.CountrySpecificDataPL?.CustomsOfficeOfDestination) },
				{ TransitOperation.DeclarationType, ": " + dataProvider.CountrySpecificDataPL?.DeclarationType },
				{ CountrySpecificDataPL.NumberOfPackages, ": " + dataProvider.CountrySpecificDataPL.NumberOfPackages },
				{ CountrySpecificDataPL.TotalGoodsItems, ": " + dataProvider.CountrySpecificDataPL.TotalNumberOfConsignmentItems },
				{ TransitOperation.GrossWeight, ": " + dataProvider.CountrySpecificDataPL?.TotalGrossMass },
				{ TransitOperation.ReleaseRejectionCode, ": " + Factory.GetNoReleaseMotivationCodeWithDescription(dataProvider.TransitOperation?.NoReleaseMotivationCode) },
				{ TransitOperation.AdditionalRejectionRemark, ": " + dataProvider.TransitOperation?.NoReleaseMotivationText },
			});
		htmlWriter.WriteThematicBreak();

		if (dataProvider.Representative != null)
		{
			htmlWriter.WriteParamValueSequence(
				caption: Representative.Caption,
				paramValues: GetRepresentative(dataProvider.Representative));
			htmlWriter.WriteThematicBreak();
		}

		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}

	static IEnumerable<IParamValue> GetRepresentative(ICC051CRepresentative representative)
	{
		if (representative.IdentificationNumber != null)
		{
			yield return new ParamValue(Representative.EORI, representative.IdentificationNumber);
		}
	}
}
