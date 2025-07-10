using System.Collections.Generic;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusApplicability
	{
		[IgnoreClientProperty]
		public ICollection<RefCusRateApplicability> RefCusRateApplicabilities { get; set; } = new HashSet<RefCusRateApplicability>();

		[IgnoreClientProperty]
		public ICollection<RefCusConditionApplicability> RefCusConditionApplicabilities { get; set; } = new HashSet<RefCusConditionApplicability>();
	}
}
