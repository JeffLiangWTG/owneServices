using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business
{
	public class AutomatedClearinghouseMessageManager
	{
		#region Send Payment Message

		public void SendAuthorisation(StatementPaymentAction action)
		{
			MQEDIMessage message = GeneratePaymentMessage(action);
			action.StatementHeader.Messages.Add(message);
			action.StatementHeader.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;
			action.StatementHeader.B2_AccountNo = action.PayerUnitNo;
			action.StatementHeader.B2_PaymentParty = action.PaymentParty;
		}

		MQEDIMessage GeneratePaymentMessage(StatementPaymentAction action)
		{
			var builder = new AutomatedClearinghouseMessageBuilder(action.Factory);
			var payType = action.StatementHeader.IsPeriodicDailyStatement ? ACEStatementPayTypePeriodicDailyStatement : ACEStatementPayTypeDailyStatement;
			return builder.Generate<PDSPT>(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, action, payType);
		}

		const string ACEStatementPayTypePeriodicDailyStatement = "01";
		const string ACEStatementPayTypeDailyStatement = "02";

		#endregion

		#region Send Statement Delete/Add Message

		public bool GenerateStatementDeleteAdd(StatementDeleteAndSendingActionCollection actions)
		{
			bool messageGenerated = false;

			foreach (StatementDeleteAndSendingAction action in actions)
			{
				if (action.US_SendMessage)
				{
					StatementDeleteTransactionWrapper wrapper = new StatementDeleteTransactionWrapper(action.entity);
					wrapper.PaymentType = action.US_PaymentType;
					wrapper.PreliminaryStatementPrintDate = action.US_PreliminaryStatementPrintDate;
					wrapper.PeriodicStatementMonth = action.US_PeriodicStatementMonth;
					wrapper.ClientBranchDesignation = action.US_ClientBranchDesignation;

					MQEDIMessage message = new StatementUpdateMessageBuilder(wrapper).PopulateMessage();

					CusStatementLine statementLine = new CusStatementLine.Loader(action.Factory).LoadTop1NotDeleted(action.entity.EntryNumber, action.entity.EntryFilerCode, action.entity.RegistryCompanyPK);
					if (statementLine != null)
					{
						statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;
					}

					messageGenerated = true;
				}
			}

			if (messageGenerated)
			{
				if (actions.StatementHeader != null)
				{
					actions.StatementHeader.ActiveLines.Rebuild();
				}
			}

			return messageGenerated;
		}

		#endregion
	}
}
