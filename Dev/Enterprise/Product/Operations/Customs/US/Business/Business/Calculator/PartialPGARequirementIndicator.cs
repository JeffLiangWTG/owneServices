using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.US.Business
{
	public static class PartialPGARequirementIndicator
	{
		public static bool HasRequirement(IEnumerable<USCTariff> tariffs, Func<USCTariff, bool> hasRequirement)
		{
			return tariffs.Where(x => x != null).Any(x => hasRequirement(x));
		}
	}
}
