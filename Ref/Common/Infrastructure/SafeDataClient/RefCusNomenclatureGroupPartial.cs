using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusNomenclatureGroup
	{
		public RefCusNomenclatureGroup()
		{
			RefCusConditionApplicabilities = new HashSet<RefCusConditionApplicability>();
			RefCusConditionWithoutApplicabilities = new HashSet<RefCusConditionWithoutApplicability>();
		}

		[IgnoreClientProperty]
		public ICollection<RefCusConditionApplicability> RefCusConditionApplicabilities { get; set; }

		[IgnoreClientProperty]
		public ICollection<RefCusConditionWithoutApplicability> RefCusConditionWithoutApplicabilities { get; set; }

		public void BuildNonPersistentObjects(ISafeRepository repo)
		{
			RefCusConditionApplicabilities = NonPersistentObjectHelper.Build(RefCusConditions.Select(x => (x, x.RefCusApplicabilities.ToArray())), repo).Cast<RefCusConditionApplicability>().ToHashSet();
			RefCusConditionWithoutApplicabilities = RefCusConditions.Where(x => x.RefCusApplicabilities.Count == 0).Select(x => new RefCusConditionWithoutApplicability(x, repo)).ToHashSet();
		}
	}
}
