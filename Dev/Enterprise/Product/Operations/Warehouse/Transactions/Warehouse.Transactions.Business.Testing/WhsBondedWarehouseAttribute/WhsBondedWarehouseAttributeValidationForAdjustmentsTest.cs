using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBondedWarehouseAttributeValidationForAdjustmentsTest : WhsBondedWarehouseAttributeValidationTest
	{
		#region TestCheckWB_EntryDate

		[TestDate(2019, 10, 10)]
		public void TestCheckWB_EntryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = "CUS";

			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			var bond = adjustmentLine.CustomsData;
			Factory.Save();

			var oldDate = new ZDateTime(2000, 01, 01);
			bond.WB_EntryDate = oldDate;
			AssertNoErrors("Should *not* show errors for dates in the past.", bond.WB_EntryDateInfo);

			adjustmentLine.WE_TransactionQuantity = -10m;
			bond.Validation.ValidateWB_EntryDate();
			AssertNoErrors("Should *not* show errors for dates in the past.", bond.WB_EntryDateInfo);

			// Doesnt make sense to adjust in a future date so we will retain this validation
			bond.WB_EntryDate = ZDateTime.Now.AddYears(5).AddDays(1);
			AssertHasErrors("Should show errors for future dates.", bond.WB_EntryDateInfo);

			adjustmentLine.WE_TransactionQuantity = 10m;
			bond.Validation.ValidateWB_EntryDate();
			AssertHasErrors("Should show errors for future dates.", bond.WB_EntryDateInfo);
		}

		#endregion

		#region TestCheckWB_EntryKey_WhenAdjustmentLineHasBondedEntryKey

		public void TestCheckWB_EntryKey_WhenAdjustmentLineHasBondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = "CUS";
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.SetParent(adjustmentLine);
			AssertNoErrors(bond.WB_EntryKeyInfo);

			bond.Validation.ValidateWB_EntryKey();
			AssertHasError(bond.WB_EntryKeyInfo, "Entry Number is mandatory for Customs Jobs.");

			adjustmentLine.WE_BondedEntryKey = "123-1";
			bond.Validation.ValidateWB_EntryKey();
			AssertNoErrors(bond.WB_EntryKeyInfo);
		}

		#endregion

		#region Implementation

		protected override WhsDocketLine GetDocketLineParent(TestDataSimpleEnvironment data, WhsWarehouse whsOverride = null)
		{
			var whs = whsOverride ?? data.Whs1;
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, whs);
			adjustment.WD_DocketSubType = "CUS";
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = whs.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;

			return Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, whs.DefaultLocation);
		}

		protected override bool IsNegativeValueAllowedForAdjustments
		{
			get { return true; }
		}

		protected override WhsBondedWarehouseAttribute GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject();
			var docket = Factory.New<WhsAdjustment>();
			docket.WD_DocketSubType = CodeLists.AdjustmentType.Codes.Customs;
			result.WB_ParentID = docket.Lines.AddNew().PK;
			return result;
		}

		#endregion
	}
}
