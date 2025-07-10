using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public class RegulationHelper
	{
		public static string GetUniqueKey(IRegulationData data)
		{
			return $"{data.RegulationId}-{data.RegulationRoleTypeId}";
		}

		public RegulationHelper(List<ITariffModel> referenceData, IDateTimeProvider dateTimeProvider, StringBuilder errorCollector)
		{
			if (referenceData != null)
			{
				BaseRegulations = ConvertToDictionary(referenceData.OfType<BaseRegulation>());
				ModificationRegulations = ConvertToDictionary(referenceData.OfType<ModificationRegulation>());
			}

			DateTimeProvider = dateTimeProvider;
			ErrorCollector = errorCollector;
		}

		public bool IsRegulationActive(string uniqueId)
		{
			if (BaseRegulations.ContainsKey(uniqueId))
			{
				var reg = BaseRegulations[uniqueId];

				return IsInDateRange(reg.StartDate, reg.EndDate, reg.EffectiveEndDate);
			}

			if (ModificationRegulations.ContainsKey(uniqueId))
			{
				var reg = ModificationRegulations[uniqueId];

				return IsInDateRange(reg.StartDate, reg.EndDate, reg.EffectiveEndDate);
			}

			ErrorCollector.Append(CultureInfo.InvariantCulture, $"Regulation {uniqueId} not found");

			return false;
		}

		bool IsInDateRange(DateTime? startDate, DateTime? endDate, DateTime? effEndDate)
		{
			var dt = DateTimeProvider.UTCDateTime;
			var sd = AdjustForFutureDatedRegulations(CommonHelper.CalcMinDate(startDate));
			var ed = CommonHelper.CalcMaxDate(effEndDate ?? endDate);

			return (dt >= sd && dt <= ed);
		}

		static DateTime AdjustForFutureDatedRegulations(DateTime startDate) => startDate.AddDays(-90);

		static Dictionary<string, T> ConvertToDictionary<T>(IEnumerable<T> data) where T : IRegulationData
		{
			var dict = new Dictionary<string, T>();

			foreach (var item in data)
			{
				var key = GetUniqueKey(item);

				if (dict.ContainsKey(key))
				{
					dict[key] = item;
				}
				else
				{
					dict.Add(key, item);
				}
			}

			return dict;
		}

		readonly IDateTimeProvider DateTimeProvider;
		StringBuilder ErrorCollector;

		protected Dictionary<string, BaseRegulation> BaseRegulations { get; private set; }
		protected Dictionary<string, ModificationRegulation> ModificationRegulations { get; private set; }
	}
}
