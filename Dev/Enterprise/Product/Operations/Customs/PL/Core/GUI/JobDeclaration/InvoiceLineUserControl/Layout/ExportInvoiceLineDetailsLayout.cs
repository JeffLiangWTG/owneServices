using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout => CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(euBag);
		var plBag = InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(plBag);

		builder.AddColumn();
		builder.Add(commonBag.UnicodeDescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TariffFindBox, ControlWidthClass.Auto);

		builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
		builder.Add(plBag.CPCUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
		builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto, commonBag.InvoiceQuantityCalcDropEdit);
		builder.Add(euBag.DestinationCodeFindBox, ControlWidthClass.Long, commonBag.CountryOfOriginDropEdit);
		builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto, euBag.AdditionalProcedureCodesUserControl);
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);

		builder.SetCaption(commonBag.CustomsSecondQuantityCalcDropEdit, LayoutHelper.GetSecondQuantityCalcDropEditCaption, invoiceLine => invoiceLine.JI_TariffInfo);
		return builder.Build();
	}
}
