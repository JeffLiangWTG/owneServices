using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class StatementDateChangeRequest
	{
		public StatementDateChangeRequest(JobDeclaration declaration)
		{
			this.declaration = declaration;
			this.entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
		}

		readonly JobDeclaration declaration;
		readonly CusEntryHeader entry;

		public void GenerateStatementDateChangeRequestIfNecessary(int attemptNumber = 0)
		{
			if (declaration.Branch != null && declaration.JE_EntryAuthorisationDate.IsValid && !declaration.US_FixPSD && entry != null)
			{
				ZDate newStatementDate = GenerateNewPreliminaryStatementPrintDate();

				if (ShouldAutoGenerationSDCR(newStatementDate))
				{
					BuildStatementDateChangeRequest(newStatementDate, attemptNumber);
				}
			}
		}

		public void BuildStatementDateChangeRequestUsingCurrentPSD()
		{
			if (declaration.US_PreliminaryStatementPrintDate.IsValid)
			{
				BuildStatementDateChangeRequest(declaration.US_PreliminaryStatementPrintDate.Date);
			}
		}

		void BuildStatementDateChangeRequest(ZDate newStatementDate, int attemptNumber = 0)
		{
			if (entry != null)
			{
				StatementDeleteTransactionWrapper changeRequestWrapper = new StatementDeleteTransactionWrapper(declaration);

				changeRequestWrapper.PaymentType = entry.US_PaymentType;
				changeRequestWrapper.PreliminaryStatementPrintDate = newStatementDate;
				changeRequestWrapper.ClientBranchDesignation = entry.US_ClientBranchDesignation;

				if (PaymentTypeList.IsPeriodicPayment(declaration.US_PaymentType))
				{
					changeRequestWrapper.PeriodicStatementMonth = new PSMonthCalculator().GetMonth(declaration.JE_EntryAuthorisationDate.Date);
				}

				MQEDIMessage changeRequestMsg = new StatementUpdateMessageBuilder(changeRequestWrapper).PopulateMessage(attemptNumber);

				if (changeRequestMsg != null)
				{
					declaration.Messages.Add(changeRequestMsg);
				}
			}
		}

		#region Implementation

		bool ShouldAutoGenerationSDCR(ZDate newStatmentDate)
		{
			return new AutomaticSTUConditionChecker().ShouldSend(declaration, newStatmentDate, delegate
				{
					var shouldSendAutomatically = !declaration.IsLiveEntry || newStatmentDate < declaration.US_PSDAccepted;
					return !shouldSendAutomatically;
				});
		}

		ZDate GenerateNewPreliminaryStatementPrintDate()
		{
			return new AddInfoJobDeclarationWorkingDate().GeneratePrelimStmtDate(declaration);
		}

		#endregion
	}
}
