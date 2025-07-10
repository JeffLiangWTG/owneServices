using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class BusinessObjectMergerHelperTest : TestCaseWithFactory
	{
		public void TestMerge_DoMerge()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Decimal = 15m;

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Decimal = 354m;

			var list = new[] { dummy1, dummy2 };
			var merger = new DummyMerger(list)
			{
				HasErrorMessage = false
			};

			var helper = new BusinessObjectMergerHelper<DummyBusinessObject>(merger, null);
			helper.Merge();

			AssertEquals("Should merge with dummy2's Z0_Decimal", 369m, dummy1.Z0_Decimal);
			AssertEquals("Should be zero", 0m, dummy2.Z0_Decimal);
		}

		public void TestMerge_CheckMerger()
		{
			var list = new[]
			{
				Factory.New<DummyBusinessObject>(),
				Factory.New<DummyBusinessObject>()
			};

			var merger = new DummyMerger(list)
			{
				HasErrorMessage = true
			};

			var helper = new BusinessObjectMergerHelper<DummyBusinessObject>(merger, null);
			helper.Merge();

			var actual = UnitTestUserNotification.Instance.LastMessage.Text;
			var expected = @"Selected items cannot be merged.

Error Message";
			AssertEquals("Should show the error message from the merger.", expected, actual);
		}
	}
}
