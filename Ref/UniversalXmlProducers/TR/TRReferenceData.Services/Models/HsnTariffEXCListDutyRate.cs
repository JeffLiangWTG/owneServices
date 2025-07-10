using System;
using System.Collections.Generic;
using System.Globalization;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Models
{
	public class HsnTariffEXCListDutyRate
	{
		public HsnTariffEXCListDutyRate(string tariffNo, string description, string dutyAmount, string uomCU3, string uomCU4, string uomCU5, string[] exemptedTariffCodes, string additionalCode, string rateType, string rateCode, string rateFormula, string startDate, string endDate)
		{
			TariffNo = tariffNo;
			Description = description;
			DutyAmount = dutyAmount;
			UomCU3 = uomCU3;
			UomCU4 = uomCU4;
			UomCU5 = uomCU5;
			ExemptedTariffCodes = exemptedTariffCodes;
			AdditionalCode = additionalCode;
			RateType = rateType;
			RateCode = rateCode;
			RateFormula = rateFormula;
			StartDate = string.IsNullOrWhiteSpace(startDate) ? DateTime.MinValue : DateTime.Parse(startDate, CultureInfo.InvariantCulture);
			EndDate = string.IsNullOrWhiteSpace(endDate) ? DateTime.MaxValue : DateTime.Parse(endDate, CultureInfo.InvariantCulture);
		}

		public string TariffNo { get; }
		public string Description { get; }
		public string DutyAmount { get; }
		public string UomCU3 { get; }
		public string UomCU4 { get; }
		public string UomCU5 { get; }
		public IEnumerable<string> ExemptedTariffCodes { get; }
		public string AdditionalCode { get; }
		public string RateType { get; }
		public string RateCode { get; }
		public string RateFormula { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
	}
}
