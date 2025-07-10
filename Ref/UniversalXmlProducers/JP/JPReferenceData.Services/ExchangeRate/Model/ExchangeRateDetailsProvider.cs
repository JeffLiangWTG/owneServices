namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ExchangeRateDetailsProvider : IExchangeRateDetails
	{
		public ExchangeRateDetailsProvider(string code, decimal rate)
		{
			Code = code;
			Rate = rate;
		}

		public string Code { get; private set; }

		public decimal Rate { get; private set; }
	}
}
