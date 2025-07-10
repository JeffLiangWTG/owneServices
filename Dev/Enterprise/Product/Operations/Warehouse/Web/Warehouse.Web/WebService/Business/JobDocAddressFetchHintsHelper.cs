using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class JobDocAddressFetchHintsHelper
	{
		public static void AddFetchHints(IEnumerable<ZGuid> docketPKs, BusinessObjectFactory factory)
		{
			foreach (var orderPK in docketPKs)
			{
				var query = new ZQuery(JobDocAddressSchema.E2_ParentTableCode, WhsDocketSchema.Constants.Prefix);
				query.AddToFilter(JobDocAddressSchema.E2_ParentID, orderPK);
				factory.AddFetchHint(JobDocAddressSchema.Instance, query);
			}
		}
	}
}
