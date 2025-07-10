using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsPickMethodInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsPickMethodInfo>
	{
		#region Implementation

		protected new WhsPickMethodInfoCollection Parent
		{
			get
			{
				return (WhsPickMethodInfoCollection)base.Parent;
			}
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsPickMethodInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsPickMethodInfoCollection);
		}

		protected override WhsPickMethodInfo GetNewObjectInfo()
		{
			return new WhsPickMethodInfo();
		}

		protected override DataObjectInfoCollection<WhsPickMethodInfo> GetNewObjectInfoCollection()
		{
			return new WhsPickMethodInfoCollection();
		}

		#endregion
	}
}
