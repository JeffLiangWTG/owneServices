using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.CommercialInvoice
{
	public sealed class InvoiceHeaderDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();
		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<BaseJobComInvoiceHeader>();
			var bag = InvoiceHeaderDetailsControlBag.Instance;

			builder.AddControlBag(bag);

			builder.AddColumn();
			builder.Add(bag.InvoiceNumberBoundTextBox, ControlWidthClass.Auto);
			builder.Add(bag.InvoiceDateEdit, ControlWidthClass.Auto);
			builder.Add(bag.InvoiceAmountCalcFindBox, ControlWidthClass.Auto);
			builder.Add(bag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(bag.IncotermAndIncotermPlaceUserControl, ControlWidthClass.Auto);
			builder.Add(bag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(bag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
