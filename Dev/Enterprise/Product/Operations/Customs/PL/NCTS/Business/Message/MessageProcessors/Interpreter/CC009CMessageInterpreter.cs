using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC009CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE009>(movementHeader)
{
	protected override void InterpretCore(IIE009 ie009, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE009);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(new ParamValueCollection {
			{ CommonStrings.LRN, ie009.LRN },
			{ CommonStrings.MRN, ie009.MRN },
			{ TransitOperation.MessageSentOn, ie009.PreparationDateAndTime },
			{ TransitOperation.CustomsOfficeOfDeparture, Factory.GetOfficeCodeWithDescription(ie009.CustomsOfficeOfDepartureReferenceNumber) },
			{ Invalidation.InvalidationRequestedDate, ie009.Invalidation.RequestDateAndTime },
			{ Invalidation.Decision, ie009.Invalidation.Decision == Decision.Item1 ? Invalidation.RequestAccepted : Invalidation.RequestRejected },
			{ Invalidation.DecisionDateAndTime, ie009.Invalidation.DecisionDateAndTime },
			{ Invalidation.Justification, ie009.Invalidation.Justification },
		});

		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: HolderOfTheTransitProcedure.Caption,
			paramValues: new HolderOfTheTransitProcedureInterpreter(ie009.HolderOfTheTransitProcedure).GetRows(NctsHeader));
	}
}
