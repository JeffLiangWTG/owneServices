using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class ExportInvoiceDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());

	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceDetailsControlBag.Instance;

		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

		builder.SetCaption(commonBag.ValuationCodeDropEdit, i => Enterprise.Customs.PL.GUI.Res.GetData("PLExportInvoiceDetailsLayout|ValuationCodeDropEdit",
			englishCaption: "[24] Transaction Nature", englishMediumCaption: "[24] Tran. Nature", englishShortCaption: "Tran. Nature",
			englishFullDescription: "The nature of the transaction."));

		builder.SetVisibility(euBag.AgreedPlaceCodeFindBox, invoice => invoice.AgreedPlaceCodeSupportAndVisible, invoice => invoice.JZ_IncoTermInfo);

		builder.AddControlBehaviour(commonBag.IncoTermsUserControl, new ZUserControlAllZDropEditSizeBehaviour());
		builder.AddControlBehaviour(commonBag.ValuationCodeDropEdit, new ZDropEditSizeBehaviour());
		builder.AddControlBehaviour(euBag.TransportChargesMethodOfPaymentDropEdit, new ZDropEditSizeBehaviour());

		return builder.Build();
	}
}
