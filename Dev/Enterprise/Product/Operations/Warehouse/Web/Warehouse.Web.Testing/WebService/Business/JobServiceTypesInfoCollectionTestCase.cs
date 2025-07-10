using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class JobServiceTypesInfoCollectionTestCase : DataObjectInfoCollectionTestCase<JobServiceTypesInfo>
	{
		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(JobServiceTypesInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(JobServiceTypesInfoCollection);
		}

		protected override JobServiceTypesInfo GetNewObjectInfo()
		{
			return new JobServiceTypesInfo();
		}

		protected new JobServiceTypesInfoCollection Parent
		{
			get { return (JobServiceTypesInfoCollection)base.Parent; }
		}

		protected override DataObjectInfoCollection<JobServiceTypesInfo> GetNewObjectInfoCollection()
		{
			return new JobServiceTypesInfoCollection();
		}

		#endregion
	}
}
