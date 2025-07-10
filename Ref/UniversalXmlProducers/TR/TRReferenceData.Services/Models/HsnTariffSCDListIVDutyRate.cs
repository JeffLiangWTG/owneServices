using CargoWise.RefDbRepo.Common.Argument;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class HsnTariffSCDListIVDutyRate
	{
		public HsnTariffSCDListIVDutyRate(string tariffCodePattern, string description, decimal rate, IEnumerable<string> exemptedTariffCodes, string additionalCode, string startDate, string endDate)
		{
			TariffCodePattern = Argument.NotNullOrEmpty(tariffCodePattern, nameof(tariffCodePattern));
			Description = Argument.NotNullOrEmpty(description, nameof(description));
			Rate = rate;
			ExemptedTariffCodes = exemptedTariffCodes;
			AdditionalCode = additionalCode;

			StartDate = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
			EndDate = DateTime.Parse(endDate, CultureInfo.InvariantCulture);
		}

		public string TariffCodePattern { get; }

		public string Description { get; }

		public decimal Rate { get; }

		public IEnumerable<string> ExemptedTariffCodes { get; }

		public string AdditionalCode { get; }

		public string AdditionalDescription { get; }

		public DateTime StartDate { get; }

		public DateTime EndDate { get; }
	}
}
