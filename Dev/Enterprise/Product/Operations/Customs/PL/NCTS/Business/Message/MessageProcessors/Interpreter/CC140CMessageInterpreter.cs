using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC140CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE140>(movementHeader)
{
	protected override void InterpretCore(IIE140 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.IE140);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(@class: CommonStrings.NoBorderBoldFont,
			paramValues: new ParamValueCollection {
				{ CommonStrings.MRN, dataProvider.MRN },
				{ TransitOperation.MessageSentOn, dataProvider.PreparationDateAndTime },
				{ TransitOperation.NonArrivedMovementDate, dataProvider.TransitOperation.RequestOnNonArrivedMovementDate },
				{ TransitOperation.CustomsOfficeOfDeparture, MessageInterpreterHelper.GetOfficeCodeWithDescription(Factory, dataProvider.CustomsOfficeOfDeparture) },
				{ TransitOperation.CustomsOfficeOfEnquiryAtDeparture, MessageInterpreterHelper.GetOfficeCodeWithDescription(Factory, dataProvider.CustomsOfficeOfEnquiryAtDeparture) },
				{ TransitOperation.LimitForResponseDate, dataProvider.TransitOperation.LimitForResponseDate },
			});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueSequence(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(dataProvider.HolderOfTheTransitProcedure).GetRows(NctsHeader));
		htmlWriter.WriteThematicBreak();
	}
}
