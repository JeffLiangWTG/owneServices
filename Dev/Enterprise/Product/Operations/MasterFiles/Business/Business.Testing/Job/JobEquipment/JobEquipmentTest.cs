using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobEquipment))]
	public class JobEquipmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(BO.GetType()));
		}

		#region Implementation

		JobEquipment BO;

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			BO = factory.New<JobEquipment>();
		}

		#endregion
	}
}
