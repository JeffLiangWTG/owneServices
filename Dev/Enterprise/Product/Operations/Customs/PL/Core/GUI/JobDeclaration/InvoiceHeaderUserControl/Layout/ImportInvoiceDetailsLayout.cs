using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class ImportInvoiceDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());

	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceDetailsControlBag.Instance;
		var plBag = ImportInvoiceDetailsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(plBag);

		builder.AddColumn();
		builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
		builder.Add(plBag.ValuationMethodDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(plBag.TranCircumstanceUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

		builder.SetCaption(commonBag.ValuationCodeDropEdit, i => Enterprise.Customs.PL.GUI.Res.GetData("PLImportInvoiceDetailsLayout|ValuationCodeDropEdit",
			englishCaption: "[8/5] Transaction Nature", englishMediumCaption: "[8/5] Tran. Nature", englishShortCaption: "Tran. Nature",
			englishFullDescription: "The nature of the transaction."));

		builder.SetVisibility(euBag.AgreedPlaceCodeFindBox, invoice => invoice.AgreedPlaceCodeSupportAndVisible, invoice => invoice.JZ_IncoTermInfo);

		builder.AddControlBehaviour(commonBag.IncoTermsUserControl, new ZUserControlAllZDropEditSizeBehaviour());
		builder.AddControlBehaviour(plBag.ValuationMethodDropEdit, new ZDropEditSizeBehaviour());
		builder.AddControlBehaviour(commonBag.ValuationCodeDropEdit, new ZDropEditSizeBehaviour());

		return builder.Build();
	}
}
