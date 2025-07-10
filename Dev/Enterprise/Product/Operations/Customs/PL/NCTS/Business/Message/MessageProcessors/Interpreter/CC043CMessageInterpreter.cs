using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC043CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE043>(movementHeader)
{
	protected override void InterpretCore(IIE043 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE043);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			paramValues: new ParamValueCollection {
				{ CommonStrings.MRN, dataProvider.MRN },
				{ TransitOperation.DeclarationType, dataProvider.TransitOperation?.DeclarationType },
				{ TransitOperation.AcceptanceDate, dataProvider.TransitOperation?.DeclarationAcceptanceDate },
				{ TransitOperation.CustomsOfficeOfDeparture, dataProvider.CountrySpecificDataPLCustomsOfficeOfDeparture },
				{ CTLControl.ContinueUnloading, dataProvider.ContinueUnloading },
				{ Consignment.grossMass, dataProvider.GrossMassValue },
				{ CommonStrings.TotalNumberOfHouse, dataProvider.TotalCountOfHouseConsignment },
				{ CommonStrings.TotalNumberOfItems, dataProvider.TotalCountOfConsignmentItem },
			});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: TraderAtDestination.Caption,
			paramValues: new ParamValueCollection {
				{ TraderAtDestination.Eori, dataProvider.TraderAtDestination },
			});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
	}
}
