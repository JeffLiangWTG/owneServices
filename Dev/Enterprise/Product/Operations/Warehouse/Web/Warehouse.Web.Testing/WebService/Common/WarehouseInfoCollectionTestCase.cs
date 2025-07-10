using System;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	public class WarehouseInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WarehouseInfo>
	{
		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WarehouseInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WarehouseInfoCollection);
		}

		protected override WarehouseInfo GetNewObjectInfo()
		{
			return new WarehouseInfo();
		}

		protected new WarehouseInfoCollection Parent
		{
			get
			{
				return (WarehouseInfoCollection)base.Parent;
			}
		}

		protected override DataObjectInfoCollection<WarehouseInfo> GetNewObjectInfoCollection()
		{
			return new WarehouseInfoCollection();
		}

		#endregion
	}
}
