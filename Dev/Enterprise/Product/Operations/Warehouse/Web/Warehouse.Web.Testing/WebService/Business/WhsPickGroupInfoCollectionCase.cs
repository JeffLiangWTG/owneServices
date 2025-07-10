using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsPickGroupInfoCollectionCase : DataObjectInfoCollectionTestCase<WhsPickGroupInfo>
	{
		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsPickGroupInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsPickGroupInfoCollection);
		}

		protected override WhsPickGroupInfo GetNewObjectInfo()
		{
			return new WhsPickGroupInfo();
		}

		protected override DataObjectInfoCollection<WhsPickGroupInfo> GetNewObjectInfoCollection()
		{
			return new WhsPickGroupInfoCollection();
		}

		#endregion
	}
}
