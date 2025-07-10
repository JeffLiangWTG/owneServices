using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandedCostHistoryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckLH_LandedCostHistoryLineType()
		{
			var header = Factory.New<DummyLandedCostHeader>();
			header.SupportsNoCostApportionmentItem = true;
			var lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.DefaultFromHost(header);

			var landedCostHistory = lcHeader.Histories.AddNew();
			landedCostHistory.LH_LandedCostHistoryLineType = ZString.Empty;
			AssertHasErrorContaining(landedCostHistory.LH_LandedCostHistoryLineTypeInfo, MandatoryValidation.MustBeEntered);
			landedCostHistory.LH_LandedCostHistoryLineType = "XX";
			AssertHasErrorContaining(landedCostHistory.LH_LandedCostHistoryLineTypeInfo, ListValidation.InvalidCodeError);
			lcHeader.CostInputs.AddNew();
			landedCostHistory.LH_LandedCostHistoryLineType = LandedCostType.NoCostApportionmentItem;
			AssertHasWarning(landedCostHistory.LH_LandedCostHistoryLineTypeInfo, "All lines are marked as No Cost Apportionment but there are Transport Logistics Costs captured.");
			lcHeader.CostInputs.RemoveAndDeleteAll();
			landedCostHistory.LH_LandedCostHistoryLineType = LandedCostType.NoCostApportionmentItem;
			AssertNoWarning(landedCostHistory.LH_LandedCostHistoryLineTypeInfo, "All lines are marked as No Cost Apportionment but there are Transport Logistics Costs captured.");
		}

		public void TestValidateLH_OP()
		{
			var product = Factory.New<OrgSupplierPart>();
			var history = Factory.New<LandedCostHistory>();
			history.LH_OP = product.PK;
			AssertEquals("no error on active product", false, history.LH_OPInfo.HasErrors());
			AssertEquals("no warning on active product", false, history.LH_OPInfo.HasWarnings());
			history.LH_OP = ZGuid.Empty;
			AssertEquals("no error on empty product", false, history.LH_OPInfo.HasErrors());
			AssertEquals("no warning on empty product", false, history.LH_OPInfo.HasWarnings());
			product.OP_IsActive = false;
			history.LH_OP = product.PK;
			AssertEquals("no error on inactive product when LH is not in DB - have you removed [ReadOnly] from LH_OP?", false, history.LH_OPInfo.HasErrors());
			AssertEquals("HAS warning on active product", true, history.LH_OPInfo.HasWarning("This Product is inactive."));
		}
	}
}
