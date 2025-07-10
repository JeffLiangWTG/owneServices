using System;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI;

public sealed class InvoiceChargesGridColumnBag
{
	public static InvoiceChargesGridColumnBag Instance => instance ??= new InvoiceChargesGridColumnBag();

	[ThreadStatic]
	static InvoiceChargesGridColumnBag instance;

	InvoiceChargesGridColumnBag()
	{
		ChargeTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(JobComInvCharge.Schema.J7_ChargeType, 40);
		var amountGroupName = Res.GetData("EC526494-38EA-4C85-9E96-4600E3EC8C03", "Amount");
		AmountCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(JobComInvCharge.Schema.J7_Amount, 80, c =>
		{
			c.GroupName = amountGroupName;
		});
		CurrencyCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(JobComInvCharge.Schema.J7_RX_NKCurrency, 40, c =>
		{
			c.GroupName = amountGroupName;
		});
		ExchangeRateCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(JobComInvCharge.Schema.J7_ExchangeRate, 80);
		PercentageCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(JobComInvCharge.Schema.J7_Percentage, 74);
		DistributeByDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(JobComInvCharge.Schema.J7_DistributeBy, 75, c =>
		{
			c.IsReadOnly = true;
		});
		IsIncludedInLineCheckBoxColumn = new GridColumnReference<ZCheckBoxColumnStyleInfo>(JobComInvCharge.Schema.J7_IsIncludedInITOT, 85);
		ApportionedTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(JobComInvCharge.Schema.J7_FullOrPartialApportionment, 90);
		IsStatisticalValueApplicableCheckBoxColumn = new GridColumnReference<ZCheckBoxColumnStyleInfo>(JobComInvCharge.Schema.J7_IsStatisticalValueApplicable, 100);
	}

	public IGridColumnReference ChargeTypeDropEditColumn { get; }
	public IGridColumnReference AmountCalcEditColumn { get; }
	public IGridColumnReference CurrencyCodeFindBoxColumn { get; }
	public IGridColumnReference ExchangeRateCalcEditColumn { get; }
	public IGridColumnReference PercentageCalcEditColumn { get; }
	public IGridColumnReference DistributeByDropEditColumn { get; }
	public IGridColumnReference IsIncludedInLineCheckBoxColumn { get; }
	public IGridColumnReference ApportionedTypeDropEditColumn { get; }
	public IGridColumnReference IsStatisticalValueApplicableCheckBoxColumn { get; }
}
