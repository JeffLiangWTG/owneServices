namespace Enterprise.Services.ServiceHost
{
	public class GetJobProfitTotalsResponse
	{
		public decimal Cost { get; set; }

		public decimal Revenue { get; set; }

		public decimal Profit { get; set; }

		public int LocalDecimals { get; set; }
	}
}
