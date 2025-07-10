using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public class ZATariffsSubSource : ISubSource
	{
		public string SubSourceName => "ZA Tariffs";

		public IEnumerable<Tuple<Type, Type>> GetEntitiesForPurging()
		{
			var result = new List<Tuple<Type, Type>>();
			var parentType = typeof(RefCusTariff);
			result.Add(Tuple.Create(parentType, typeof(RefCusTariffUOM)));
			result.Add(Tuple.Create(parentType, typeof(RefCusTariffAttribute)));
			result.Add(Tuple.Create(parentType, typeof(RefCusTariffRelationship)));
			result.Add(Tuple.Create(parentType, typeof(RefCusRate)));

			result.Add(Tuple.Create(typeof(RefCusRate), typeof(RefCusApplicability)));
			result.Add(Tuple.Create(typeof(RefCusApplicability), typeof(RefCusExcludedTradeGroup)));
			return result;
		}
	}
}
