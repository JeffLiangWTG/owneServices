using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class NomenclaturePlusDailyProducer : NomenclatureProducer
	{
		public NomenclaturePlusDailyProducer(IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider)
		{
			this.dailyTariffUpdatesFileProvider = Argument.NotNull(dailyTariffUpdatesFileProvider, nameof(dailyTariffUpdatesFileProvider));
		}

		public override IEnumerable<IWebFileInfo> LocateWebFiles()
		{
			var monthlyWebFiles = base.LocateWebFiles();
			DownloadDailyFiles(monthlyWebFiles);

			return monthlyWebFiles;
		}

		protected override (List<IRawNomenclatureRecord> nomenclatures, List<IRawDeclarableCodeRecord> declarables) FinalizeNomenclatureAndDeclarableFileParsing(
			List<IRawNomenclatureRecord> rawNomenclatureRecords,
			List<IRawDeclarableCodeRecord> rawDeclarableCodeRecords)
		{
			var dailyNomenclaturesRawRecords = dailyTariffUpdatesFileProvider.NomenclatureDailyRawRecordCollection
				.OrderBy(x => x.FileName)
				.ThenBy(x => x.SequenceNumber)
				.ToList();

			var nomenclatureRecords = rawNomenclatureRecords.ToDictionary(x => GetNomenclatureKey(x));
			var declarableRecords = rawDeclarableCodeRecords.ToDictionary(x => x.TariffHeader);

			var hierarchyPositionCache = new HierarchyPositionCache(rawNomenclatureRecords);
			hierarchyPositionCache.MergeWithDaily(dailyNomenclaturesRawRecords);

			foreach (var dailyRecord in dailyNomenclaturesRawRecords)
			{
				MergeDailyRecord(dailyRecord, nomenclatureRecords, declarableRecords, hierarchyPositionCache);
			}

			new DeclarableCodeRefresher(declarableRecords, dailyNomenclaturesRawRecords, Now)
				.RefreshLeafStatusForDailyUpdateRecords();

			if (dailyTariffUpdatesFileProvider.LastDailyPublishTime is DateTime lastDailyPublishTime)
			{
				PublishTime = lastDailyPublishTime;
				ApplicationConfig.SetPublishTime(lastDailyPublishTime);
			}

			return (nomenclatureRecords.Values.ToList(), declarableRecords.Values.ToList());
		}

		void MergeDailyRecord(
			IRawNomenclatureDailyRecord dailyRecord,
			Dictionary<string, IRawNomenclatureRecord> nomenclatureRecords,
			Dictionary<string, IRawDeclarableCodeRecord> declarableRecords,
			HierarchyPositionCache hierarchyPositionCache)
		{
			switch (dailyRecord.Publish)
			{
				case InsertOperation:
					AddNewDailyNomenclature(dailyRecord, nomenclatureRecords, declarableRecords, hierarchyPositionCache);
					break;

				case UpdateOperation:
					UpdateOrAddNomenclatureAndDeclarable(dailyRecord, nomenclatureRecords, declarableRecords, hierarchyPositionCache);
					break;

				case DeleteOperation:
					DeleteNomenclature(dailyRecord, nomenclatureRecords, declarableRecords);
					break;

				default:
					break;
			}
		}

		void UpdateOrAddNomenclatureAndDeclarable(
			IRawNomenclatureDailyRecord dailyRecord,
			Dictionary<string, IRawNomenclatureRecord> nomenclatureRecords,
			Dictionary<string, IRawDeclarableCodeRecord> declarableRecords,
			HierarchyPositionCache hierarchyPositionCache)
		{
			UpdateOrAddNomenclature(dailyRecord, nomenclatureRecords, hierarchyPositionCache);
			UpdateOrAddDeclarableRecord(dailyRecord, declarableRecords);
		}

		void UpdateOrAddNomenclature(
			IRawNomenclatureDailyRecord dailyRecord,
			Dictionary<string, IRawNomenclatureRecord> nomenclatureRecords,
			HierarchyPositionCache hierarchyPositionCache)
		{
			if (IsNomenclatureRecord(dailyRecord))
			{
				var nomenclatureKey = GetNomenclatureKey(dailyRecord);
				if (!nomenclatureRecords.TryGetValue(nomenclatureKey, out var existingRawNomenclatureRecord))
				{
					var hierarchyPosition = hierarchyPositionCache.TryGet(dailyRecord.TariffHeader, out var cachedHierarchyPosition) ? cachedHierarchyPosition : dailyRecord.HierarchyPosition;

					if (EUNUtils.IsValidHierarchyPosition(hierarchyPosition))
					{
						nomenclatureRecords[nomenclatureKey] = new RawNomenclatureRecord(
							dailyRecord.TariffHeader,
							dailyRecord.StartDate,
							((IRawNomenclatureRecord)dailyRecord).EndDate,
							dailyRecord.Language,
							hierarchyPosition,
							GetDailyUpdateLevel(dailyRecord),
							dailyRecord.Description);
					}
					else
					{
						Console.WriteLine($"WARNING: Skipping daily nomenclature record with invalid hierarchy position. Tariff:'{dailyRecord.TariffHeader}', HierarchyPosition:'{dailyRecord.HierarchyPosition}', Level:'{dailyRecord.Level}', Description:'{dailyRecord.Description}', StartDate:'{dailyRecord.StartDate}', EndDate:'{((IRawNomenclatureRecord)dailyRecord).EndDate}'");
					}
				}
				else
				{
					var hierarchyPosition = hierarchyPositionCache.TryGet(dailyRecord.TariffHeader, out var cachedHierarchyPosition) ? cachedHierarchyPosition : existingRawNomenclatureRecord.HierarchyPosition;

					nomenclatureRecords[nomenclatureKey] = new RawNomenclatureRecord(
						existingRawNomenclatureRecord.TariffHeader,
						dailyRecord.StartDate == EUNUtils.MinDateTime ? existingRawNomenclatureRecord.StartDate : dailyRecord.StartDate,
						((IRawDeclarableCodeRecord)dailyRecord).EndDate == EUNUtils.MinDateTime ? existingRawNomenclatureRecord.EndDate : ((IRawDeclarableCodeRecord)dailyRecord).EndDate,
						existingRawNomenclatureRecord.Language,
						hierarchyPosition,
						existingRawNomenclatureRecord.Level,
						dailyRecord.DeclarableStartDate <= Now ? dailyRecord.Description : existingRawNomenclatureRecord.Description);
				}
			}

			int GetDailyUpdateLevel(IRawNomenclatureDailyRecord record)
			{
				if (record.Level > 0)
				{
					return record.Level;
				}

				// best guess workaround against missing Level value in daily updates
				switch (record.HierarchyPosition)
				{
					case 6:
						return 2;
					case 8:
						return 4;
					case 10:
						return 6;
					default:
						return record.Level;
				};
			}
		}

		static void UpdateOrAddDeclarableRecord(
			IRawNomenclatureDailyRecord dailyRecord,
			Dictionary<string, IRawDeclarableCodeRecord> declarableRecords)
		{
			if (ShouldAddDeclarableRecordFromDaily())
			{
				declarableRecords.Add(
					dailyRecord.TariffHeader,
					new RawDeclarableCodeRecord(
						dailyRecord.TariffHeader,
						dailyRecord.StartDate,
						dailyRecord.DeclarableStartDate,
						isLeaf: IsLeaf,
						((IRawDeclarableCodeRecord)dailyRecord).EndDate));
			}
			else if (ShouldUpdateDeclarableRecordFromDaily())
			{
				var existingDeclarableRecord = declarableRecords[dailyRecord.TariffHeader];

				declarableRecords[dailyRecord.TariffHeader] = new RawDeclarableCodeRecord(
					dailyRecord.TariffHeader,
					dailyRecord.StartDate == EUNUtils.MinDateTime ? existingDeclarableRecord.StartDate : dailyRecord.StartDate,
					dailyRecord.DeclarableStartDate == EUNUtils.MinDateTime ? existingDeclarableRecord.DeclarableStartDate : dailyRecord.DeclarableStartDate,
					isLeaf: IsLeaf,
					((IRawDeclarableCodeRecord)dailyRecord).EndDate);
			}			

			bool ShouldAddDeclarableRecordFromDaily() => IsDeclarableRecord(dailyRecord) && !declarableRecords.ContainsKey(dailyRecord.TariffHeader);
			bool ShouldUpdateDeclarableRecordFromDaily() => string.IsNullOrEmpty(dailyRecord.Language) && declarableRecords.ContainsKey(dailyRecord.TariffHeader);
		}

		void AddNewDailyNomenclature(
			IRawNomenclatureDailyRecord dailyRecord,
			Dictionary<string, IRawNomenclatureRecord> nomenclatureRecords,
			Dictionary<string, IRawDeclarableCodeRecord> declarableRecords,
			HierarchyPositionCache hierarchyPositionCache)
		{
			UpdateOrAddNomenclature(dailyRecord, nomenclatureRecords, hierarchyPositionCache);

			if (IsDeclarableRecord(dailyRecord) && !declarableRecords.ContainsKey(dailyRecord.TariffHeader))
			{
				declarableRecords.Add(dailyRecord.TariffHeader, dailyRecord);
			}
		}

		static void DeleteNomenclature(IRawNomenclatureDailyRecord dailyRecord,
			Dictionary<string, IRawNomenclatureRecord> nomenclatureRecords,
			Dictionary<string, IRawDeclarableCodeRecord> declarableRecords)
		{
			var nomenclatureKey = GetNomenclatureKey(dailyRecord);

			if (nomenclatureRecords.TryGetValue(nomenclatureKey, out var existingRawNomenclatureRecord))
			{
				nomenclatureRecords[nomenclatureKey] = new RawNomenclatureRecord(
					existingRawNomenclatureRecord.TariffHeader,
					existingRawNomenclatureRecord.StartDate,
					existingRawNomenclatureRecord.EndDate,
					existingRawNomenclatureRecord.Language,
					existingRawNomenclatureRecord.HierarchyPosition,
					existingRawNomenclatureRecord.Level,
					"No description available");
			}
		}

		static bool IsDeclarableRecord(IRawNomenclatureDailyRecord dailyRecord) => dailyRecord.Language == EnglishLanguageCode || string.IsNullOrEmpty(dailyRecord.Language);

		static bool IsNomenclatureRecord(IRawNomenclatureDailyRecord dailyRecord) => !string.IsNullOrEmpty(dailyRecord.Language);

		void DownloadDailyFiles(IEnumerable<IWebFileInfo> monthlyWebFiles)
		{
			var lastMonthlyModificationTime = monthlyWebFiles
				.OrderBy(x => x.LastModificationTime)
				.Last()
				.LastModificationTime;

			dailyTariffUpdatesFileProvider.DownloadAndExtractDailyUpdates(Now, lastMonthlyModificationTime);
		}

		static string GetNomenclatureKey(IRawNomenclatureRecord record) => $"{record.TariffHeader}_{record.Language}";

		DateTime Now => (now ?? (now = GetNow())).Value;
		DateTime? now;

		protected virtual DateTime GetNow() => DateTime.Now;

		readonly IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider;

		const string InsertOperation = "INSERT";
		const string UpdateOperation = "UPDATE";
		const string DeleteOperation = "DELETE";
		const string EnglishLanguageCode = "EN";
		const string IsLeaf = "1";

		sealed class HierarchyPositionCache
		{
			public HierarchyPositionCache(List<IRawNomenclatureRecord> rawNomenclatureRecords)
			{
				foreach (var item in rawNomenclatureRecords)
				{
					hierarchyPositionsCache[item.TariffHeader] = item.HierarchyPosition;
				}
			}

			internal void MergeWithDaily(List<IRawNomenclatureDailyRecord> dailyNomenclatureRawRecords)
			{
				foreach (var dailyRawRecord in dailyNomenclatureRawRecords)
				{
					if (dailyRawRecord.HierarchyPosition > 0)
					{
						hierarchyPositionsCache[dailyRawRecord.TariffHeader] = dailyRawRecord.HierarchyPosition;
					}
				}
			}

			internal bool TryGet(string tariffHeader, out int hierarchyPosition) => hierarchyPositionsCache.TryGetValue(tariffHeader, out hierarchyPosition);

			readonly Dictionary<string, int> hierarchyPositionsCache = new Dictionary<string, int>();
		}
	}
}
