using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Warehouse.Transit.Business
{
	public class ViewPackagesCFSInfoController
	{
		public ViewPackagesCFSInfoController(ViewPackagesManager manager)
		{
			CFSInfoList = new List<ViewPackagesCFSInfo>();
			Manager = manager;
		}

		public List<ViewPackagesCFSInfo> CFSInfoList { get; }
		public ViewPackagesManager Manager { get; }

		public EventHandler CurrentWarehouseChanged;

		public void CFSAddressSelected(ViewPackagesCFSInfo selectedCFSInfo)
		{
			foreach (var cfsDetail in CFSInfoList.Where(o => o != selectedCFSInfo))
			{
				cfsDetail.Deselect();
			}

			selectedCFSInfo.Select();
			Manager.SetCurrentWarehousePK(selectedCFSInfo.TransitWarehouse.PK);
			CurrentWarehouseChanged?.Invoke(this, null);
		}
	}
}
