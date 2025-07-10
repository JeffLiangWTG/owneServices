using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;

namespace CargoWise.RefDbRepo.Common.Web
{
	public sealed class ODataEnableQueryAttribute : EnableQueryAttribute
	{
		public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
		{
			try
			{
				base.ValidateQuery(request, queryOptions);
			}
			catch (Exception e)
			{
				var argumentException = new ArgumentException(e.Message, e);
				throw argumentException;
			}
		}
	}
}
