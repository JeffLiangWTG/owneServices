using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.OData.Query;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	public partial class DataPushSubscriptionUpdateController
	{
		[InternalDataSetActionFilter]
		public override IEnumerable<DataPushSubscription> Get()
		{
			return base.Get();
		}

		[InternalDataSetActionFilter]
		public override IEnumerable<DataPushSubscription> Get(Guid key)
		{
			return base.Get(key);
		}

		[InternalDataSetActionFilter]
		public override IEnumerable<DataPushSubscription> GetWithOptimizedExpand(ODataQueryOptions<DataPushSubscription> queryOptions)
		{
			return base.GetWithOptimizedExpand(queryOptions);
		}
	}
}
