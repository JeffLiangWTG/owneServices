using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class DeclarationPrelimStatementPrintDateValidator : PrelimStatementPrintDateValidator
	{
		protected override void ValidateDefaultedPSD(ZPropertyInfo prelimStatementDateInfo, ZDateTime statementPrintDate, IPrelimStatementDetailsDefault statementDateDefaultee)
		{
			AddInfoJobDeclarationWorkingDate workingDate = new AddInfoJobDeclarationWorkingDate();
			ZDateTime calculatedDefaultDate = workingDate.GeneratePrelimStmtDate(statementDateDefaultee);

			if (calculatedDefaultDate != statementPrintDate)
			{
				ZInt daysAdded = workingDate.DaysToAddToStatementDate(statementDateDefaultee.Branch.Company.PK, statementDateDefaultee.Branch.PK, statementDateDefaultee.IOR);
				ZInt iOROverridenDaysAdded = workingDate.IOROverridenDays(statementDateDefaultee.IOR);
				ZString notDefaultedWarningMessage = string.Format(PrelimStatementPrintDateNotDefaulted, calculatedDefaultDate.ToShortDateString(), statementDateDefaultee.DatePrecedenceMessage, "(" + daysAdded + RegistryWorkingDays);

				if (iOROverridenDaysAdded > 0)
				{
					notDefaultedWarningMessage = string.Format(PrelimStatementPrintDateNotDefaulted, calculatedDefaultDate.ToShortDateString(), statementDateDefaultee.DatePrecedenceMessage, "(" + iOROverridenDaysAdded + ImporterOfRecordWorkingDays);
				}

				prelimStatementDateInfo.AddWarning(notDefaultedWarningMessage);
			}
		}
	}
}
