using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public static class SubSourceHelper
	{
		public static IEnumerable<ISubSource> AvailableSubSources => new ISubSource[] { new ZATariffsSubSource() };
	}
}
