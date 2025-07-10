using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI;

[TestedType(typeof(InvoiceChargesGridColumnBag))]
sealed class InvoiceChargesGridColumnBagTest : TestCase
{
	public void TestChargeTypeDropEditColumn()
	{
		AssertNotNull(ColumnBag.ChargeTypeDropEditColumn);

		var columnInfo = ColumnBag.ChargeTypeDropEditColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZDropEditColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_ChargeType), columnInfo.ColumnName);
			AssertEquals("Width", 40, columnInfo.Width);
		});
	}

	public void TestAmountCalcEditColumn()
	{
		AssertNotNull(ColumnBag.AmountCalcEditColumn);

		var columnInfo = ColumnBag.AmountCalcEditColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_Amount), columnInfo.ColumnName);
			AssertEquals("Width", 80, columnInfo.Width);
			AssertEquals("GroupName.Caption", "Amount", columnInfo.GroupName.Caption);
		});
	}

	public void TestCurrencyCodeFindBoxColumn()
	{
		AssertNotNull(ColumnBag.CurrencyCodeFindBoxColumn);

		var columnInfo = ColumnBag.CurrencyCodeFindBoxColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_RX_NKCurrency), columnInfo.ColumnName);
			AssertEquals("Width", 40, columnInfo.Width);
			AssertEquals("GroupName.Caption", "Amount", columnInfo.GroupName.Caption);
		});
	}

	public void TestExchangeRateCalcEditColumn()
	{
		AssertNotNull(ColumnBag.ExchangeRateCalcEditColumn);

		var columnInfo = ColumnBag.ExchangeRateCalcEditColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_ExchangeRate), columnInfo.ColumnName);
			AssertEquals("Width", 80, columnInfo.Width);
		});
	}

	public void TestPercentageCalcEditColumn()
	{
		AssertNotNull(ColumnBag.PercentageCalcEditColumn);

		var columnInfo = ColumnBag.PercentageCalcEditColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_Percentage), columnInfo.ColumnName);
			AssertEquals("Width", 74, columnInfo.Width);
		});
	}

	public void TestDistributeByDropEditColumn()
	{
		AssertNotNull(ColumnBag.DistributeByDropEditColumn);

		var columnInfo = ColumnBag.DistributeByDropEditColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZDropEditColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_DistributeBy), columnInfo.ColumnName);
			AssertEquals("Width", 75, columnInfo.Width);
			AssertEquals("IsReadOnly", true, columnInfo.IsReadOnly);
		});
	}

	public void TestIsIncludedInLineCheckBoxColumn()
	{
		AssertNotNull(ColumnBag.IsIncludedInLineCheckBoxColumn);

		var columnInfo = ColumnBag.IsIncludedInLineCheckBoxColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZCheckBoxColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_IsIncludedInITOT), columnInfo.ColumnName);
			AssertEquals("Width", 85, columnInfo.Width);
		});
	}

	public void TestApportionedTypeDropEditColumn()
	{
		AssertNotNull(ColumnBag.ApportionedTypeDropEditColumn);

		var columnInfo = ColumnBag.ApportionedTypeDropEditColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZDropEditColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_FullOrPartialApportionment), columnInfo.ColumnName);
			AssertEquals("Width", 90, columnInfo.Width);
		});
	}

	public void TestIsStatisticalValueApplicableCheckBoxColumn()
	{
		AssertNotNull(ColumnBag.IsStatisticalValueApplicableCheckBoxColumn);

		var columnInfo = ColumnBag.IsStatisticalValueApplicableCheckBoxColumn.CreateGridColumnInfo();

		CombineAssertions(() =>
		{
			AssertType<ZCheckBoxColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", nameof(JobComInvCharge.J7_IsStatisticalValueApplicable), columnInfo.ColumnName);
			AssertEquals("Width", 100, columnInfo.Width);
		});
	}

	InvoiceChargesGridColumnBag ColumnBag => InvoiceChargesGridColumnBag.Instance;
}
