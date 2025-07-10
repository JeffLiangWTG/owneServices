using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

sealed class ExportInvoiceChargesGridColumLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var commonControlBag = Enterprise.Customs.GUI.InvoiceChargesGridColumnBag.Instance;
		var euControlBag = Enterprise.Customs.EU.GUI.InvoiceChargesGridColumnBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(commonControlBag.ChargeTypeDropEditColumn);
		builder.AddColumn(commonControlBag.AmountCalcEditColumn);
		builder.AddColumn(commonControlBag.CurrencyCodeFindBoxColumn);
		builder.AddColumn(commonControlBag.ExchangeRateCalcEditColumn);
		builder.AddColumn(euControlBag.AmountInLocalCurrencyCalcEditColumn);
		builder.AddColumn(commonControlBag.PercentageCalcEditColumn);
		builder.AddColumn(commonControlBag.DistributeByDropEditColumn);
		builder.AddColumn(commonControlBag.IsIncludedInLineCheckBoxColumn);
		builder.AddColumn(commonControlBag.ApportionedTypeDropEditColumn);
		builder.AddColumn(commonControlBag.IsStatisticalValueApplicableCheckBoxColumn);
		builder.AddColumn(euControlBag.AmountCorrectionCalcEditColumn);
		return builder.Build();
	}
}
