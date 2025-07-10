using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOuterPackageProcessTask))]
	public class HVLVOuterPackageProcessTaskTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var outerPackage = Factory.New<HVLVOuterPackage>();
			return outerPackage.WorkflowItems.AddNew();
		}

		#endregion
	}
}
