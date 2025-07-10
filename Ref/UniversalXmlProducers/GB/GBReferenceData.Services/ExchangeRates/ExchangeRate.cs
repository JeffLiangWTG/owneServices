using System;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.ExchangeRates
{
	public class ExchangeRate
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Currency { get; set; }
		public decimal Rate { get; set; }

		public bool IsValid()
		{
			return !string.IsNullOrWhiteSpace(Currency) && Rate != 0;
		}
	}
}
