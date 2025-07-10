using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public static class WhsReceiveHelper
	{
		public static void SetSingleDockDoorLocationDetails(BusinessObjectFactory factory, WhsDocketWebServiceBaseResponse response, string whsCode)
		{
			var dockDoorLocations = WebServiceHelper.GetDockDoorLocations(factory, whsCode);
			var isSingleDDL = dockDoorLocations.Count() == 1;
			response.WarehouseHasSingleDockDoorLocation = isSingleDDL;

			if (isSingleDDL)
			{
				var ddl = dockDoorLocations.First();
				response.SingleDockDoorLocation = ddl.WLV_LocationString;
				response.SingleDockDoorLocation_UserFriendly = ddl.WLV_LocationString_UserFriendly;
				response.SingleDockDoorLocationPK = ddl.PK.ToGuid();
			}
		}
	}
}
