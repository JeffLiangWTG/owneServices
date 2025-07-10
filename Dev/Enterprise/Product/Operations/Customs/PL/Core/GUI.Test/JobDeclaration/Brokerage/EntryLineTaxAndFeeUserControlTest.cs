using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class EntryLineTaxAndFeeUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceType()
	{
		AssertEquals(typeof(Customs.Business.AllCusEntryLineCollection<CusEntryLine>), entryLineTaxAndFeeUserControl.BindingSource.DataSourceType);
	}

	public void TestEntryLineDutyAndTaxGrid_BaseAmount()
	{
		AssertColumnStyle<ZMultiControlColumnStyleInfo>(CusEntryLineFee.Schema.CF_BaseValueForDisplay, "Base Amount", 80);
	}

	public void TestEntryLineDutyAndTaxGrid_TaxRate()
	{
		var columnName = CusEntryLineFee.Schema.CF_RateForDisplay;
		AssertColumnStyle<ZMultiControlColumnStyleInfo>(columnName, "Tax Rate", 80);
		var columnStyle = (ZMultiControlColumnStyleInfo)entryLineTaxAndFeeUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);
		AssertEquals("TextAlign", HorizontalAlignment.Right, columnStyle.TextAlign);
	}

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
}
