using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.OData.Query;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	public partial class RefDataSetInformationUpdateController
	{
		[InternalDataSetActionFilter]
		public override IEnumerable<RefDataSetInformation> Get()
		{
			return base.Get();
		}

		[InternalDataSetActionFilter]
		public override IEnumerable<RefDataSetInformation> Get(Guid key)
		{
			return base.Get(key);
		}

		[InternalDataSetActionFilter]
		public override IEnumerable<RefDataSetInformation> GetWithOptimizedExpand(ODataQueryOptions<RefDataSetInformation> queryOptions)
		{
			return base.GetWithOptimizedExpand(queryOptions);
		}
	}
}
