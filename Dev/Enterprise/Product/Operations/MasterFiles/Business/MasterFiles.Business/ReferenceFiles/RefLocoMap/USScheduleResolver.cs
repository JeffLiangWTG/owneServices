using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public enum Schedule { D, K }

	public static class USScheduleResolver
	{
		public static List<RefLocoMap> GetMatchesForSchedule(Schedule schedule, ZString unLoco, ZString transportMode, BusinessObjectFactory factory)
		{
			var result = new List<RefLocoMap>();
			var port = new RefUNLOCO.Loader(factory).Load(unLoco);
			if (port != null)
			{
				if (schedule == Schedule.D)
				{
					if (transportMode == Enterprise.Core.Constants.TransportModes.Air || transportMode == Enterprise.Core.Constants.TransportModes.Sea)
					{
						result = GetMatchingRefLocoMaps(port, new[] { transportMode.ToString() }, true);
					}

					if (!result.Any())
					{
						result = GetMatchingRefLocoMaps(port, new[] { USLocoMapSystemUsageList.Codes.All });
					}

					if (!result.Any())
					{
						result = GetMatchingRefLocoMaps(port, new[] { USLocoMapSystemUsageList.Codes.SCD });
					}
				}
				else
				{
					result = GetMatchingRefLocoMaps(port, new[] { USLocoMapSystemUsageList.Codes.SCK });
				}
			}

			return result;
		}

		static List<RefLocoMap> GetMatchingRefLocoMaps(RefUNLOCO port, string[] systemUsage, bool ignoreIsSystem = false)
		{
			var result = GetMatchingRefLocoMaps(port, systemUsage, false, ignoreIsSystem);
			if (!result.Any())
			{
				result = GetMatchingRefLocoMaps(port, systemUsage, true, ignoreIsSystem);
			}

			return result;
		}

		static List<RefLocoMap> GetMatchingRefLocoMaps(RefUNLOCO port, string[] systemUsage, bool isSystem, bool ignoreIsSystem)
		{
			var loclMapQuery = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.UnitedStates);
			loclMapQuery.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);
			loclMapQuery.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, SQLComparisonOperator.IsNotBlank, ZString.Empty);
			if (!ignoreIsSystem)
			{
				loclMapQuery.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);
			}

			return new List<RefLocoMap>(port.RefLocoMaps.Find(loclMapQuery));
		}

		public static ZString MatchingUNLOCO(ZString schedulePort, BusinessObjectFactory factory)
		{
			var locoMapQuery = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.UnitedStates);
			locoMapQuery.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, schedulePort);

			string[] systemUsage = new string[]
			{
				USLocoMapSystemUsageList.Codes.SCD,
				USLocoMapSystemUsageList.Codes.SCK,
				USLocoMapSystemUsageList.Codes.All,
				USLocoMapSystemUsageList.Codes.Sea,
				USLocoMapSystemUsageList.Codes.Air
			};

			locoMapQuery.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);

			var locoMaps = factory.Load<RefLocoMap>(locoMapQuery);
			locoMaps = locoMaps.OrderBy(x => x.RY_RL_NKLocoPort).ToArray();
			return locoMaps.Length >= 1 ? locoMaps[0].RY_RL_NKLocoPort : ZString.Empty;
		}

		public static ZString GetScheduleCode(Schedule schedule, ZString unLoco, ZString transportMode, BusinessObjectFactory factory)
		{
			var matches = GetMatchesForSchedule(schedule, unLoco, transportMode, factory);
			return matches.Count == 1 ? matches[0].RY_LocalPortCode : ZString.Empty;
		}
	}
}
