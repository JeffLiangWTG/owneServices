using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolExportAWBRateLine))]
	sealed class ConsolExportAWBRateLineTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestHumanReadableName()
		{
			ConsolExportAWBHeader aWBHeader = Factory.New<ConsolExportAWBHeader>();
			ConsolExportAWBRateLine rateLine = (ConsolExportAWBRateLine)aWBHeader.AWBRateLines.AddNew();
			AssertEquals("Master Air Waybill Freight Breakdown", rateLine.HumanReadableName);
		}

		public void TestRequireHSCode()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "DEHAM";

			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var rateLine = (ConsolExportAWBRateLine)header.AWBRateLines.AddNew();
			Assert(rateLine.RequireHSCode);

			consol.JK_RL_NKDischargePort = "NZAKL";
			Assert(!rateLine.RequireHSCode);

			consol.JK_RL_NKLoadPort = "ITSPE";
			consol.JK_RL_NKDischargePort = "DEHAM";

			transport.JW_RL_NKLoadPort = "ITSPE";
			transport.JW_RL_NKDiscPort = "DEHAM";
			Assert(!rateLine.RequireHSCode);
		}

		public void TestRequireHSCode_WhenAirLegsAreDomestic()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "DEFRA";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "USJFK";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "USJFK";
			transport2.JW_RL_NKDiscPort = "USHAM";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "USHAM";
			transport3.JW_RL_NKDiscPort = "USFRA";

			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var rateLine = (ConsolExportAWBRateLine)header.AWBRateLines.AddNew();
			Assert(!rateLine.RequireHSCode);
		}

		public void TestRequireHSCode_WhenSomeEuAirLegsAreDomesticDuringTransit()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "UAKBP";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "USJFK";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "USJFK";
			transport2.JW_RL_NKDiscPort = "USHAM";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "USHAM";
			transport3.JW_RL_NKDiscPort = "USFRA";

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport4.JW_RL_NKLoadPort = "USFRA";
			transport4.JW_RL_NKDiscPort = "UAKBP";

			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var rateLine = (ConsolExportAWBRateLine)header.AWBRateLines.AddNew();
			Assert(!rateLine.RequireHSCode);
		}

		public void TestRequireHSCode_WhenTransittingThroughEU()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "DEHAM";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "USLAX";

			var header = Factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var rateLine = (ConsolExportAWBRateLine)header.AWBRateLines.AddNew();
			Assert(rateLine.RequireHSCode);

			transport1.JW_RL_NKDiscPort = "NZAKL";
			Assert(!rateLine.RequireHSCode);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ConsolExportAWBRateLine>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var header = factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			header.ForceSavingByFactory = true;

			var result = factory.NewWithValidTestData<ConsolExportAWBRateLine>();
			result.ER_EH = header.PK;

			return result;
		}
	}
}
