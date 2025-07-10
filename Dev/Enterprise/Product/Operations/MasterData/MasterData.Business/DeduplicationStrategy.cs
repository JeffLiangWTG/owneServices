using System;
using Enterprise.MasterData.Common;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Business
{
	public class OrganisationDeduplicationStrategy : IDeduplicationStrategy
	{
		public Func<int> MaximumPKs => () => OrganisationsDataRegistry.Instance.MaximumPotentialTargets.Value;
	}

	public class PersonDeduplicationStrategy : IDeduplicationStrategy
	{
		public Func<int> MaximumPKs => () => SystemDataRegistry.Instance.PersonsMaximumPotentialTargets.Value;
	}

	public class AdminPanelDeduplicationStrategy : IDeduplicationStrategy
	{
		public Func<int> MaximumPKs => () => int.MaxValue;
	}
}
