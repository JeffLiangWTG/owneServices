using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUtcOffsetUpdater
{
	public class UnlocoDataHelper
	{
		readonly ISafeRepository safeRepository;
		public UnlocoDataHelper(ISafeRepository safeRepository)
		{
			this.safeRepository = safeRepository;
		}

		public UnlocoDataHelper() { }

		public virtual Dictionary<Guid, string[]> GetRefUNLOCODictionary()
		{
			return GetResult(safeRepository.Get<RefUNLOCO>().Where(x => x.RL_R3 != null).Select(x => new { x.RL_R3, x.RL_Code })).GroupBy(x => x.RL_R3).ToDictionary(x => x.Key ?? Guid.Empty, x => x.Select(y => y.RL_Code).ToArray());
		}

		public virtual Dictionary<Guid, RefTimeZoneSet> GetRefTimezoneSetDictionary()
		{
			return GetResult(safeRepository.Get<RefTimeZoneSet>().Where(x => x.R3_IsActive)).ToDictionary(x => x.R3_PK, x => x);
		}

		protected static IEnumerable<T> GetResult<T>(IQueryable<T> query)
		{
			var result = query.ExecuteAsync()?.Result;
			return result;
		}
	}
}
