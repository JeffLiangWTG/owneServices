using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public sealed class DeclarableCodeRefresher
	{
		public DeclarableCodeRefresher(
			Dictionary<string, IRawDeclarableCodeRecord> declarableRecords,
			ICollection<IRawNomenclatureDailyRecord> dailyNomenclaturesRawRecords,
			DateTime now)
		{
			this.declarableRecords = declarableRecords;
			this.dailyNomenclaturesRawRecords = dailyNomenclaturesRawRecords;
			this.now = now;
		}

		public void RefreshLeafStatusForDailyUpdateRecords()
		{
			NotLeafTariffs.ForEach(x => UpdateLeafStatus(x, isLeaf: false));

			var leafTariffs = dailyNomenclaturesRawRecords.Except(NotLeafTariffs);
			for (var i = 2; i < 6; i++)
			{
				var tariffs = GetDistinctTariffs(i, leafTariffs);

				foreach (var fullTariff in tariffs)
				{
					var parentTariff = GetBranchTariff(fullTariff);

					if (!IsValidDeclarableRecord(parentTariff))
					{
						continue;
					}

					var parentDeclarableCodeRecord = declarableRecords[parentTariff];
					var childDeclarableCodeRecords = GetChildDeclarableCodeRecords(fullTariff, parentTariff, notLeafTariffs);

					var isLeaf = !HasValidChildDeclarableCodeRecords(childDeclarableCodeRecords);

					UpdateLeafStatusIfDifferent(parentDeclarableCodeRecord, isLeaf);
				}
			}
		}

		static IEnumerable<string> GetDistinctTariffs(int i, IEnumerable<IRawNomenclatureDailyRecord> leafTariffs)
		{
			return leafTariffs
				.Select(x => x.TariffHeader.Substring(0, 12 - 2 * i))
				.Distinct()
				.ToList();
		}

		static string GetBranchTariff(string tariff)
			=> tariff.PadRight(10, '0') + " 80";

		bool IsValidDeclarableRecord(string branchTariff)
		{
			return declarableRecords.ContainsKey(branchTariff) &&
				 IsDeclarableCodeValid(declarableRecords[branchTariff]);
		}

		IEnumerable<IRawDeclarableCodeRecord> GetChildDeclarableCodeRecords(string tariff, string branchTariff, IEnumerable<IRawDeclarableCodeRecord> notLeafTariffs)
		{
			return declarableRecords.Values
				.Where(x => x.TariffHeader.StartsWith(tariff, StringComparison.InvariantCultureIgnoreCase)
							&& x.TariffHeader != branchTariff)
				.ToList();
		}

		bool HasValidChildDeclarableCodeRecords(IEnumerable<IRawDeclarableCodeRecord> childDeclarableCodeRecords)
		{
			return childDeclarableCodeRecords.Any(x => IsDeclarableCodeValid(x));
		}

		void UpdateLeafStatusIfDifferent(IRawDeclarableCodeRecord branchDeclarableCodeRecord, bool isLeaf)
		{
			if (branchDeclarableCodeRecord.IsLeaf != isLeaf)
			{
				UpdateLeafStatus(branchDeclarableCodeRecord, isLeaf);
			}
		}

		void UpdateLeafStatus(IRawDeclarableCodeRecord branchDeclarableCodeRecord, bool isLeaf)
		{
			var tariffCode = branchDeclarableCodeRecord.TariffHeader;

			declarableRecords[tariffCode] = new RawDeclarableCodeRecord(
				tariffCode,
				branchDeclarableCodeRecord.StartDate,
				branchDeclarableCodeRecord.DeclarableStartDate,
				isLeaf ? "1" : "0",
				branchDeclarableCodeRecord.EndDate);
		}

		bool IsDeclarableCodeValid(IRawDeclarableCodeRecord record)
		{
			var lastDailyUpdate = dailyNomenclaturesRawRecords
				.Where(x => x.TariffHeader == record.TariffHeader)
				.OrderBy(x => x.FileName)
				.ThenBy(x => x.SequenceNumber)
				.LastOrDefault();

			if (lastDailyUpdate != null)
			{
				return lastDailyUpdate.StartDate <= now && ((IRawDeclarableCodeRecord)lastDailyUpdate).EndDate > now;
			}

			return record.StartDate <= now && record.EndDate > now;
		}

		List<IRawNomenclatureDailyRecord> NotLeafTariffs => notLeafTariffs ?? (notLeafTariffs = dailyNomenclaturesRawRecords.Where(x => !x.TariffHeader.EndsWith("80", StringComparison.InvariantCulture)).ToList());
		List<IRawNomenclatureDailyRecord> notLeafTariffs;

		readonly Dictionary<string, IRawDeclarableCodeRecord> declarableRecords;
		readonly ICollection<IRawNomenclatureDailyRecord> dailyNomenclaturesRawRecords;
		readonly DateTime now;
	}
}
