using CargoWise.EntityFramework.Testing;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.GUI.Testing
{
	sealed class LandCostHistoryUserControlTest : TestCaseWithFactory
	{
#if !WINZOR
		public void TestCustomiseColumnsOfGridOnLCHistoryMasterSet()
		{
			var lCHeader = Factory.New<LandedCostHeader>();
			using (var testForm = new ZForm(lCHeader))
			using (var testControl = new LandCostHistoryUserControl())
			{
				testForm.Controls.Add(testControl);
				testControl.SetBindTo("Histories");
				testForm.Show();
				testControl.SetDataBinding(lCHeader, "");
				testControl.LCHistoryMaster = lCHeader;

				AssertEquals("PreCondition: LCHeader ShouldMarginPercentagesReadOnly", false, ((ILandedCostHistoryMaster)lCHeader).ShouldMarginPercentagesReadOnly);
				AssertEquals("ReadOnly MarginPercent1", false, testControl.LCHistoryGrid.Columns[LandedCostHistorySchema.LH_LandedCostMarginPercent1.Name].ColumnStyle.ReadOnly);
				AssertEquals("ReadOnly MarginPercent2", false, testControl.LCHistoryGrid.Columns[LandedCostHistorySchema.LH_LandedCostMarginPercent2.Name].ColumnStyle.ReadOnly);
				AssertEquals("ReadOnly MarginPercent3", false, testControl.LCHistoryGrid.Columns[LandedCostHistorySchema.LH_LandedCostMarginPercent3.Name].ColumnStyle.ReadOnly);
			}

			var product = Factory.New<OrgSupplierPart>();
			var testCollection = new GenericLandedCostHistoryCollection(product);
			using (var testForm = new ZForm(testCollection))
			using (var testControl = new LandCostHistoryUserControl())
			{
				testForm.Controls.Add(testControl);
				testForm.Show();
				testControl.SetDataBinding(testCollection, "");
				testControl.LCHistoryMaster = product;
				AssertEquals("PreCondition: Product ShouldMarginPercentagesReadOnly", true, ((ILandedCostHistoryMaster)product).ShouldMarginPercentagesReadOnly);
				AssertEquals("ReadOnly MarginPercent1", true, testControl.LCHistoryGrid.Columns[LandedCostHistorySchema.LH_LandedCostMarginPercent1.Name].ColumnStyle.ReadOnly);
				AssertEquals("ReadOnly MarginPercent2", true, testControl.LCHistoryGrid.Columns[LandedCostHistorySchema.LH_LandedCostMarginPercent2.Name].ColumnStyle.ReadOnly);
				AssertEquals("ReadOnly MarginPercent3", true, testControl.LCHistoryGrid.Columns[LandedCostHistorySchema.LH_LandedCostMarginPercent3.Name].ColumnStyle.ReadOnly);
			}
		}
#endif

		public void TestVisibilityOfColumns()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var zaCompany = Factory.New<GlbCompany>();
			zaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_GC = nzCompany.PK;
			using (var testForm = new ZForm(lCHeader))
			using (var testControl = new LandCostHistoryUserControl())
			{
				testForm.Controls.Add(testControl);
				testControl.SetBindTo("Histories");
				testControl.LCHistoryMaster = lCHeader;
				testForm.Show();
				testControl.SetDataBinding(lCHeader, "");

				AssertEquals("Visible Total Duty", true, testControl.LCHistoryGrid.Columns.Contains("TDT"));
				AssertEquals("Visible Excise", false, testControl.LCHistoryGrid.Columns.Contains("EXC"));
				AssertEquals("Visible EntryFees", true, testControl.LCHistoryGrid.Columns.Contains("ENT"));
				AssertEquals("Visible DutyPercent", true, testControl.LCHistoryGrid.Columns.Contains(LandedCostHistory.Schema.LH_DutyPercent));
				AssertEquals("Visible SpecialTax1", true, testControl.LCHistoryGrid.Columns.Contains("ST1"));
				AssertEquals("Visible SpecialTax2", true, testControl.LCHistoryGrid.Columns.Contains("ST2"));
				AssertEquals("Visible SpecialTax3", true, testControl.LCHistoryGrid.Columns.Contains("ST3"));
				AssertEquals("Visible OtherOrFlatDutyAmount", true, testControl.LCHistoryGrid.Columns.Contains("OTH"));
			}

			lCHeader.LT_GC = zaCompany.PK;
			using (var testForm = new ZForm(lCHeader))
			using (var testControl = new LandCostHistoryUserControl())
			{
				testForm.Controls.Add(testControl);
				testControl.SetBindTo("Histories");
				testControl.LCHistoryMaster = lCHeader;
				testForm.Show();
				testControl.SetDataBinding(lCHeader, "");

				AssertEquals("Visible Total Duty", true, testControl.LCHistoryGrid.Columns.Contains("TDT"));
				AssertEquals("Visible Excise", false, testControl.LCHistoryGrid.Columns.Contains("EXC"));
				AssertEquals("Visible EntryFees", false, testControl.LCHistoryGrid.Columns.Contains("ENT"));
				AssertEquals("Visible EntryFees", false, testControl.LCHistoryGrid.Columns.Contains(LandedCostHistory.Schema.LH_DutyPercent));
				AssertEquals("Visible SpecialTax1", false, testControl.LCHistoryGrid.Columns.Contains("ST1"));
				AssertEquals("Visible SpecialTax2", false, testControl.LCHistoryGrid.Columns.Contains("ST2"));
				AssertEquals("Visible SpecialTax3", false, testControl.LCHistoryGrid.Columns.Contains("ST3"));
				AssertEquals("Visible OtherOrFlatDutyAmount", false, testControl.LCHistoryGrid.Columns.Contains("OTH"));
			}
		}
	}
}
