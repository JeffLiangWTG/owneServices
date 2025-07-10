namespace Enterprise.MarketingManager.Business
{
	public class OpportunityCreationChartData
	{
		public int CurrentCount { get; set; }

		public int WonCount { get; set; }

		public int OtherCount { get; set; }

		public int LostCount { get; set; }

		public int TotalCount { get; set; }

		public decimal WinRatio { get; set; }
	}
}
