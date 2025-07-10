using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class WhsReceiveStrategyBuilderTest : TestCaseWithFactory
	{
		#region WhsReceiveLineWrapperStrategy

		public void TestBuildWrapperStrategyIsWeb()
		{
			Globals.IsWeb = true;

			var receive = Factory.New<WhsReceiveLine>();
			var wrapper = receive.WrapperStrategy;
			Assert("Should have created the wrapper.", wrapper != null && !(wrapper is WhsReceiveLine));
			Globals.IsWeb = false;
		}

		public void TestBuildWrapperStrategyDefault()
		{
			try
			{
				Assert("Pre-Condition: No in web environment.", !Globals.IsWeb);
				var receive = Factory.New<WhsReceiveLine>();
				var wrapper = receive.WrapperStrategy;
				Assert("Should have created the wrapper.", wrapper != null && wrapper is WhsReceiveLine);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#endregion
	}
}
