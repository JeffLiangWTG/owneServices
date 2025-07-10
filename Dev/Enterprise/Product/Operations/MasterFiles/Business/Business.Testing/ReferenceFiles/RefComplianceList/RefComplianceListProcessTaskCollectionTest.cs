using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefComplianceListProcessTaskCollection))]
	class RefComplianceListProcessTaskCollectionTest : ProcessTaskCollectionTest<RefComplianceListProcessTaskCollection>
	{
		protected override RefComplianceListProcessTaskCollection GetCollectionToTestCore()
		{
			return new RefComplianceListProcessTaskCollection(Factory.New<RefComplianceList>());
		}
	}
}
