using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public interface ISubSource
	{
		string SubSourceName { get; }
		IEnumerable<Tuple<Type, Type>> GetEntitiesForPurging();
	}
}
