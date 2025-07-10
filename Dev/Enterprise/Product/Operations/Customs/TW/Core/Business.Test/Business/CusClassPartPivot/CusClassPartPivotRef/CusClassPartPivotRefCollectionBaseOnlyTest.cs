using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotRefCollectionForTest))]
	sealed class CusClassPartPivotRefCollectionBaseOnlyTest : CusClassPartPivotRefCollectionTest<CusClassPartPivotRefForTest>
	{
		protected override CusClassPartPivotRefCollection<CusClassPartPivotRefForTest> GetCusClassPartPivotRefCollection()
		{
			return new CusClassPartPivotRefCollectionForTest(Factory.New<CusClassPartPivot>());
		}
	}
}
