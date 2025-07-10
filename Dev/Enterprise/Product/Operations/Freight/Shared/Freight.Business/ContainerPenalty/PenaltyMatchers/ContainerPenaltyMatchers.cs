using System;
using System.Collections.Generic;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business
{
	public static class ContainerPenaltyMatchers
	{
		public static IReadOnlyList<IContainerPenaltyMatcher> PriorityOrderedPenaltyMatchers
		{
			get
			{
				if (priorityOrderedPenaltyMatchers == null)
				{
					priorityOrderedPenaltyMatchers = new List<IContainerPenaltyMatcher>()
					{
						new CarrierContractPenaltyMatcher(),
						new ClientContractPenaltyMatcher(),
						new OrgContainerPenaltyMatcher(),
						new RegistryPenaltyMatcher()
					};
				}

				return priorityOrderedPenaltyMatchers;
			}
		}

		[ThreadStatic]
		static IReadOnlyList<IContainerPenaltyMatcher> priorityOrderedPenaltyMatchers;
	}
}
