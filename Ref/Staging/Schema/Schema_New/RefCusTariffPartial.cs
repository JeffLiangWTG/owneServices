using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public partial class RefCusTariff
	{
		[IgnoreClientProperty]
		public ICollection<RefCusRateApplicability> RefCusRateApplicabilities { get; set; } = new HashSet<RefCusRateApplicability>();

		[IgnoreClientProperty]
		public ICollection<RefCusConditionApplicability> RefCusConditionApplicabilities { get; set; } = new HashSet<RefCusConditionApplicability>();

		[IgnoreClientProperty]
		public ICollection<RefCusRateWithoutApplicability> RefCusRateWithoutApplicabilities { get; set; } = new HashSet<RefCusRateWithoutApplicability>();

		[IgnoreClientProperty]
		public ICollection<RefCusConditionWithoutApplicability> RefCusConditionWithoutApplicabilities { get; set; } = new HashSet<RefCusConditionWithoutApplicability>();

		public void BuildNonPersistentObjects()
		{
			RefCusRateApplicabilities = RefCusRates?.SelectMany(x => x.RefCusApplicabilities.Select(y => new RefCusRateApplicability(x, y))).ToHashSet()
				?? new HashSet<RefCusRateApplicability>();
			RefCusConditionApplicabilities = RefCusConditions?.SelectMany(x => x.RefCusApplicabilities.Select(y => new RefCusConditionApplicability(x, y))).ToHashSet()
				?? new HashSet<RefCusConditionApplicability>();
			RefCusRateWithoutApplicabilities = RefCusRates?.Where(x => x.RefCusApplicabilities.Count == 0).Select(x => new RefCusRateWithoutApplicability(x)).ToHashSet()
				?? new HashSet<RefCusRateWithoutApplicability>();
			RefCusConditionWithoutApplicabilities = RefCusConditions?.Where(x => x.RefCusApplicabilities.Count == 0).Select(x => new RefCusConditionWithoutApplicability(x)).ToHashSet()
				?? new HashSet<RefCusConditionWithoutApplicability>();
		}
	}
}
