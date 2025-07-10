namespace Enterprise.Customs.US.Business
{
	using CargoWise.Types;

	class PSMonthCalculator
	{
		public ZString GetMonth(ZDate baseDate)
		{
			return baseDate.IsValid ? baseDate.AddMonths(1).ToString("MM") : string.Empty;
		}
	}
}
