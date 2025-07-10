using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeaderProcessTaskCollection))]
	sealed class CusISFHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<CusISFHeaderProcessTaskCollection>
	{
		public void TestIsConditionMet()
		{
			AssertEquals(true, Collection.IsCondition1Met("ZZZ"));
			AssertEquals(true, Collection.IsCondition2Met(ZString.Empty, ""));
		}

		protected override CusISFHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			var header = Factory.NewWithValidTestData<CusISFHeader>();
			return new CusISFHeaderProcessTaskCollection(header);
		}
	}
}
