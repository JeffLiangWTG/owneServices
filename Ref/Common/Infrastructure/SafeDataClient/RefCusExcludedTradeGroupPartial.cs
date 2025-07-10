using System.Collections.Generic;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class RefCusExcludedTradeGroup
	{
		[IgnoreClientProperty]
		public ICollection<RefCusExcludedTradeGroupNew> RefCusExcludedTradeGroupNews { get; set; } = new HashSet<RefCusExcludedTradeGroupNew>();
	}
}
