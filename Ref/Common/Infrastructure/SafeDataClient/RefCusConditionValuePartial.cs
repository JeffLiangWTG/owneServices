using System.Collections.Generic;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusConditionValue
	{
		[IgnoreClientProperty]
		public ICollection<RefCusConditionApplicabilityValue> RefCusConditionApplicabilityValues { get; set; } = new HashSet<RefCusConditionApplicabilityValue>();
	}
}
