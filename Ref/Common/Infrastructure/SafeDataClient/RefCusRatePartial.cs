using Microsoft.OData.Client;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusRate: INonPersistentBusinessObjectParent
	{
		[IgnoreClientProperty]
		public ICollection<RefCusRateApplicability> RefCusRateApplicabilities { get; set; } = new HashSet<RefCusRateApplicability>();
	}
}
