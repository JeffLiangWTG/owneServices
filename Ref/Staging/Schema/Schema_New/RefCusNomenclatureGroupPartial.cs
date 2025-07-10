using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public partial class RefCusNomenclatureGroup
	{
		[IgnoreClientProperty]
		public ICollection<RefCusConditionApplicability> RefCusConditionApplicabilities { get; set; } = new HashSet<RefCusConditionApplicability>();

		[IgnoreClientProperty]
		public ICollection<RefCusConditionWithoutApplicability> RefCusConditionWithoutApplicabilities { get; set; } = new HashSet<RefCusConditionWithoutApplicability>();

		public void BuildNonPersistentObjects()
		{
			RefCusConditionApplicabilities = RefCusConditions?.SelectMany(x => x.RefCusApplicabilities.Select(y => new RefCusConditionApplicability(x, y))).ToHashSet()
				?? new HashSet<RefCusConditionApplicability>();
			RefCusConditionWithoutApplicabilities = RefCusConditions?.Where(x => x.RefCusApplicabilities.Count == 0).Select(x => new RefCusConditionWithoutApplicability(x)).ToHashSet()
				?? new HashSet<RefCusConditionWithoutApplicability>();
		}
	}
}
