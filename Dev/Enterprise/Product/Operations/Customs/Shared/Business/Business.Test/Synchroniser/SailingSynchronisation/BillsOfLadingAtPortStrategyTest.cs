using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BillsOfLadingAtPortStrategyTest : TestCaseWithFactory
	{
		public void TestGetBillsOnVesselA_PortMatching()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = ZDateTime.Today.AddDays(1);
			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "USCHI";
			destination1.JB_E_ARV = ZDateTime.Today.AddDays(5);
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUMEL";
			origin2.JA_E_DEP = ZDateTime.Today.AddDays(10);
			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "USLAX";
			destination2.JB_E_ARV = ZDateTime.Today.AddDays(15);

			voyage.GenerateSailings();
			var sailing1 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA == origin1.PK && x.JX_JB == destination1.PK);
			var sailing2 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA == origin2.PK && x.JX_JB == destination2.PK);
			var sailing3 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA == origin1.PK && x.JX_JB == destination2.PK);

			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_JX = sailing1.PK;
			bill1.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var bill2 = Factory.New<BillOfLading>();
			bill2.JS_JX = sailing2.PK;
			bill2.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var bill3 = Factory.New<BillOfLading>();
			bill3.JS_JX = sailing3.PK;
			bill3.JS_PackingMode = Core.Constants.ContainerModes.Bulk;

			var billsAtDestination2 = new List<BillOfLading>(new BillsOfLadingAtPortStrategy(voyage).GetBillsOnVesselAt(ZDateTime.Today.AddDays(15), false));
			AssertEquals(2, billsAtDestination2.Count);
			Assert(!billsAtDestination2.Contains(bill1));
			Assert(billsAtDestination2.Contains(bill2));
			Assert(billsAtDestination2.Contains(bill3));

			billsAtDestination2 = new List<BillOfLading>(new BillsOfLadingAtPortStrategy(voyage).GetBillsOnVesselAt(ZDateTime.Today.AddDays(15), true));
			AssertEquals(1, billsAtDestination2.Count);
			Assert(!billsAtDestination2.Contains(bill1));
			Assert(billsAtDestination2.Contains(bill2));
			Assert(!billsAtDestination2.Contains(bill3));
		}

		public void TestGetBillsOnVesselAt_PackingModeCoverage()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUMEL";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(10);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(15);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA == origin.PK && x.JX_JB == destination.PK);

			var billCON = Factory.New<BillOfLading>();
			billCON.JS_JX = sailing.PK;
			billCON.JS_PackingMode = Core.Constants.ContainerModes.AgentConsol;
			var billAIR = Factory.New<BillOfLading>();
			billAIR.JS_JX = sailing.PK;
			billAIR.JS_PackingMode = Core.Constants.ContainerModes.AIR;
			var billBBK = Factory.New<BillOfLading>();
			billBBK.JS_JX = sailing.PK;
			billBBK.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			var billBLK = Factory.New<BillOfLading>();
			billBLK.JS_JX = sailing.PK;
			billBLK.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			var billBCN = Factory.New<BillOfLading>();
			billBCN.JS_JX = sailing.PK;
			billBCN.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			var billCOM = Factory.New<BillOfLading>();
			billCOM.JS_JX = sailing.PK;
			billCOM.JS_PackingMode = Core.Constants.ContainerModes.Combination;
			var billCNT = Factory.New<BillOfLading>();
			billCNT.JS_JX = sailing.PK;
			billCNT.JS_PackingMode = Core.Constants.ContainerModes.Containerised;
			var billEMP = Factory.New<BillOfLading>();
			billEMP.JS_JX = sailing.PK;
			billEMP.JS_PackingMode = Core.Constants.ContainerModes.Empty;
			var billFCL = Factory.New<BillOfLading>();
			billFCL.JS_JX = sailing.PK;
			billFCL.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var billFCX = Factory.New<BillOfLading>();
			billFCX.JS_JX = sailing.PK;
			billFCX.JS_PackingMode = Core.Constants.ContainerModes.FCLMixedShipper;
			var billFAK = Factory.New<BillOfLading>();
			billFAK.JS_JX = sailing.PK;
			billFAK.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
			var billFTL = Factory.New<BillOfLading>();
			billFTL.JS_JX = sailing.PK;
			billFTL.JS_PackingMode = Core.Constants.ContainerModes.FTL;
			var billGRP = Factory.New<BillOfLading>();
			billGRP.JS_JX = sailing.PK;
			billGRP.JS_PackingMode = Core.Constants.ContainerModes.Groupage;
			var billLCL = Factory.New<BillOfLading>();
			billLCL.JS_JX = sailing.PK;
			billLCL.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			var billLQD = Factory.New<BillOfLading>();
			billLQD.JS_JX = sailing.PK;
			billLQD.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			var billLSE = Factory.New<BillOfLading>();
			billLSE.JS_JX = sailing.PK;
			billLSE.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			var billLTL = Factory.New<BillOfLading>();
			billLTL.JS_JX = sailing.PK;
			billLTL.JS_PackingMode = Core.Constants.ContainerModes.LTL;
			var billMAI = Factory.New<BillOfLading>();
			billMAI.JS_JX = sailing.PK;
			billMAI.JS_PackingMode = Core.Constants.ContainerModes.Mail;
			var billNCT = Factory.New<BillOfLading>();
			billNCT.JS_JX = sailing.PK;
			billNCT.JS_PackingMode = Core.Constants.ContainerModes.NonContainerised;
			var billOBC = Factory.New<BillOfLading>();
			billOBC.JS_JX = sailing.PK;
			billOBC.JS_PackingMode = Core.Constants.ContainerModes.OnBoardCourier;
			var billOTH = Factory.New<BillOfLading>();
			billOTH.JS_JX = sailing.PK;
			billOTH.JS_PackingMode = Core.Constants.ContainerModes.Other;
			var billROR = Factory.New<BillOfLading>();
			billROR.JS_JX = sailing.PK;
			billROR.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			var billULD = Factory.New<BillOfLading>();
			billULD.JS_JX = sailing.PK;
			billULD.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			var billUNA = Factory.New<BillOfLading>();
			billUNA.JS_JX = sailing.PK;
			billUNA.JS_PackingMode = Core.Constants.ContainerModes.Unaccompanied;

			var billsAtDestination = new List<BillOfLading>(new BillsOfLadingAtPortStrategy(voyage).GetBillsOnVesselAt(ZDateTime.Today.AddDays(15), false));
			AssertEquals(24, billsAtDestination.Count);
			Assert(billsAtDestination.Contains(billCON));
			Assert(billsAtDestination.Contains(billAIR));
			Assert(billsAtDestination.Contains(billBBK));
			Assert(billsAtDestination.Contains(billBLK));
			Assert(billsAtDestination.Contains(billBCN));
			Assert(billsAtDestination.Contains(billCOM));
			Assert(billsAtDestination.Contains(billCNT));
			Assert(billsAtDestination.Contains(billEMP));
			Assert(billsAtDestination.Contains(billFCL));
			Assert(billsAtDestination.Contains(billFCX));
			Assert(billsAtDestination.Contains(billFAK));
			Assert(billsAtDestination.Contains(billFTL));
			Assert(billsAtDestination.Contains(billGRP));
			Assert(billsAtDestination.Contains(billLCL));
			Assert(billsAtDestination.Contains(billLQD));
			Assert(billsAtDestination.Contains(billLSE));
			Assert(billsAtDestination.Contains(billLTL));
			Assert(billsAtDestination.Contains(billMAI));
			Assert(billsAtDestination.Contains(billNCT));
			Assert(billsAtDestination.Contains(billOBC));
			Assert(billsAtDestination.Contains(billOTH));
			Assert(billsAtDestination.Contains(billROR));
			Assert(billsAtDestination.Contains(billULD));
			Assert(billsAtDestination.Contains(billUNA));

			billsAtDestination = new List<BillOfLading>(new BillsOfLadingAtPortStrategy(voyage).GetBillsOnVesselAt(ZDateTime.Today.AddDays(15), true));
			AssertEquals(5, billsAtDestination.Count);
			Assert(!billsAtDestination.Contains(billCON));
			Assert(!billsAtDestination.Contains(billAIR));
			Assert(!billsAtDestination.Contains(billBBK));
			Assert(!billsAtDestination.Contains(billBLK));
			Assert(billsAtDestination.Contains(billBCN));
			Assert(billsAtDestination.Contains(billCOM));
			Assert(billsAtDestination.Contains(billCNT));
			Assert(!billsAtDestination.Contains(billEMP));
			Assert(billsAtDestination.Contains(billFCL));
			Assert(billsAtDestination.Contains(billFCX));
			Assert(!billsAtDestination.Contains(billFAK));
			Assert(!billsAtDestination.Contains(billFTL));
			Assert(!billsAtDestination.Contains(billGRP));
			Assert(!billsAtDestination.Contains(billLCL));
			Assert(!billsAtDestination.Contains(billLQD));
			Assert(!billsAtDestination.Contains(billLSE));
			Assert(!billsAtDestination.Contains(billLTL));
			Assert(!billsAtDestination.Contains(billMAI));
			Assert(!billsAtDestination.Contains(billNCT));
			Assert(!billsAtDestination.Contains(billOBC));
			Assert(!billsAtDestination.Contains(billOTH));
			Assert(!billsAtDestination.Contains(billROR));
			Assert(!billsAtDestination.Contains(billULD));
			Assert(!billsAtDestination.Contains(billUNA));
		}
	}
}
