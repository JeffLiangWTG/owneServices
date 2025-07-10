using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVClearanceProcessTaskCollection))]
	public class CusUSLVClearanceProcessTaskCollectionTest : ProcessTaskCollectionTest<CusUSLVClearanceProcessTaskCollection>
	{
		#region Implementation

		protected override CusUSLVClearanceProcessTaskCollection GetCollectionToTestCore()
		{
			return new CusUSLVClearanceProcessTaskCollection(Factory.NewWithValidTestData<CusUSLVClearance>());
		}

		#endregion
	}
}
