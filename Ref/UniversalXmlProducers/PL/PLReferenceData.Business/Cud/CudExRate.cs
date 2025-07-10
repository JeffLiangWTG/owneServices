using System;
using System.Collections.Generic;
using System.Globalization;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Cud
{
	public class CudExRate
	{
		public List<CudExRateSingle> ExRateData { get; set; }
		public DateTime PublicationDate { get; set; }

		public CudExRate()
		{
			ExRateData = new List<CudExRateSingle>();
		}

		public void AddSingle(string currency, double rateString, DateTime startDate)
		{
			var rateSingle = new CudExRateSingle
			{
				CurrencyCode = currency,
				ExRate = Convert.ToString(rateString, CultureInfo.CurrentCulture),
				StartDate = startDate
			};
			ExRateData.Add(rateSingle);
		}
	}

	public class CudExRateSingle
	{
		public string CurrencyCode { get; set; }
		public string ExRate { get; set; }
		public DateTime StartDate { get; set; }

		public CudExRateSingle()
		{
			CurrencyCode = string.Empty;
			ExRate = string.Empty;
			StartDate = DateTime.UtcNow;
		}
	}
}
