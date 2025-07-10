namespace Enterprise.Customs.US.Business
{
	class DeclarationPrelimStatementDetailsDefaulter : PrelimStatementDetailsDefaulter
	{
		protected override void DefaultPeriodicStatementMonth(IPrelimStatementDetailsDefault declaration)
		{
			if (!declaration.FixPSD)
			{
				if (PaymentTypeList.IsPeriodicPayment(declaration.US_PaymentType))
				{
					if (declaration.RegistryAllowsDefaulting)
					{
						declaration.US_PeriodicStatementMM = new PSMonthCalculator().GetMonth(declaration.BaseDateToCalculateOn);
					}
				}
				else
				{
					declaration.US_PeriodicStatementMM = "";
				}
			}
		}
	}
}
