using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC045CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE045>(movementHeader)
{
	protected override void InterpretCore(IIE045 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE045);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			paramValues: new ParamValueCollection {
				{ CommonStrings.MRN, dataProvider.MRN },
				{ TransitOperation.WriteOffDate, dataProvider.WriteOffDate },
				{ TransitOperation.CustomsOfficeOfDeparture, MessageInterpreterHelper.GetOfficeCodeWithDescription(Factory, dataProvider.CustomsOfficeOfDeparture) },
			});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: Guarantor.Caption,
			paramValues: new ParamValueCollection {
				{ Guarantor.EORI, dataProvider.Guarantor?.IdentificationNumber },
				{ Guarantor.Name, dataProvider.Guarantor?.Name },
				{ Guarantor.StreetAddress, dataProvider.Guarantor?.Address?.StreetAndNumber },
				{ Guarantor.Postcode, dataProvider.Guarantor?.Address?.PostCode },
				{ Guarantor.City, dataProvider.Guarantor?.Address?.City },
				{ Guarantor.Country, dataProvider.Guarantor?.Address?.CountryCode },
			});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}
}
