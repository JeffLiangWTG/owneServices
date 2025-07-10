using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class WarehouseReceiveHelper
	{
		public static BusinessObject GetRelatedWarehouseReceiveForPivot(BusinessObjectFactory factory, ZGuid pk, string tablePrefix)
		{
			var pivotQuery = new ZQuery();
			pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_ParentId, pk);
			pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_ParentTableCode, tablePrefix);
			pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, "INW");

			var pivot = factory.LoadTop1<IWhsDocketJobPivot>(pivotQuery);
			return pivot != null ? factory.Load(ObjectFactory.GetType<IWhsReceive>(), pivot.WV_WD_Docket) : null;
		}
	}
}
