using System;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	public class DepartmentInfoCollectionTestCase : DataObjectInfoCollectionTestCase<DepartmentInfo>
	{
		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(DepartmentInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(DepartmentInfoCollection);
		}

		protected override DepartmentInfo GetNewObjectInfo()
		{
			return new DepartmentInfo();
		}

		protected new DepartmentInfoCollection Parent
		{
			get
			{
				return (DepartmentInfoCollection)base.Parent;
			}
		}

		protected override DataObjectInfoCollection<DepartmentInfo> GetNewObjectInfoCollection()
		{
			return new DepartmentInfoCollection();
		}

		#endregion
	}
}
