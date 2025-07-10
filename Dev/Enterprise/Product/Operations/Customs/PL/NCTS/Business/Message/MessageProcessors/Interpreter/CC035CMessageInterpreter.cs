using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC035CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE035>(movementHeader)
{
	protected override void InterpretCore(IIE035 ie035, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(MessageTitles.IE035);
		htmlWriter.WriteThematicBreak();

		var recoveryNotification = ie035.RecoveryNotification;
		htmlWriter.WriteParamValueTable(new ParamValueCollection
		{
			{ CommonStrings.MRN, ie035.MRN },
			{ TransitOperation.MessageSentOn, ie035.PreparationDateAndTime },
			{ TransitOperation.DeclarationAcceptanceDate, $"{ie035.DeclarationAcceptanceDate}" },
			{ TransitOperation.CustomsOfficeOfDeparture, Factory.GetOfficeCodeWithDescription(ie035.CustomsOfficeOfDeparture) },
			{ RecoveryNotification.RecoveryNotificationDate, $"{recoveryNotification.RecoveryNotificationDate}" },
			{ RecoveryNotification.RecoveryNotificationText, recoveryNotification.RecoveryNotificationText },
			{ RecoveryNotification.RecoveryAmountClaimed, $"{recoveryNotification.AmountClaimed:F2} {recoveryNotification.Currency}" },
		});
		htmlWriter.WriteThematicBreak();

		htmlWriter.Interpret(ie035.Guarantor).WriteThematicBreak();
		htmlWriter.Interpret(NctsHeader, ie035.HolderOfTheTransitProcedure);
	}
}
