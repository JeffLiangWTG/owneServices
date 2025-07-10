using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public class RefExchangeRateElement
	{
		public string ZZN_ExRateType { get; set; }
		public decimal ZZN_Rate { get; set; }
		public string ZZN_RX_NKExCurrency { get; set; }
		public string ZZN_RN_NKCountry { get; set; }
		public DateTime ZZN_StartDate { get; set; }
		public DateTime ZZN_EndDate { get; set; }
	}
}
