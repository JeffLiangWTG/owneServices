using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class HsnTariffBanDataParser
	{
		public IEnumerable<RefCusTariff> PopulateTariffs(IEnumerable<RefCusTariff> tariffs)
		{
			var tariffRateRecords = GetTariffRateRecords();
			var tariffCodes = tariffRateRecords.Select(x => x.TariffCode).ToHashSet();
			var relatedTariffs = tariffs.Where(x => tariffCodes.Contains(x.ZZ1_TariffCode));

			foreach (var relatedTariff in relatedTariffs)
			{
				var refCusTariffAttributeToAdd = new RefCusTariffAttribute { ZZ3_Name = Constants.TariffAttribute.Name.ISETRADEBANDEROL, ZZ3_Value = Constants.TariffAttribute.Value.Y };
				relatedTariff.RefCusTariffAttributes = relatedTariff.RefCusTariffAttributes == null ? new[] { refCusTariffAttributeToAdd } : relatedTariff.RefCusTariffAttributes.Append(refCusTariffAttributeToAdd).ToArray();

				var relatedTariffRatesRecords = tariffRateRecords.Where(r => r.TariffCode == relatedTariff.ZZ1_TariffCode);
				foreach (var relatedTariffRateRecord in relatedTariffRatesRecords)
				{
					var refCusRate = relatedTariff.RefCusRates?.FirstOrDefault(x => x.ZZ2_ZY1_NKRateCode == Constants.TariffRateCode.Code._75 && x.ZZ2_ZY1_ZZR_NKRateType == Constants.TariffRateType.Code.BAN && x.ZZ2_RateFormula == relatedTariffRateRecord.Formula);
					if (refCusRate == null)
					{
						refCusRate = new RefCusRate()
						{
							ZZ2_RateFormula = relatedTariffRateRecord.Formula,
							ZZ2_RateFormulaDerivedFrom = relatedTariffRateRecord.Percent + "%",
							ZZ2_StartDate = Constants.HsnTariffRateStartDate,
							ZZ2_ZY1_NKRateCode = Constants.TariffRateCode.Code._75,
							ZZ2_ZY1_ZZR_NKRateType = Constants.TariffRateType.Code.BAN
						};

						relatedTariff.RefCusRates = relatedTariff.RefCusRates == null ? new[] { refCusRate } : relatedTariff.RefCusRates.Append(refCusRate).ToArray();
					}

					if (!string.IsNullOrEmpty(relatedTariffRateRecord.AdditionalCode) && !(refCusRate.RefCusApplicabilities?.Any(x => x.ZZT_AdditionalCode == relatedTariffRateRecord.AdditionalCode) ?? false))
					{
						var refCusApplicabilityToAdd = new RefCusApplicability { ZZT_AdditionalCode = relatedTariffRateRecord.AdditionalCode, ZZT_StartDate = Constants.HsnTariffRateStartDate };
						refCusRate.RefCusApplicabilities = refCusRate.RefCusApplicabilities == null ? new[] { refCusApplicabilityToAdd } : refCusRate.RefCusApplicabilities.Append(refCusApplicabilityToAdd).ToArray();
					}
				}
			}

			return tariffs;
		}

		IEnumerable<DeclarationTariff> GetTariffRateRecords()
		{
			var result = Enumerable.Empty<DeclarationTariff>();
			try
			{
				result = CsvLoader.GetDeclarationTariffs().ToList();
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GetDeclarationTariffs failure, Exception: {ex.GetBaseException().Message}");
			}
			return result;
		}

		public string ErrorMessage => ErrorBuilder.ToString();
		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		internal StringBuilder errorBuilder;
	}
}
