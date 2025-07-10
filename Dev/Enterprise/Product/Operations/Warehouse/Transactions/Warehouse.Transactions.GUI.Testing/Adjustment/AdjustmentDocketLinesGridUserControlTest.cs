using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class AdjustmentDocketLinesGridUserControlTest : DocketLinesGridUserControlTest<AdjustmentDocketLinesGridUserControl>
	{
		#region TestColumnLayoutContextForLinesGrid

		public void TestColumnLayoutContextForLinesGrid()
		{
			using (var control = new AdjustmentDocketLinesGridUserControl())
			{
				AssertEquals("layout context is defined", nameof(DocketLinesGridContext.Adjustment), control.LinesGrid.ColumnLayoutContext);
			}
		}

		#endregion

		#region Drag-n-Drop

		public void TestOnDragDrop()
		{
			var adjustment = (WhsAdjustment)GetNewDocket();
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (var form = GetNewDocketLinesTestForm(adjustment))
			{
				form.Show();
				AssertEquals("Precondition no lines in adjustment", 0, adjustment.Lines.Count);

				adjustment.WD_WW_Whs = data.Whs1.PK;
				adjustment.WD_OH_Client = data.Org1.PK;
				AssertEquals("Precondition collection allows new", true, ((IBindingList)adjustment.Lines).AllowNew);

				form.UserControl.LinesGridDragAndDropManagerForTesting.OnDragDropCore(new BusinessObject[] { data.Line111 });
				AssertEquals("New line should be generated for adjustment", 1, adjustment.Lines.Count);
			}
		}

		#endregion

		#region TestLocationAutoComplete

		public void TestLocationAutoCompleteDisabled()
		{
			using (var control = new AdjustmentDocketLinesGridUserControl())
			{
				var style = (ZCodeFindBoxColumnStyleInfo)control.LinesGrid.GetColumnStyle("LocationString");
				Assert("Location style should have auto complete disabled", style.AutoCompleteDisabled);
			}
		}

		public void TestNonLocationAutoCompleteEnabled()
		{
			using (var control = new AdjustmentDocketLinesGridUserControl())
			{
				var productStyle = (ZGuidFindBoxColumnStyleInfo)control.LinesGrid.GetColumnStyle("WE_OP");
				Assert("Product style should have auto complete enabled", !productStyle.AutoCompleteDisabled);

				var commodityStyle = (ZCodeFindBoxColumnStyleInfo)control.LinesGrid.GetColumnStyle("CommodityCode");
				Assert("Commodity style should have auto complete enabled", !commodityStyle.AutoCompleteDisabled);
			}
		}

		#endregion

		#region TestCustomsInfoColumnsVisibility

		public void TestCustomsInfoColumnsVisibility_BondedAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;

			using (var form = GetNewDocketLinesTestForm(adjustment))
			{
				form.Show();
				var control = form.UserControl;
				AssertCustomsInfoColumnsVisibility(control, true);
			}
		}

		public void TestCustomsInfoColumnsVisibility_NonBondedAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;

			using (var form = GetNewDocketLinesTestForm(adjustment))
			{
				form.Show();
				var control = form.UserControl;
				AssertCustomsInfoColumnsVisibility(control, false);
			}
		}

		public void TestCustomsInfoColumnsVisibility_NonBondedAdjustmentWithBondedEnabledWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;

			using (var form = GetNewDocketLinesTestForm(adjustment))
			{
				form.Show();
				var control = form.UserControl;
				AssertCustomsInfoColumnsVisibility(control, false);
			}
		}

		public void TestCustomsInfoColumnsVisibility_NonBondedAdjustmentWithExciseEnabledWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForExcise(data.Whs1, true);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;

			using (var form = GetNewDocketLinesTestForm(adjustment))
			{
				form.Show();
				var control = form.UserControl;
				AssertCustomsInfoColumnsVisibility(control, false);
			}
		}

		void AssertCustomsInfoColumnsVisibility(AdjustmentDocketLinesGridUserControl control, bool isAvailable)
		{
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsTariffLookup").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsTariffItem").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsTariffDesc").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_EntryKey").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_EntryLineNo").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_EntryDate").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_DeclarationReference").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_CustomsDeadline").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_InwardStyle").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_InwardProcedure").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_RN_NKCountryOfOrigin").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_CustomsQty").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_CustomsUnitOfQty").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_ValueForDuty").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_TILV").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_AddInfo").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_CustomsSecondQuantity").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_CustomsSecondUnitQty").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_Tariff").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_PrimaryPreference").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_CustomsThirdQuantity").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_CustomsThirdUnitQty").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+ManufacturerCode").IsUnavailable);
			AssertEquals(!isAvailable, control.LinesGrid.GetColumnStyle("CustomsData+WB_OA_ManufacturerAddress").IsUnavailable);
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsAdjustment>();
		}

		protected override AdjustmentDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
		{
			return new AdjustmentDocketLinesGridUserControl();
		}

		protected override DocketLinesTestForm GetNewDocketLinesTestForm(WhsDocket docket)
		{
			return new AdjustmentLinesTestForm(docket);
		}

		protected class AdjustmentLinesTestForm : DocketLinesTestForm
		{
			public AdjustmentLinesTestForm(WhsDocket docket)
				: base(docket)
			{
			}

			protected override AdjustmentDocketLinesGridUserControl GetNewDocketLinesGridUserControl()
			{
				return new AdjustmentDocketLinesGridUserControl();
			}
		}

		protected override void SetTransactionAsBondedCore(WhsDocket docket)
		{
			base.SetTransactionAsBondedCore(docket);
			docket.WD_DocketSubType = AdjustmentType.Codes.Customs;
		}

		#endregion
	}
}
