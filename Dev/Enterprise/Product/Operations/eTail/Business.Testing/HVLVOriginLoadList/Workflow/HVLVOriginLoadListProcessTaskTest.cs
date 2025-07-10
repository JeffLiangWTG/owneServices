using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOriginLoadListProcessTask))]
	public class HVLVOriginLoadListProcessTaskTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var originLoadList = Factory.New<HVLVOriginLoadList>();

			return originLoadList.WorkflowItems.AddNew();
		}

		#endregion
	}
}
