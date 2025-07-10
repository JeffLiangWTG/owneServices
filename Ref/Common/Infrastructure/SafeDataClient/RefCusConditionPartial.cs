using Microsoft.OData.Client;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusCondition: INonPersistentBusinessObjectParent
	{
		[IgnoreClientProperty]
		public ICollection<RefCusConditionApplicability> RefCusConditionApplicabilities { get; set; } = new HashSet<RefCusConditionApplicability>();
	}
}
