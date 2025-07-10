namespace Enterprise.Customs.US.Business
{
	public class EntryDutyCalculator
	{
		public EntryDutyCalculator(JobDeclaration declaration)
		{
			ensEntryOrFTZEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry ?? declaration.ActiveEntryHeaders.FTZEntry;
			isNoDutyCalc = declaration.US_NoDutyCalc;
		}

		public void CalculateActualDuty()
		{
			if (!isNoDutyCalc && ensEntryOrFTZEntry != null)
			{
				CalculateDuty(ensEntryOrFTZEntry.Declaration, ensEntryOrFTZEntry);
			}
		}

		public void CalculateDutyReportingDuty()
		{
			var declaration = !isNoDutyCalc && ensEntryOrFTZEntry != null ? ensEntryOrFTZEntry.Declaration : null;
			if (declaration != null && declaration.US_EnableENS && declaration.JE_MessageType == JobMessageTypeList.Codes.Import)
			{
				CalculateDutyReportingDuty(declaration, true);
				CalculateDutyReportingDuty(declaration, false);
			}
		}

		void CalculateDutyReportingDuty(JobDeclaration declaration, bool useSPI)
		{
			var reportingDeclaration = new ReportingDeclarationDutyDataProvider(declaration, useSPI);
			var reportingEntry = reportingDeclaration.GetOrCreate(ensEntryOrFTZEntry);
			CalculateDuty(reportingDeclaration, reportingEntry);
			reportingDeclaration.StoreResult();
		}

		void CalculateDuty(IDeclarationDutyDataProvider declaration, IEntryHeaderDutyDataProvider entry)
		{
			var calculatorManager = declaration.GetCalculationManager();
			calculatorManager.Calculate();

			if (declaration.IsFormalImport)
			{
				var shouldReportApportionError = !declaration.IsFixedTransportInstallations || !declaration.US_MonthlyFiling;
				new PayableMPFCalculator(shouldReportApportionError).Calculate(entry);
			}

			new LineDutyApportionManager().Apportion(declaration);
		}

		readonly CusEntryHeader ensEntryOrFTZEntry;
		readonly bool isNoDutyCalc;
	}
}
