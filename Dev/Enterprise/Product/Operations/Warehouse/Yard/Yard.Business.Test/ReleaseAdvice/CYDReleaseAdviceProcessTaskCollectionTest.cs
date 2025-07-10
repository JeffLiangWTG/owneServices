using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReleaseAdviceProcessTaskCollection))]
	public class CYDReleaseAdviceProcessTaskCollectionTest : ProcessTaskCollectionTest<CYDReleaseAdviceProcessTaskCollection>
	{
		protected override CYDReleaseAdviceProcessTaskCollection GetCollectionToTestCore()
		{
			var releaseAdvice = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			return new CYDReleaseAdviceProcessTaskCollection(releaseAdvice);
		}
	}
}
