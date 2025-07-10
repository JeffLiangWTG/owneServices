using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusTariff
	{
		public RefCusTariff()
		{
			RefCusRateApplicabilities = new HashSet<RefCusRateApplicability>();
			RefCusConditionApplicabilities = new HashSet<RefCusConditionApplicability>();
			RefCusRateWithoutApplicabilities = new HashSet<RefCusRateWithoutApplicability>();
			RefCusConditionWithoutApplicabilities = new HashSet<RefCusConditionWithoutApplicability>();
		}

		[IgnoreClientProperty]
		public ICollection<RefCusRateApplicability> RefCusRateApplicabilities { get; set; }

		[IgnoreClientProperty]
		public ICollection<RefCusConditionApplicability> RefCusConditionApplicabilities { get; set; }

		[IgnoreClientProperty]
		public ICollection<RefCusRateWithoutApplicability> RefCusRateWithoutApplicabilities { get; set; }

		[IgnoreClientProperty]
		public ICollection<RefCusConditionWithoutApplicability> RefCusConditionWithoutApplicabilities { get; set; }

		public void BuildNonPersistentObjects(ISafeRepository repo)
		{
			RefCusRateApplicabilities = NonPersistentObjectHelper.Build(RefCusRates.Select(x => (x, x.RefCusApplicabilities.ToArray())), repo).Cast<RefCusRateApplicability>().ToHashSet();
			RefCusConditionApplicabilities = NonPersistentObjectHelper.Build(RefCusConditions.Select(x => (x, x.RefCusApplicabilities.ToArray())), repo).Cast<RefCusConditionApplicability>().ToHashSet();
			RefCusRateWithoutApplicabilities = RefCusRates.Where(x => x.RefCusApplicabilities.Count == 0).Select(x => new RefCusRateWithoutApplicability(x, repo)).ToHashSet();
			RefCusConditionWithoutApplicabilities = RefCusConditions.Where(x => x.RefCusApplicabilities.Count == 0).Select(x => new RefCusConditionWithoutApplicability(x, repo)).ToHashSet();
		}
	}
}
