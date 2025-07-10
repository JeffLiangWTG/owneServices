using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonInvoiceLineDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout InvoiceLineDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => InvoiceLineDetails;

		public CommonInvoiceLineDetailsLayouts()
		{
			InvoiceLineDetails = CreateInvoiceLineDetailsLayout();
		}

		PanelLayout CreateInvoiceLineDetailsLayout()
		{
			var builder = new CommonInvoiceLineDetailsLayoutBuilder<BaseJobComInvoiceLine>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Medium);
			builder.Add(commonBag.ProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.TariffFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.PrimaryPreferenceDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.TaxTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Long);
			builder.Add(commonBag.BondedWHSOrderNumberTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
