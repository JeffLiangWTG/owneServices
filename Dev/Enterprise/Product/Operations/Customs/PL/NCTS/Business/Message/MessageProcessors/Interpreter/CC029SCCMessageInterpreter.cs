using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC029SCCMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE029SC>(movementHeader)
{
	protected override void InterpretCore(IIE029SC dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE029SC);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(new ParamValueCollection {
			{ CommonStrings.LRN, dataProvider.LRN },
			{ CommonStrings.MRN, dataProvider.MRN },
			{ TransitOperation.MessageSentOn, dataProvider.AcceptanceDate },
			{ TransitOperation.CustomsOfficeOfDeparture, Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDeparture) },
			{ TransitOperation.CustomsOfficeOfDestination, Factory.GetOfficeCodeWithDescription(dataProvider.CustomsOfficeOfDestination) },
			{ TransitOperation.DeclarationType, dataProvider.DeclarationType },
			{ TransitOperation.ReleaseDate, dataProvider.ReleaseDate },
			{ TransitOperation.GrossWeight, dataProvider.GrossMass },
		});
	}
}
