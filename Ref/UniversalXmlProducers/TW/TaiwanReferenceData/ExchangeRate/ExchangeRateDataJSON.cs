using System;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class ExchangeRateDataJSON : IExchangeRateData
	{
		public ExchangeRateDataJSON(string currency, DateTime startTime, DateTime endTime, decimal inRate, decimal exRate)
		{
			Currency = currency;
			StartDate = startTime;
			EndDate = endTime;
			InRate = inRate;
			ExRate = exRate;
		}

		public string Currency { get; private set; }

		public DateTime StartDate { get; private set; }

		public DateTime EndDate { get; private set; }

		public decimal InRate { get; private set; }

		public decimal ExRate { get; private set; }
	}
}
