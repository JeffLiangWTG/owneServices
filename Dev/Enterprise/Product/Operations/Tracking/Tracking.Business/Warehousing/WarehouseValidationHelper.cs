using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	static class WarehouseValidationHelper
	{
		public static void CheckWarehouseEligibility(ZPropertyInfo warehousePropertyInfo, OrgContact contact)
		{
			if (!warehousePropertyInfo.HasErrors() && contact != null && !warehousePropertyInfo.Value.IsEmpty)
			{
				var collection = new OrgContactWebWarehouseEligibilityCollection(contact.Factory, contact);
				var warehouseEligibility = collection.Cast<OrgContactWebWarehouseEligibility>().SingleOrDefault(x => (ZGuid)warehousePropertyInfo.Value == x.WarehousePK);
				if (warehouseEligibility != null && !warehouseEligibility.IsGranted)
				{
					warehousePropertyInfo.AddError(Res.GetString("4938a6ca-ae9d-4f9f-bddc-3e14ebb1ee01", "You are not authorized for warehouse {0}. Please contact your system administrator to request access rights.", warehouseEligibility.WarehouseName));
				}
			}
		}
	}
}
