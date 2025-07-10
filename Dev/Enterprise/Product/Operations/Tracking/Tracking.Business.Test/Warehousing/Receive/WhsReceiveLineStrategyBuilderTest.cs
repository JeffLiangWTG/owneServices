using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class WhsReceiveLineStrategyBuilderTest : TestCaseWithFactory
	{
		#region WhsReceiveLineWrapperStrategy

		public void TestBuildWrapperStrategyIsWeb()
		{
			try
			{
				Globals.IsWeb = true;

				var receive = Factory.New<WhsReceive>();
				var wrapper = receive.WrapperStrategy;
				Assert("Should have created the wrapper.", wrapper != null && !(wrapper is WhsReceive));
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestBuildWrapperStrategyDefault()
		{
			try
			{
				Globals.IsWeb = false;

				var receive = Factory.New<WhsReceive>();
				var wrapper = receive.WrapperStrategy;
				Assert("Should have created the wrapper.", wrapper != null && wrapper is WhsReceive);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#endregion
	}
}
