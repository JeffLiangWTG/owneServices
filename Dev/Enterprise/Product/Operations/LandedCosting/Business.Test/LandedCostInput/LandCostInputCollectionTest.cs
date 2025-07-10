using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandCostInputCollection))]
	sealed class LandCostInputCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDeleteSystemDefaultedRowsOnly()
		{
			LandCostInput lCInput1 = Header.CostInputs.AddNew();
			lCInput1.LI_IsUserEntered = true;
			LandCostInput lCInput2 = Header.CostInputs.AddNew();
			lCInput2.LI_IsUserEntered = false;

			Header.CostInputs.DeleteSystemDefaultedRowsOnly();
			AssertEquals("only 1 row left", 1, Header.CostInputs.Count);
			AssertEquals("The row left is user-entered", true, Header.CostInputs[0].LI_IsUserEntered);
		}

		public void TestSetDefaultValues()
		{
			LandCostInput lCInput = Header.CostInputs.AddNew();
			AssertEquals("Default values", true, lCInput.LI_IsUserEntered);
		}

		public void TestGetLCInputsToDistributeTo()
		{
			DummyLandedCostHeader dummy = Factory.New<DummyLandedCostHeader>();
			Header.LT_ParentID = dummy.PK;
			Header.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			DummyLandedCostDistributeTo dummyDistribute = Factory.New<DummyLandedCostDistributeTo>();
			dummy.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { dummyDistribute };

			AssertEquals("No LC inputs with this distribute to", 0, Header.CostInputs.GetLCInputsToDistributeTo(dummyDistribute).Length);

			LandCostInput lCInput = Header.CostInputs.AddNew();
			AssertEquals("No LC inputs with this distribute to", 0, Header.CostInputs.GetLCInputsToDistributeTo(dummyDistribute).Length);

			lCInput.LI_ParentID = dummyDistribute.PK;
			lCInput.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			AssertEquals("One LC input with this distribute to", 1, Header.CostInputs.GetLCInputsToDistributeTo(dummyDistribute).Length);
		}

		public void TestTotals()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandCostInput costInput1 = lCHeader.CostInputs.AddNew();
			costInput1.LI_LandedCostGroup = 1;
			costInput1.LI_CostAmount = 100;
			costInput1.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput costInput2 = lCHeader.CostInputs.AddNew();
			costInput2.LI_LandedCostGroup = 2;
			costInput2.LI_CostAmount = 200;
			costInput2.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput costInput3 = lCHeader.CostInputs.AddNew();
			costInput3.LI_LandedCostGroup = 3;
			costInput3.LI_CostAmount = 300;
			costInput3.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput costInput4 = lCHeader.CostInputs.AddNew();
			costInput4.LI_LandedCostGroup = 4;
			costInput4.LI_CostAmount = 400;
			costInput4.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput costInput5 = lCHeader.CostInputs.AddNew();
			costInput5.LI_LandedCostGroup = 5;
			costInput5.LI_CostAmount = 500;
			costInput5.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput costInput6 = lCHeader.CostInputs.AddNew();
			costInput6.LI_LandedCostGroup = 6;
			costInput6.LI_CostAmount = 600;
			costInput6.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput costInput7 = lCHeader.CostInputs.AddNew();
			costInput7.LI_LandedCostGroup = 7;
			costInput7.LI_CostAmount = 700;
			costInput7.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertEquals("Total amount", 2800m, lCHeader.CostInputs.TotalLandingCost);
			AssertEquals("TotalGroup1", 100m, lCHeader.CostInputs.TotalGroup1);
			AssertEquals("TotalGroup2", 200m, lCHeader.CostInputs.TotalGroup2);
			AssertEquals("TotalGroup3", 300m, lCHeader.CostInputs.TotalGroup3);
			AssertEquals("TotalGroup4", 400m, lCHeader.CostInputs.TotalGroup4);
			AssertEquals("TotalGroup5", 500m, lCHeader.CostInputs.TotalGroup5);
			AssertEquals("TotalGroup6", 600m, lCHeader.CostInputs.TotalGroup6);
			AssertEquals("TotalGroupMisc", 700m, lCHeader.CostInputs.TotalGroupMisc);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new LandCostInputCollection(Header);

		LandedCostHeader header;
		LandedCostHeader Header => header ?? (header = Factory.New<LandedCostHeader>());
	}
}
