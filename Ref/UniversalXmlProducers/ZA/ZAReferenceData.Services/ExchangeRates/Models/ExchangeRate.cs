using System;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates.Models
{
	public class ExchangeRate
	{
		public string CountryCode { get; set; }
		public string RateType { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Currency { get; set; }
		public decimal Rate { get; set; }
	}
}
