using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class NewExportAWBHeaderTestForEu : NewConsolExportAWBHeaderTest
	{
		ZString portCode = "";
		// SCI, IATA AWB box 21A
		public void TestSpecialHandlingCode_GB()
		{
			RunCtTest(true, false, "GBLON", "AUSYD", true);
		}

		public void TestSpecialHandlingCode_CH()
		{
			RunCtTest(false, true, "CHZRH", "AUSYD", true);
		}

		public void TestSpecialHandlingCode_NO()
		{
			RunCtTest(true, false, "NOOSL", "AUSYD", true);
		}

		public void TestSpecialHandlingCode_DE()
		{
			RunCtTest(true, false, "DEHAM", "AUSYD", true);
		}

		public void TestSpecialHandlingCode_EUDomestic()
		{
			RunCtTest(true, false, "ESMAD", "ESBCN", true);
		}

		public void TestSpecialHandlingCode_Domestic()
		{
			RunCtTest(false, false, "AUMEL", "AUBNE", false);
		}

		public void TestSpecialHandlingCode_TransittingThroughEU()
		{
			RunCtTest("INDEL", "DEHAM", "USLAX", true);
			RunCtTest("INDEL", "AUSYD", "NZAKL", false);
		}

		#region Implementation

		void RunCtTest(bool addEU, bool addTransit, ZString loadPort, ZString dischargePort, bool shouldBePopulated)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = loadPort.Right(3);
			company.GC_RN_NKCountryCode = loadPort.Left(2);
			var branch = company.Branches.AddNew();
			branch.GB_Code = company.GC_Code;
			branch.GB_RL_NKHomePort = loadPort;
			portCode = loadPort;

			if (addEU || addTransit)
			{
				var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping("EUN");
				if (addEU)
				{
					var tradeGroup1 = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
					helper.AddCountry(tradeGroup1, company.GC_RN_NKCountryCode, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
				}

				if (addTransit)
				{
					var tradeGroup2 = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
					helper.AddCountry(tradeGroup2, company.GC_RN_NKCountryCode, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
				}
			}
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				AWBHeader.Consol.JK_RL_NKDischargePort = dischargePort;
				AWBHeader.Consol.Numbers.RemoveAndDeleteAll();
				AWBHeader.Consol.Shipments.RemoveAndDeleteAll();

				var shipment_MRN = AWBHeader.Consol.Shipments.AddNew();
				shipment_MRN.JS_CommunityTransitStatus = "MRN";
				AssertEquals("SpecialHandlingCode is empty, as it does not belong to ExportCommunityTransitStatus list.", "", AWBHeader.SpecialHandlingCode);
				var shipment_X = AWBHeader.Consol.Shipments.AddNew();
				shipment_X.JS_CommunityTransitStatus = "X";
				AssertEquals("CT status of export consol is X, the most severe of its shipments' CTs", shouldBePopulated ? "X" : "", AWBHeader.SpecialHandlingCode);
				var shipment_C = AWBHeader.Consol.Shipments.AddNew();
				shipment_C.JS_CommunityTransitStatus = "C";
				AssertEquals("CT status of export consol is X, the most severe of its shipments' CTs", shouldBePopulated ? "X" : "", AWBHeader.SpecialHandlingCode);
				var shipment_TD = AWBHeader.Consol.Shipments.AddNew();
				shipment_TD.JS_CommunityTransitStatus = "TD";
				AssertEquals("CT status of export consol is TD, the most severe of its shipments' CTs", shouldBePopulated ? "TD" : "", AWBHeader.SpecialHandlingCode);
				var shipment_T1 = AWBHeader.Consol.Shipments.AddNew();
				shipment_T1.JS_CommunityTransitStatus = "T1";
				AssertEquals("CT status of export consol is still TD", shouldBePopulated ? "TD" : "", AWBHeader.SpecialHandlingCode);
			}
		}

		void RunCtTest(ZString loadPort, ZString dischargePort, ZString destinationPort, bool shouldBePopulated)
		{
			AWBHeader.Consol.JK_RL_NKLoadPort = loadPort;
			AWBHeader.Consol.JK_RL_NKDischargePort = destinationPort;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = loadPort;
			shipment.JS_RL_NKDestination = destinationPort;

			var transport1 = AWBHeader.Consol.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportCodes.Air;
			transport1.JW_RL_NKLoadPort = loadPort;
			transport1.JW_RL_NKDiscPort = dischargePort;

			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportCodes.Air;
			transport2.JW_RL_NKLoadPort = dischargePort;
			transport2.JW_RL_NKDiscPort = destinationPort;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(dischargePort.Left(2)))
			{
				shipment.JS_CommunityTransitStatus = "T1";

				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T1" : string.Empty, AWBHeader.EH_SpecialHandlingCode);
			}
		}

		protected override BusinessObject GetNewParent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = portCode;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = portCode;

			return consol;
		}

		#endregion
	}
}
