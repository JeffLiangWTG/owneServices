using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using static CargoWise.RefDbRepo.KRReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class RefDataEntityLoader
	{
		public RefDataEntityLoader(ISafeRepository safeRepo)
		{
			Argument.NotNull(safeRepo, nameof(safeRepo));
			SafeRepository = safeRepo;
		}
		ISafeRepository SafeRepository { get; set; }

		IEnumerable<RefCusNomenclatureGroup> GetNomenclatureData(string dataGroup)
		{
			return SafeRepository.Get<RefCusNomenclatureGroup>()?.Where(t => t.ZZ5_ZZZ_NKDataGrouping == dataGroup).ExecuteAsync().Result.ToList();
		}
		IEnumerable<RefCusNomenclatureGroup> KRNomenclatures => krNomenclatures ?? (krNomenclatures = GetNomenclatureData(CountryCodes.KoreaSouth));
		IEnumerable<RefCusNomenclatureGroup> krNomenclatures;

		public IEnumerable<RefCusNomenclatureGroup> GetNomenclatureData(string dataGroup, string strStartWith, DateTime publicationDate)
		{
			return SafeRepository.Get<RefCusNomenclatureGroup>()?.Where(t => t.ZZ5_ZZZ_NKDataGrouping == dataGroup &&
																			t.ZZ5_StartDate <= publicationDate && t.ZZ5_EndDate >= publicationDate &&
																			t.ZZ5_Value.StartsWith(strStartWith)).ExecuteAsync().Result.ToList();
		}

		public IEnumerable<RefCusTariff> GetTariffData(string dataGroup, string strStartWith, DateTime publicationDate)
		{
			return SafeRepository.Get<RefCusTariff>()?.Where(t => t.ZZ1_ZZZ_NKDataGrouping == dataGroup &&
																t.ZZ1_StartDate <= publicationDate && t.ZZ1_EndDate >= publicationDate &&
																t.ZZ1_TariffCode.StartsWith(strStartWith)).ExecuteAsync().Result.ToList();
		}

		public string GenerateCompositeKey(Common.UniversalXmlWriter.EntityType.RefCusTariff tariff)
		{
			var result = string.Empty;
			if (tariff != null)
			{
				var tariffCode = tariff.ZZ1_TariffCode;
				var endDate = tariff.ZZ1_EndDate == DateTime.MinValue ? MaxEndDate : tariff.ZZ1_EndDate;
				if (!string.IsNullOrEmpty(tariffCode) && !exceptionalTariffs.Contains(tariffCode))
				{
					var nomenclatureLen = 6;
					var nomenclature = GetWCOCopiedParentNomenclature(tariffCode, endDate, nomenclatureLen);
					if (nomenclature == null)
					{
						nomenclatureLen = 4;
						nomenclature = GetWCOCopiedParentNomenclature(tariffCode, endDate, nomenclatureLen);
					}
					var compositeKeyBuilder = new StringBuilder(nomenclature.ZZ5_CompositeKey);
					for (var i = nomenclatureLen; i < Constants.TariffLength; i++)
					{
						compositeKeyBuilder.Append(CultureInfo.InvariantCulture, $".{tariffCode.Substring(i, 1)}");
					}
					result = compositeKeyBuilder.ToString();
				}
			}
			return result;
		}
		List<string> exceptionalTariffs = new List<string>() { "2424000000" };

		public string GenerateCompositeKey(Common.UniversalXmlWriter.EntityType.RefCusNomenclatureGroup nomenclature)
		{
			var result = string.Empty;
			if (nomenclature != null)
			{
				var nomenclatureValue = nomenclature.ZZ5_Value;
				var endDate = nomenclature.ZZ5_EndDate == DateTime.MinValue ? MaxEndDate : nomenclature.ZZ5_EndDate;
				if (!string.IsNullOrEmpty(nomenclatureValue))
				{
					var parentNomenclatureLen = 6;
					var parentNomenclature = GetWCOCopiedParentNomenclature(nomenclatureValue, endDate, parentNomenclatureLen);
					if (parentNomenclature == null)
					{
						parentNomenclatureLen = 4;
						parentNomenclature = GetWCOCopiedParentNomenclature(nomenclatureValue, endDate, parentNomenclatureLen);
					}
					var compositeKeyBuilder = new StringBuilder(parentNomenclature.ZZ5_CompositeKey);
					for (var i = parentNomenclatureLen; i < nomenclatureValue.Length; i++)
					{
						compositeKeyBuilder.Append(CultureInfo.InvariantCulture, $".{nomenclatureValue.Substring(i, 1)}");
					}
					result = compositeKeyBuilder.ToString();
				}
			}
			return result;
		}

		RefCusNomenclatureGroup GetWCOCopiedParentNomenclature(string nomenclatureOrTariffCode, DateTime endDate, int nomenclatureLen)
		{
			return KRNomenclatures.SingleOrDefault(x => x.ZZ5_Value == nomenclatureOrTariffCode.Substring(0, nomenclatureLen) &&
														x.ZZ5_StartDate <= endDate && x.ZZ5_EndDate >= endDate);
		}

		readonly DateTime MaxEndDate = new DateTime(2079, 06, 06, 23, 59, 00);
	}
}
