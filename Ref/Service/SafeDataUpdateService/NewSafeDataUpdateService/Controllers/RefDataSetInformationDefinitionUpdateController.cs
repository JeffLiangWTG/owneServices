using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.OData.Query;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	public partial class RefDataSetInformationDefinitionUpdateController
	{
		[InternalDataSetActionFilter]
		public override IEnumerable<RefDataSetInformationDefinition> Get()
		{
			return base.Get();
		}

		[InternalDataSetActionFilter]
		public override IEnumerable<RefDataSetInformationDefinition> Get(Guid key)
		{
			return base.Get(key);
		}

		[InternalDataSetActionFilter]
		public override IEnumerable<RefDataSetInformationDefinition> GetWithOptimizedExpand(ODataQueryOptions<RefDataSetInformationDefinition> queryOptions)
		{
			return base.GetWithOptimizedExpand(queryOptions);
		}
	}
}
