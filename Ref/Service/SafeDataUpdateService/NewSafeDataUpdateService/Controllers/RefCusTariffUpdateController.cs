using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	public partial class RefCusTariffUpdateController : DataSetHeaderController<RefCusTariff>
	{
		[HttpGet]
		[SystemVersion]
		public override IEnumerable<RefCusTariff> GetWithOptimizedExpand(ODataQueryOptions<RefCusTariff> queryOptions)
		{
			var results = queryOptions.ApplyTo(GetCore(), AllowedQueryOptions.Expand)?.Cast<RefCusTariff>().OrderBy(x => x.ZZ1_PK).ToArray();
			var expandClause = queryOptions.SelectExpand?.SelectExpandClause;
			if (expandClause != null && results != null)
			{
				Repository.ExpandTariff(results, new ExpandClauseWrapper(expandClause));
			}
			var oDataProperties = Request.ODataFeature();
			if (oDataProperties != null && expandClause != null)
			{
				oDataProperties.SelectExpandClause = expandClause;
			}
			return results;
		}
	}
}
