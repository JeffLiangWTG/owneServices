using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class EntryLineTaxAndFeeUserControlTest : TestCaseWithFactory
	{
		public void TestNationalFeeTypeCode_IsAvailable_IsReadonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			using (var form = new ZForm(entryheader.AllEntryLines))
			{
				form.Controls.Add(entryLineTaxAndFeeUserControl);
				form.Show();
				var feesGrid = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid;
				var column = feesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x =>
					!x.IsUnavailable && x.ColumnName == CusEntryLineFee.Schema.NationalFeeTypeCode);
				AssertNotNull("Column is available", column);
				CombineAssertions(() =>
				{
					AssertEquals("Column is readonly", false, column.IsReadOnly);
					AssertEquals("Column not visible by default", true, column.IsVisible);
				});
			}
		}

		public void TestEntryLineDutyAndTaxGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			using (var form = new ZForm(entryheader.AllEntryLines))
			{
				form.Controls.Add(entryLineTaxAndFeeUserControl);
				form.Show();
				var feesGrid = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid;

				CombineAssertions(() =>
				{
					AssertNotNull("CF_ChargeType column", feesGrid.GetColumnStyle("CF_ChargeType"));
					AssertNotNull("NationalFeeTypeCode column", feesGrid.GetColumnStyle("NationalFeeTypeCode"));
					AssertNotNull("NationalFeeTypeCodeDescription column", feesGrid.GetColumnStyle("NationalFeeTypeCodeDescription"));
					AssertNotNull("CF_BaseValue column", feesGrid.GetColumnStyle("CF_BaseValue"));
					AssertNotNull("CF_MethodOfCalculation column", feesGrid.GetColumnStyle("CF_MethodOfCalculation"));
					AssertNotNull("CF_Rate column", feesGrid.GetColumnStyle("CF_Rate"));
					AssertNotNull("CF_ChargeAmount column", feesGrid.GetColumnStyle("CF_ChargeAmount"));
					AssertNotNull("CF_MethodOfPayment column", feesGrid.GetColumnStyle("CF_MethodOfPayment"));
				});
			}
		}

		public void TestVisibleAndColumnsOrder()
		{
			for (int i = 0; i < ExpectedColumnOrderList.Count; i++)
			{
				ZGridColumnInfo columnInfo = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles[i] as ZGridColumnInfo;
				AssertNotNull(columnInfo);
				ZString expectedColumnName = ExpectedColumnOrderList[i];
				AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
				AssertEquals("Visible", true, columnInfo.IsVisible);
			}
		}

		List<string> ExpectedColumnOrderList
		{
			get
			{
				if (fExpectedColumnOrderList == null)
				{
					fExpectedColumnOrderList = new List<string>();
					fExpectedColumnOrderList.Add(CusEntryLineFee.Schema.NationalFeeTypeCode);
					fExpectedColumnOrderList.Add(nameof(CusEntryLineFee.NationalFeeTypeCodeDescription));
					fExpectedColumnOrderList.Add(CusEntryLineFee.Schema.CF_BaseValue);
					fExpectedColumnOrderList.Add(CusEntryLineFee.Schema.CF_MethodOfCalculation);
					fExpectedColumnOrderList.Add(CusEntryLineFee.Schema.CF_Rate);
					fExpectedColumnOrderList.Add(CusEntryLineFee.Schema.CF_ChargeAmount);
					fExpectedColumnOrderList.Add(CusEntryLineFee.Schema.CF_MethodOfPayment);
				}
				return fExpectedColumnOrderList;
			}
		}
		List<string> fExpectedColumnOrderList;

		protected override void SetUp()
		{
			base.SetUp();
			entryLineTaxAndFeeUserControl = new EntryLineTaxAndFeeUserControl();
		}
		EntryLineTaxAndFeeUserControl entryLineTaxAndFeeUserControl;

		protected override void TearDown()
		{
			entryLineTaxAndFeeUserControl?.Dispose();
			base.TearDown();
		}
	}
}
