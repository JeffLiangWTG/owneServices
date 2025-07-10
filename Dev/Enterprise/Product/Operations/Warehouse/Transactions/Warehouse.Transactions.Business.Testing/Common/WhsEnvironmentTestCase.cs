using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsEnvironmentTestCase : WhsTestCaseWithFactory
	{
		#region TestIsRF

		public void TestIsRF()
		{
			AssertEquals("Precondition - when cargowise one.", false, WhsEnvironment.IsRF);

			try
			{
				Globals.IsWeb = true;
				AssertEquals("When Web Tracker.", false, WhsEnvironment.IsRF);

				Globals.IsUserInteractive = false;
				AssertEquals("When RF.", true, WhsEnvironment.IsRF);

				WhsEnvironment.IsRF = false;
				AssertEquals("Should be reset back to cargowise one.", false, WhsEnvironment.IsRF);

				WhsEnvironment.IsRF = true;
				AssertEquals("Should be set back to RF.", true, WhsEnvironment.IsRF);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestIsWebTracker

		public void TestIsWebTracker()
		{
			AssertEquals("Precondition - when cargowise one.", false, WhsEnvironment.IsWebTracker);

			try
			{
				Globals.IsWeb = true;
				AssertEquals("When Web Tracker.", true, WhsEnvironment.IsWebTracker);

				Globals.IsUserInteractive = false;
				AssertEquals("When RF.", false, WhsEnvironment.IsWebTracker);

				WhsEnvironment.IsWebTracker = true;
				AssertEquals("Should be set back to Web Tracker.", true, WhsEnvironment.IsWebTracker);

				WhsEnvironment.IsWebTracker = false;
				AssertEquals("Should be reset back to cargowise one.", false, WhsEnvironment.IsWebTracker);
			}
			finally
			{
				WhsEnvironment.IsWebTracker = false;
			}
		}

		#endregion
	}
}
