using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;

namespace Enterprise.LandedCosting.GUI.Testing
{
	sealed class LandedLineCostItemAmountPropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestLandedLineCostItemAmountPropertyDescriptor()
		{
			var history = Factory.New<LandedCostHistory>();
			history.SetLineValue(1000m, "TDT");
			var descriptor = new LandedLineCostItemAmountPropertyDescriptor("TDT");
			AssertEquals("PropertyType", typeof(ZDecimal), descriptor.PropertyType);
			AssertEquals("IsReadOnly", true, descriptor.IsReadOnly);
			AssertEquals("GetValue", 1000m, descriptor.GetValue(history));
		}
	}
}
