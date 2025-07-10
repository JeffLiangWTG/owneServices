using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	[TestedType(typeof(CommonCartageOrg))]
	sealed class CommonCaratgeOrgBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommonCartageType cartageType = factory.New<CommonCartageType>();
			cartageType.E3_GE = GlbDepartment.CurrentDepartment.PK;
			cartageType.E3_Description = "Some Description";
			cartageType.E3_JobType = "JOB";
			CommonCartageOrg cartageOrg = factory.New<CommonCartageOrg>();
			cartageOrg.E5_E3 = cartageType.PK;
			return cartageOrg;
		}

		#endregion
	}
}
