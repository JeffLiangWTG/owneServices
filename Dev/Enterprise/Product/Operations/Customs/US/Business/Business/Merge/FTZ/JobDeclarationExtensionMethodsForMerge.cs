namespace Enterprise.Customs.US.Business
{
	static class JobDeclarationExtensionMethodsForMerge
	{
		public static ILineDutyFeeCalculator GetLineCalculator(this IDeclarationDutyDataProvider declaration)
		{
			if (declaration.IsFTZAdmission)
			{
				return new FTZLineDutyFeeCalculator(declaration);
			}
			else
			{
				return new LineDutyFeeCalculator();
			}
		}

		public static IDutyFeeCalculator GetCalculationManager(this IDeclarationDutyDataProvider declaration)
		{
			if (declaration.IsFTZAdmission)
			{
				return new FTZDutyFeeCalculator(declaration);
			}
			else
			{
				return new DutyFeeCalculationManager(declaration);
			}
		}
	}
}
