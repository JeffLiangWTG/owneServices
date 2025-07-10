using System.Collections.Generic;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusConditionLanguage
	{
		[IgnoreClientProperty]
		public ICollection<RefCusConditionApplicabilityLanguage> RefCusConditionApplicabilityLanguages { get; set; } = new HashSet<RefCusConditionApplicabilityLanguage>();
	}
}
