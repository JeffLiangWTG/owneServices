using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class WeightUQCalculatorNotSubclassedTest : TestCaseWithFactory
	{
		public void TestSingleEntryWeightProxiesBackToDeclaration()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_TotalWeight = 150m;
			dec.JE_TotalWeightUnit = "KG";
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			AssertEquals(150m, entryHeader.GrossWeight.InKilograms);
		}
	}
}
