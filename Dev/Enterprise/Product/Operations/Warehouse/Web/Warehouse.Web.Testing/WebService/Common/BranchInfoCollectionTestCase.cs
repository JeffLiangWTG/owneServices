using System;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	public class BranchInfoCollectionTestCase : DataObjectInfoCollectionTestCase<BranchInfo>
	{
		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(BranchInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(BranchInfoCollection);
		}

		protected override BranchInfo GetNewObjectInfo()
		{
			return new BranchInfo();
		}

		protected new BranchInfoCollection Parent
		{
			get
			{
				return (BranchInfoCollection)base.Parent;
			}
		}

		protected override DataObjectInfoCollection<BranchInfo> GetNewObjectInfoCollection()
		{
			return new BranchInfoCollection();
		}

		#endregion
	}
}
