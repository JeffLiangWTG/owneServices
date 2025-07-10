using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsAreaInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsAreaInfo>
	{
		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsAreaInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsAreaInfoCollection);
		}

		protected override WhsAreaInfo GetNewObjectInfo()
		{
			return new WhsAreaInfo();
		}

		protected new WhsAreaInfoCollection Parent
		{
			get
			{
				return (WhsAreaInfoCollection)base.Parent;
			}
		}

		protected override DataObjectInfoCollection<WhsAreaInfo> GetNewObjectInfoCollection()
		{
			return new WhsAreaInfoCollection();
		}

		#endregion
	}
}
