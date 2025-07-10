using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;
namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class OverlappingChecker : IValidationChecker
	{
		IStagingDataWrapper wrapper;
		IOverlappingCalculator calculator;
		IEnumerable<object> safeObjs;
		DateTimeRange wrapperEffectiveDateRange;
		IEnumerable<Tuple<DateTimeRange, Guid>> historicalDateRanges;

		public OverlappingChecker(IStagingDataWrapper wrapper, IOverlappingCalculator calculator, IEnumerable<object> safeObjs, DateTimeRange wrapperEffectiveDateRange, IEnumerable<Tuple<DateTimeRange, Guid>> historicalDateRanges)
		{
			this.wrapper = wrapper;
			this.calculator = calculator;
			this.safeObjs = safeObjs;
			this.wrapperEffectiveDateRange = wrapperEffectiveDateRange;
			this.historicalDateRanges = historicalDateRanges;
		}

		public bool IsValid()
		{
			var sortedDateRanges = historicalDateRanges.OrderBy(x => x.Item1.StartDate).ToList();
			if (calculator.IsOverlapped(sortedDateRanges.Select(x => x.Item1)))
			{
				var safeHandledResult = HandleSafeNonPersistentObject();
				var errorMessage = $@"Overlapping of existing data in SafeDB is identified.
{safeHandledResult.OriginalType.Name} has overlapping of Effective DateRange;
Existing DateRanges in SafeDB:
{string.Join("\r\n", sortedDateRanges.Select(x => $"[{x.Item1.StartDate.GetFormatString()}~{x.Item1.EndDate.GetFormatString()}, {GetSafeObjectOriginalPK(safeHandledResult.PKMap, x.Item2)}]"))}";
				throw new RefDataProcessingException(errorMessage, ErrorCodes.OverlappingDateRange);
			}

			if (calculator.IsOverlapped(sortedDateRanges.Select(x => x.Item1).Concat(new[] { wrapperEffectiveDateRange })))
			{
				var safeHandledResult = HandleSafeNonPersistentObject();
				var safeObjTablPrefix = safeHandledResult.OriginalType.GetTablePrefix();
				var errorMessage = $@"Overlapping is caused by new data from XML.
{safeHandledResult.OriginalType.Name} has overlapping of Effective DateRange;
New data in StageDB:
[{wrapperEffectiveDateRange.StartDate.GetFormatString()}~{wrapperEffectiveDateRange.EndDate.GetFormatString()}, {GetStagingObjectOriginalPK($"{safeObjTablPrefix}_PK")}]
{GetMessageOfSafeDateRanges(sortedDateRanges, safeHandledResult.PKMap)}";
				throw new RefDataProcessingException(errorMessage, ErrorCodes.OverlappingDateRange);
			}
			return true;
		}

		Guid GetStagingObjectOriginalPK(string columnPK)
		{
			var stagingTypeName = wrapper.GetStagingTypeName();
			if (stagingTypeName == nameof(Stage.RefCusRateApplicability)
				|| stagingTypeName == nameof(Stage.RefCusConditionApplicability))
			{
				return wrapper.GetWrapperOriginalPK();
			}
			return wrapper.GetWrapperValue<Guid>(columnPK);
		}

		(Dictionary<Guid, Guid> PKMap, Type OriginalType) HandleSafeNonPersistentObject()
		{
			var pkMap = new Dictionary<Guid, Guid>();
			Type originalType = safeObjs.First().GetEntityType();
			foreach (var item in safeObjs)
			{
				if (item is Safe.RefCusRateApplicability rateApp)
				{
					pkMap[rateApp.S01_PK] = rateApp.RefCusApplicability is null ? Guid.Empty : rateApp.RefCusApplicability.ZZT_PK;
					originalType = typeof(Safe.RefCusApplicability);
				}
				else if (item is Safe.RefCusConditionApplicability condApp)
				{
					pkMap[condApp.S07_PK] = condApp.RefCusApplicability is null ? Guid.Empty : condApp.RefCusApplicability.ZZT_PK;
					originalType = typeof(Safe.RefCusApplicability);
				}
			}
			return (pkMap, originalType);
		}

		static string GetMessageOfSafeDateRanges(IEnumerable<Tuple<DateTimeRange, Guid>> sortedDateRanges, Dictionary<Guid, Guid> pkMap)
		{
			var strBuilder1 = new StringBuilder();
			var strBuilder2 = new StringBuilder();
			foreach (var dateRange in sortedDateRanges)
			{
				var safeOriginalPK = GetSafeObjectOriginalPK(pkMap, dateRange.Item2);
				if (safeOriginalPK == Guid.Empty)
				{
					strBuilder1.AppendLine(CultureInfo.InvariantCulture, $"[{dateRange.Item1.StartDate.GetFormatString()}~{dateRange.Item1.EndDate.GetFormatString()}]");
				}
				else
				{
					strBuilder2.AppendLine(CultureInfo.InvariantCulture, $"[{dateRange.Item1.StartDate.GetFormatString()}~{dateRange.Item1.EndDate.GetFormatString()}, {safeOriginalPK}]");
				}
			}
			var resultBuilder = new StringBuilder();
			if (strBuilder1.Length > 0)
			{
				resultBuilder.AppendLine("Data processed but not yet committed to SafeDB:");
				resultBuilder.Append(strBuilder1);
			}
			if (strBuilder2.Length > 0)
			{
				resultBuilder.AppendLine("Existing DateRanges in SafeDB:");
				resultBuilder.Append(strBuilder2);
			}
			return resultBuilder.ToString().TrimEnd();
		}

		static Guid GetSafeObjectOriginalPK(Dictionary<Guid, Guid> pkMap, Guid pkKey)
		{
			return pkMap.TryGetValue(pkKey, out var originalPK) ? originalPK : pkKey;
		}
	}
}
