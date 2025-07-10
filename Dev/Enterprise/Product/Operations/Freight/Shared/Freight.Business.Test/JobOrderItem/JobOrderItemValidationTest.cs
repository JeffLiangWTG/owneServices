using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobOrderItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJT_OrderReference()
		{
			OrderItem item = Factory.New<OrderItem>();
			item.JT_OrderReference = "";
			AssertEquals("JT_OrderReference mandatory", true, item.JT_OrderReferenceInfo.HasErrors());
			item.JT_OrderReference = "12-/:#&*3";
			AssertEquals("Shouldn't have any errors", false, item.JT_OrderReferenceInfo.HasErrors());
			item.JT_OrderReference = "123,456";
			AssertEquals("Only alpha-numeric and some others allowed", true, item.JT_OrderReferenceInfo.HasErrors());
			item.JT_OrderReference = "123 456";
			AssertEquals("No spaces allowed", true, item.JT_OrderReferenceInfo.HasErrors());
		}
	}
}
