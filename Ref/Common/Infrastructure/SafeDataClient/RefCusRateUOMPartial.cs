using System.Collections.Generic;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusRateUOM
	{
		[IgnoreClientProperty]
		public ICollection<RefCusRateApplicabilityUOM> RefCusRateApplicabilityUOMs { get; set; } = new HashSet<RefCusRateApplicabilityUOM>();
	}
}
