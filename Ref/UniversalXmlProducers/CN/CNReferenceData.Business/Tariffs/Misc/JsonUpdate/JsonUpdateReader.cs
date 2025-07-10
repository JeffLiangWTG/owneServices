using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class JsonUpdateReader
	{
		public JsonUpdateReader(bool checkUpdates, DateTime? date, int? daysBack, EChinaAPIProxy proxy = null)
		{
			CheckUpdates = checkUpdates;

			if (!CheckUpdates && date.HasValue)
			{
				StartDate = date.Value.AddDays(-1 * (daysBack ?? 0));
				EndDate = date.Value;
			}
			else
			{
				var now = GlobalOption.Instance.Now;
				Trace = TariffProducerTrace.ReadFromLocalFile();
				StartDate = CalculateStartDate(Trace, now);
				EndDate = now.Date;
			}

			Proxy = proxy ?? new EChinaAPIProxy(GlobalOption.Instance.Setting, Log);
		}

		readonly bool CheckUpdates;

		readonly ILog Log = GlobalOption.Instance.Log;

		readonly EChinaAPIProxy Proxy;

		readonly TariffProducerTrace Trace;

		static int PageSize => 20;

		static int MaxConcurrency => 10;

		static int MaxCountofRetries => 3;

		DateTime StartDate { get; }
		DateTime EndDate { get; }

		#region GetUpdatedTariffsData

		public List<HSData> GetUpdatedTariffsData(int estimatedCountofUpdates)
		{
			var pageNumber = 1;
			var tariffs = new List<HSData>();

			while (true)
			{
				var batchSize = (estimatedCountofUpdates - tariffs.Count <= PageSize) ? 1 : MaxConcurrency;
				var updatesResponses = GetUpdatedTariffsData(GetPageNumbers(pageNumber, batchSize));

				pageNumber += batchSize;

				var validResponses = updatesResponses.Where(x => x.IsSuccess());

				Log.Info($"Count of updated tariffs: {string.Join(" + ", validResponses.Select(x => x.CountOfUpdates.ToString(CultureInfo.InvariantCulture)).ToArray())} = {validResponses.Sum(x => x.CountOfUpdates)}");

				foreach (var response in validResponses)
				{
					if (response.RESULT_DATA_LIST?.HS_TAX != null)
					{
						foreach (var tariffData in response.RESULT_DATA_LIST.HS_TAX)
						{
							if (tariffData.DIGIT_MARK == 10)
							{
								tariffs.Add(tariffData);

								Log.Debug($"Updated Tariff: {LogForTariff(tariffData)}");
							}
							else
							{
								if (tariffData.DIGIT_MARK < 10 && (tariffData.GEN_LIST?.Count ?? 0) > 0)
								{
									Log.Warning($"Page Number: {response.PageNumber}, {LogForTariff(tariffData)}, but has rates.");
								}

								if (tariffData.DIGIT_MARK == 8)
								{
									tariffs.Add(tariffData);

									Log.Debug($"Auxiliary Tariff: {LogForTariff(tariffData)}");
								}
								else
								{
									Log.Info($"Skip Tariff: {LogForTariff(tariffData)}.");
								}
							}
						}
					}
				}

				if ((updatesResponses.LastOrDefault()?.CountOfUpdates ?? 0) == 0)
				{
					break;
				}
			}

			return tariffs;
		}

		static string LogForTariff(HSData tariffData)
		{
			return $"Tariff: {tariffData.ID} - {tariffData.HS_CODE} - BOOK: {tariffData.HS_BOOK} DIGIT_MARK: {tariffData.DIGIT_MARK}";
		}

		static IEnumerable<int> GetPageNumbers(int startPageNumber, int batchSize)
		{
			for (int i = 0; i < batchSize; i++)
			{
				yield return startPageNumber + i;
			}
		}

		List<GetUpdatesResponse> GetUpdatedTariffsData(IEnumerable<int> pageNumbers, int numberOfRetries = 0)
		{
			var tasks = new List<Task<string>>();
			var updatesResponses = new List<GetUpdatesResponse>();

			foreach (var pageNumber in pageNumbers)
			{
				tasks.Add(Proxy.GetUpdateDataTask(StartDate, EndDate, pageNumber, PageSize));
			}

			Task.WaitAll(tasks.ToArray());

			var failedPageNumbers = new List<int>();

			for (int i = 0; i < pageNumbers.Count(); i++)
			{
				var result = tasks[i].Result;
				var pageNumber = pageNumbers.ElementAt(i);

				if (string.IsNullOrEmpty(result))
				{
					failedPageNumbers.Add(pageNumber);
				}
				else
				{
					var responseFilePath = GlobalOption.Instance.Setting.GetFullResponseFileName($@"{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}_{pageNumber:00}_Response.json");
					if (!string.IsNullOrEmpty(responseFilePath))
					{
						File.WriteAllText(responseFilePath, result);
						GlobalOption.Instance.OutputFiles.Add(responseFilePath);
					}

					var updatesResponse = JsonConvert.DeserializeObject<GetUpdatesResponse>(result);
					updatesResponse.PageNumber = pageNumber;

					if (!updatesResponse.IsSuccess())
					{
						failedPageNumbers.Add(pageNumber);
						Log.Error($"Page Number: {pageNumber} failed, Result Status: {updatesResponse.StateInfo()}");
					}
					else
					{
						updatesResponses.Add(updatesResponse);
					}
				}
			}

			if (failedPageNumbers.Any())
			{
				if (numberOfRetries < MaxCountofRetries)
				{
					Log.Warning($"Retry to get updates from page numbers: {string.Join(",", failedPageNumbers)} failed, retry({++numberOfRetries})...");
					updatesResponses.AddRange(GetUpdatedTariffsData(failedPageNumbers, numberOfRetries));
				}
				else
				{
					Log.Error($"Fail to get updates from page numbers: {string.Join(",", failedPageNumbers)}.");
				}
			}

			return updatesResponses;
		}

		#endregion

		#region Trace

		static DateTime CalculateStartDate(TariffProducerTrace trace, DateTime now)
		{
			DateTime startDate;

			var nextDayOfGetUpdates = trace.LatestGetUpdatesTime.Date.AddDays(1);

			if (trace.CountOfUpdates > 0)
			{
				startDate = trace.UpdatesDetectedTime.Date > nextDayOfGetUpdates ? trace.UpdatesDetectedTime.Date : nextDayOfGetUpdates;
			}
			else
			{
				if (trace.LatestCheckForUpdatesTime.Date < now.Date)
				{
					startDate = nextDayOfGetUpdates;
				}
				else
				{
					startDate = now.Date;
				}
			}

			if (startDate > now.Date)
			{
				startDate = now.Date;
			}

			var minDate = now.Date.AddDays(-10);
			if (startDate < minDate)
			{
				startDate = minDate;
			}
			return startDate;
		}

		int UpdateTraceAfterCheckUpdates(int countOfUpdates, DateTime checkTime)
		{
			if (Trace != null)
			{
				if (countOfUpdates > 0 && Trace.CountOfUpdates == 0)
				{
					Trace.UpdatesDetectedTime = checkTime;
				}
				Trace.CountOfUpdates += countOfUpdates;

				Trace.LatestCheckForUpdatesTime = checkTime;
				Trace.SaveToLocalFile();

				countOfUpdates = Trace.CountOfUpdates;
			}

			return countOfUpdates;
		}

		void UpdateTraceAfterPopulateUpdates(DateTime getUpdatesTime, bool hasUpdates)
		{
			if (Trace != null)
			{
				if (!hasUpdates)
				{
					getUpdatesTime = getUpdatesTime.AddDays(-1);
				}
				if (!CheckUpdates)
				{
					if (hasUpdates)
					{
						Trace.UpdatesDetectedTime = getUpdatesTime;
					}
					Trace.LatestCheckForUpdatesTime = getUpdatesTime;
				}
				Trace.LatestGetUpdatesTime = getUpdatesTime;
				Trace.CountOfUpdates = 0;
				Trace.SaveToLocalFile();
			}
		}

		#endregion

		#region Main Task

		IEnumerable<HSData> AfterCheckUpdatedCount(Task<int> checkUpdateTask, DateTime now)
		{
			IEnumerable<HSData> result;

			var updateCount = checkUpdateTask.Result;
			if (CheckUpdates)
			{
				updateCount = UpdateTraceAfterCheckUpdates(updateCount, now);
			}

			if (updateCount > 0)
			{
				result = GetUpdatedTariffsData(updateCount);
			}
			else
			{
				result = new HSData[0];
			}
			return result;
		}

		async Task<int> GetSkipCheckUpdatesTask() => await Task.Run(() =>
		{
			Log.Info("Skip Checking Updates.");
			return 30;
		});

		public Task GetMainTask()
		{
			return new Task(() =>
			{
				var now = GlobalOption.Instance.Now;

				var getUpdateCountTask = CheckUpdates ? Proxy.GetUpdatedCountTask() : GetSkipCheckUpdatesTask();
				var getUpdatesTask = getUpdateCountTask.ContinueWith(task => AfterCheckUpdatedCount(task, now));

				var updates = getUpdatesTask.GetAwaiter().GetResult();
				var parser = new JsonTariffParser(updates.ToArray());
				var tariffs = parser.GetTariffList();
				if (tariffs.Any())
				{
					var tariffWriter = new CNRefCusTariffUniversalXMLWriter(now, true);
					tariffWriter.Write(tariffs, $@"{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}_CNRefCusTariff_{now:yyyyMMddHHmmss}.xml");

					var codeWriter = new CNRefCusCodeListUniversalXMLWriter(now, parser.AdditionalElementHelper);
					codeWriter.Write($@"{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}_CNRefCusCodeList_{now:yyyyMMddHHmmss}.xml");
				}

				UpdateTraceAfterPopulateUpdates(now, updates.Any());
			});
		}

		#endregion
	}
}
