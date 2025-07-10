using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffEndDateFinder
{
	public class NomenclatureEndDateUpdater
	{
		public NomenclatureEndDateUpdater(ISafeRepository safeRepo)
		{
			this.safeRepo = safeRepo;
			codesWithEndDates = new Dictionary<string, DateTime>();
		}

		readonly Dictionary<string, DateTime> codesWithEndDates;
		readonly ISafeRepository safeRepo;

		public IEnumerable<Tuple<string, DateTime>> GetCodesWithEndDates()
		{
			var maxEndDate = codesWithEndDates.Max(x => x.Value);
			var result = codesWithEndDates.Select(x => Tuple.Create(x.Key, x.Value)).Where(x => x.Item2 < maxEndDate);
			Console.WriteLine($"{result.Count()} tariffCodes is no longer valid on {maxEndDate}");
			return result;
		}

		public void Update(IEnumerable<string> codes, DateTime datetime)
		{
			var endDate = datetime.AddMonths(1);
			endDate = new DateTime(endDate.Year, endDate.Month, 1).AddMinutes(-1);
			foreach (var code in codes)
			{
				if (!codesWithEndDates.ContainsKey(code))
				{
					codesWithEndDates.Add(code, endDate);
				}
				else if (codesWithEndDates[code] < endDate)
				{
					codesWithEndDates[code] = endDate;
				}
			}
		}

		public async Task<IEnumerable<Tuple<string, DateTime>>> CheckCodesInSafeDb(DateTime checkDate)
		{
			var maxEndDate = codesWithEndDates.Max(x => x.Value);
			if (checkDate >= maxEndDate)
			{
				throw new ArgumentException($"CheckDate[{checkDate}] is later than MaxEndDate[{maxEndDate}]. Cannot check for the future.");
			}
			var tariffs = await safeRepo.Get<RefCusTariff>()
				.Expand(x => x.RefCusTariffType)
				.Where(x => x.ZZ1_ZZZ_NKDataGrouping == "EUN")
				.Where(x => x.RefCusTariffType.ZZI_TariffType == "IMP")
				.Where(x => x.ZZ1_StartDate < checkDate && x.ZZ1_EndDate > checkDate).ExecuteAsync();

			var NonCurrentCodesInSafeDb = new Dictionary<string, DateTime>();
			foreach (var tariff in tariffs)
			{
				if (!codesWithEndDates.ContainsKey(tariff.ZZ1_TariffCode))
				{
					if (!NonCurrentCodesInSafeDb.ContainsKey(tariff.ZZ1_TariffCode))
					{
						NonCurrentCodesInSafeDb.Add(tariff.ZZ1_TariffCode, checkDate);
					}
				}
				else if (codesWithEndDates[tariff.ZZ1_TariffCode] < checkDate)
				{
					if (!NonCurrentCodesInSafeDb.ContainsKey(tariff.ZZ1_TariffCode))
					{
						NonCurrentCodesInSafeDb.Add(tariff.ZZ1_TariffCode, codesWithEndDates[tariff.ZZ1_TariffCode]);
					}
				}
			}
			var result = NonCurrentCodesInSafeDb.Select(x => Tuple.Create(x.Key, x.Value));
			Console.WriteLine($"{result.Count()} tariffCodes in SafeDb is no longer valid on {checkDate}");
			return result;
		}
	}
}
