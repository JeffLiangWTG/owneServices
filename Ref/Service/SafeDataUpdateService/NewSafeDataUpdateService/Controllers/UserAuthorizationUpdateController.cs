using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.OData.Query;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	public partial class UserAuthorizationUpdateController
	{
		[InternalDataSetActionFilter]
		public override IEnumerable<UserAuthorization> Get(	)
		{
			return base.Get();
		}

		[InternalDataSetActionFilter]
		public override IEnumerable<UserAuthorization> Get(Guid key)
		{
			return base.Get(key);
		}

		[InternalDataSetActionFilter]
		public override IEnumerable<UserAuthorization> GetWithOptimizedExpand(ODataQueryOptions<UserAuthorization> queryOptions)
		{
			return base.GetWithOptimizedExpand(queryOptions);
		}
	}
}
