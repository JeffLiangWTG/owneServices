using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class EntryLineTaxAndFeeUserControlTest : TestCaseWithFactory, System.IDisposable
{
	public void TestEntryLineDutyAndTaxGroupBoxCaption()
	{
		AssertEquals("Duty And Tax", entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString.Caption);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_ChargeTypeGroup()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		using (var form = new ZForm(entryHeader.AllEntryLines))
		{
			form.Controls.Add(entryLineTaxAndFeeUserControl);
			form.Show();
			var dutyAndTaxGrid = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Type Group", "0FDFDE61-90C5-45A9-A522-B1B7419AE2BB", dutyAndTaxGrid.Columns[Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType].GroupName.Key);
				AssertEquals("Charge Description Group", "0FDFDE61-90C5-45A9-A522-B1B7419AE2BB", dutyAndTaxGrid.Columns[nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription)].GroupName.Key);
			});
		}
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_Type()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeType;
		var columnStyle = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 100, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_TypeDescription()
	{
		var columnName = nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription);
		AssertColumnStyle<ZTextBoxColumnStyleInfo>(columnName, "Description", 150);
		var columnStyle = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("IsReadOnly", true, columnStyle.IsReadOnly);
		AssertEquals("Width", 150, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_Action()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_RateOverrideReasonCode;
		var columnStyle = (ZDropEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 140, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_BaseAmount()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_BaseValue;
		var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 80, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfCalculation()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfCalculation;
		var columnStyle = (ZDropEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 150, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_TaxRate()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_Rate;
		var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		AssertEquals("Width", 80, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_TotalAmount()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_ChargeAmount;
		var columnStyle = (ZCalcEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
		AssertEquals("Width", 120, columnStyle.Width);
	}

	public void TestEntryLineTaxAndConfirmedFeeUserControl_MethodOfPayment()
	{
		var columnName = Customs.Business.AutoCusEntryLineFee.Schema.CF_MethodOfPayment;
		var columnStyle = (ZDropEditColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("Width", 120, columnStyle.Width);
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryLineTaxAndFeeUserControl = new EntryLineTaxAndFeeUserControl();
	}
	EntryLineTaxAndFeeUserControl entryLineTaxAndFeeUserControl;

	protected override void TearDown()
	{
		base.TearDown();
		entryLineTaxAndFeeUserControl?.Dispose();
	}

	void AssertColumnStyle<T>(string columnName, string caption, int width) where T : ZGridColumnInfo
	{
		var columnStyle = entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);

		CombineAssertions(() =>
		{
			AssertType<T>(columnStyle);
			AssertEquals("Caption", caption, columnStyle.CaptionResourceString.Caption);
			AssertEquals("Width", width, columnStyle.Width);
		});
	}

	public void Dispose()
	{
		entryLineTaxAndFeeUserControl?.Dispose();
	}
}
