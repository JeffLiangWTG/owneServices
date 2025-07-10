using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData
{
	public class SIMARatesFactory
	{
		public RefCusTariff GetSIMARate(WebSIMAText webSIMAText)
		{
			Argument.NotNull(webSIMAText, nameof(webSIMAText));

			var relationships = new List<RefCusTariffRelationship>();
			foreach (var classification in webSIMAText.ClassificationNumbers)
			{
				relationships.Add(new RefCusTariffRelationship
				{
					ZZH_TariffCode = classification.Replace(".", "")
				});
			}
			var result = new RefCusTariff()
			{
				ZZ1_TariffCode = webSIMAText.CBSAReferenceNumber,
				ZZ1_Description = webSIMAText.Description,
				ZZ1_ZZI_NKTariffType = Constants.SpecialImportMeasureAct
			};
			result.RefCusTariffRelationships = relationships.ToArray();

			var rates = new List<RefCusRate>();

			foreach (var duty in webSIMAText.Duties)
			{
				var rate = new RefCusRate()
				{
					ZZ2_StartDate = DateTime.Parse(!string.IsNullOrEmpty(duty.EffectiveDate) ? duty.EffectiveDate : webSIMAText.DeterminationDate),
					ZZ2_ZY1_NKRateCode = duty.DutyType,
					ZZ2_RateFormula = duty.DutyValue.Equals(Constants.DefaultValues.Undefined, StringComparison.Ordinal) ? "0" : duty.DutyValue,
					ZZ2_RX_NKCurrencyOverride = duty.DutyCurrency
				};
				rates.Add(rate);
				var applicabilities = new List<RefCusApplicability>();
				foreach (var countryOfOrigin in duty.CountryOfOriginOrExport)
				{
					applicabilities.Add(new RefCusApplicability
					{
						ZZT_StartDate = rate.ZZ2_StartDate,
						ZZT_ZZA_NKTradeGroup = countryOfOrigin
					});
				}
				rate.RefCusApplicabilities = applicabilities.ToArray();
			}
			result.RefCusRates = rates.ToArray();
			result.ZZ1_StartDate = rates.Min(x => x.ZZ2_StartDate);
			return result;
		}
	}
}
