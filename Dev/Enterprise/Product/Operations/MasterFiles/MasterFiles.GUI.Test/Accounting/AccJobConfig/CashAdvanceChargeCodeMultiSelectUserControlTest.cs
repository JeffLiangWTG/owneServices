using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CashAdvanceChargeCodeMultiSelectUserControl))]
	public class CashAdvanceChargeCodeMultiSelectUserControlTest : BasherTest
	{
		public void TestChargeCodeModuleButtonGridAndColumns()
		{
			using (var form = GetFormForTest())
			{
				form.Show();
				var grid = form.ChargeCodeMultiSelectUserControl_ForTestOnly.Grid_ForTestOnly.InnerGrid;

				AssertNotNull("Check DataSource is bounded successfully.", grid.List as AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection);
				Assert(grid.ReadOnly);

				var expectedListOfColumns = new[]
				{
					"AC_Code",
					"AC_Desc",
				};
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>()
					.Where(x => x.IsVisible)
					.Select(x => x.ColumnName)
					.ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		[RequiresSTA]
		public void TestChargeCodesAreAddedToCashAdvanceJobConfigurationChargeCodeCollectionWhenUserAttachChargeCodes()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			using (var form = GetFormForTest())
			{
				var configForTest = ((AccCashAdvanceDefaultingConfiguration)form.DataSource);

				form.Show();
				var grid = form.ChargeCodeMultiSelectUserControl_ForTestOnly.Grid_ForTestOnly;

				AssertEquals(1, configForTest.ChargeCodes.Count);
				Assert(!configForTest.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId).Contains(chargeCode1.PK));
				Assert(!configForTest.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId).Contains(chargeCode2.PK));

				form.ChargeCodeMultiSelectUserControl_ForTestOnly.ChargeCodeModuleButtonGrid_AttachedExposed(this, new ModuleButtonGridOnAttachEventArgs(new[] { chargeCode1, chargeCode2 }));

				AssertEquals(3, configForTest.ChargeCodes.Count);
				Assert(configForTest.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId).Contains(chargeCode1.PK));
				Assert(configForTest.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId).Contains(chargeCode2.PK));
			}
		}

		public void TestChargeCodesAreRemovedFromCashAdvanceJobConfigurationChargeCodeCollectionWhenUserDetachChargeCodes()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			using (var form = GetFormForTest())
			{
				var configForTest = ((AccCashAdvanceDefaultingConfiguration)form.DataSource);
				var defaultingChargeCode1 = Factory.New<CashAdvanceDefaultingChargeCode>();
				defaultingChargeCode1.JCT_ParentId = chargeCode1.PK;
				defaultingChargeCode1.JCT_JCF_JobConfig = configForTest.PK;
				var defaultingChargeCode2 = Factory.New<CashAdvanceDefaultingChargeCode>();
				defaultingChargeCode2.JCT_ParentId = chargeCode2.PK;
				defaultingChargeCode2.JCT_JCF_JobConfig = configForTest.PK;

				form.Show();

				configForTest.ChargeCodes.Add(defaultingChargeCode1);
				configForTest.ChargeCodes.Add(defaultingChargeCode2);

				AssertEquals(3, configForTest.ChargeCodes.Count);
				Assert(configForTest.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId).Contains(chargeCode1.PK));
				Assert(configForTest.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId).Contains(chargeCode2.PK));

				form.ChargeCodeMultiSelectUserControl_ForTestOnly.ChargeCodeModuleButtonGrid_DetachedExposed(this, new ModuleButtonGridOnDetachedEventArgs(new[] { chargeCode1, chargeCode2 }));

				AssertEquals(1, configForTest.ChargeCodes.Count);
				Assert(!configForTest.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId).Contains(chargeCode1.PK));
				Assert(!configForTest.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId).Contains(chargeCode2.PK));
			}
		}

		public override Form GetFormToBash() => GetFormForTest();

		DummyForm GetFormForTest()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;
			var chargeCodePivot = config.ChargeCodes.AddNew();
			chargeCodePivot.JCT_ParentId = chargeCode.PK;
			Factory.Save();

			return new DummyForm(config) { CaptionRenderingEnabled = true };
		}

		class DummyForm : ZChildForm
		{
			public DummyForm(IBusiness businessEntity) : base(businessEntity)
			{
				ChargeCodeMultiSelectUserControl_ForTestOnly = new CashAdvanceChargeCodeMultiSelectUserControlForTest()
				{
					Dock = DockStyle.Fill
				};
				Controls.Add(ChargeCodeMultiSelectUserControl_ForTestOnly);
				ChargeCodeMultiSelectUserControl_ForTestOnly.Bind(businessEntity as AccCashAdvanceDefaultingConfiguration);
			}

			public CashAdvanceChargeCodeMultiSelectUserControlForTest ChargeCodeMultiSelectUserControl_ForTestOnly { get; }
		}

		class CashAdvanceChargeCodeMultiSelectUserControlForTest : CashAdvanceChargeCodeMultiSelectUserControl
		{
			public ZModuleButtonGridForDesigner Grid_ForTestOnly => Controls.Find("ChargeCodeModuleButtonGrid", false).FirstOrDefault() as ZModuleButtonGridForDesigner;

			public void ChargeCodeModuleButtonGrid_AttachedExposed(object sender, ModuleButtonGridOnAttachEventArgs e) => ChargeCodeModuleButtonGrid_Attached(sender, e);

			public void ChargeCodeModuleButtonGrid_DetachedExposed(object sender, ModuleButtonGridOnDetachedEventArgs e) => ChargeCodeModuleButtonGrid_Detached(sender, e);
		}
	}
}
