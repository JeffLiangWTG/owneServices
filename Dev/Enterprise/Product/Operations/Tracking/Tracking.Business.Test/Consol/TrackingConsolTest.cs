using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingConsolTest : CommonConsolTest2
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
			Globals.IsWeb = false;
		}

		protected override CommonConsol GetNewConsol()
		{
			return (CommonConsol)Factory.New(typeof(TrackingConsol));
		}

		protected TrackingConsol TestConsol
		{
			get
			{
				if (fTestConsol == null)
				{
					fTestConsol = (TrackingConsol)GetNewConsol();
				}
				return fTestConsol;
			}
		}
		TrackingConsol fTestConsol;

		#endregion

		public void TestPortOfLoadingPortName()
		{
			AssertNull("PreCondition: Expected PortOfLoading to initially be null", TestConsol.LoadPort);
			AssertEquals("Empty string should be returned for null PortOfLoading", "", TestConsol.PortOfLoadingPortName);
			RefUNLOCO testPort = Factory.NewWithValidTestData<RefUNLOCO>();
			testPort.RL_Code = "TST";
			testPort.RL_PortName = "TestPortName";

			TestConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			TestConsol.JK_RL_NKLoadPort = testPort.RL_Code;
			AssertEquals("Port name returned differs from expected", "TestPortName", TestConsol.PortOfLoadingPortName);
		}

		public void TestPortOfDischargePortName()
		{
			AssertNull("PreCondition: Expected PortOfDischarge to initially be null", TestConsol.DischargePort);
			AssertEquals("Empty string should be returned for null PortOfDischarge", "", TestConsol.PortOfDischargePortName);
			RefUNLOCO testPort = Factory.NewWithValidTestData<RefUNLOCO>();
			testPort.RL_Code = "TST";
			testPort.RL_PortName = "TestPortName";

			TestConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			TestConsol.JK_RL_NKDischargePort = testPort.RL_Code;
			AssertEquals("Port name returned differs from expected", "TestPortName", TestConsol.PortOfDischargePortName);
		}

		public void TestVoyageFlightWithSuppression()
		{
			SuppressionTest.DummySuppressionBizO sbizO = new SuppressionTest.DummySuppressionBizO();
			TestConsol.FlightDetailsSuppressionBizO = sbizO;
			Transport transport = TestConsol.Transports[0];
			transport.JW_VoyageFlight = "ABC";
			AssertEquals("ABC", TestConsol.VoyageFlightWithSuppression);
			sbizO.IsAir = true;
			AssertEquals("ABC", TestConsol.VoyageFlightWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);

			AssertEquals(Suppression.SuppressedString, TestConsol.VoyageFlightWithSuppression);
		}

		public void TestMasterBillNumWithSuppression()
		{
			SuppressionTest.DummySuppressionBizO sbizO = new SuppressionTest.DummySuppressionBizO();
			TestConsol.FlightDetailsSuppressionBizO = sbizO;
			TestConsol.JK_MasterBillNum = "ABC";
			AssertEquals("ABC", TestConsol.MasterBillNumWithSuppression);
			sbizO.IsAir = true;
			AssertEquals("ABC", TestConsol.MasterBillNumWithSuppression);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);

			AssertEquals(Suppression.SuppressedString, TestConsol.MasterBillNumWithSuppression);
		}
	}
}
