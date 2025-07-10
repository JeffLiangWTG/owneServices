using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class ImportInvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new ImportInvoiceLineDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(euBag);
			var trBag = InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(trBag);

			builder.AddColumn();
			builder.Add(trBag.PartNoCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
			builder.Add(euBag.SupplementaryCode1AndGDMUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.TaxTypeDropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
			builder.Add(euBag.CommercialPaymentCodeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.UnicodeDescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.PreferenceCodeDropEdit, ControlWidthClass.Long);
			builder.Add(trBag.BrandNameTextBox, ControlWidthClass.Long);
			builder.Add(euBag.ValuationCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ManufacturerAddressControl, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsFifthQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.PriceTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.UsedGoodsCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.ReturnToOriginCheckBox, ControlWidthClass.Long);
			builder.Add(euBag.SecondaryTreatedProductCheckBox, ControlWidthClass.Long);
			builder.Add(euBag.InwardProcessingLicenseLineNumberTextBox, ControlWidthClass.Long);
			builder.Add(euBag.ReturningGoodsReasonCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.ReturningGoodsReasonDetailTextBox, ControlWidthClass.Long);
			builder.Add(euBag.EntryExitPurposeCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.EntryExitPurposeDetailTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(euBag.QuotaDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.BorderTradeStateCodeFindBox, ControlWidthClass.Medium);
			builder.Add(euBag.ExcessStockCheckBox, ControlWidthClass.Auto);
			builder.Add(commonBag.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);
			builder.Add(euBag.ProcessingDescriptionLongTextControl, ControlWidthClass.Medium);

			builder.SetVisibility(euBag.ReturningGoodsReasonDetailTextBox, b => b.ReturningGoodsReasonCodeVisibility, b => b.ZG_ReturningGoodsReasonCodeInfo);

			builder.SetVisibility(commonBag.PreviousEntryNumberTextBox, b => b.IsPreviousEntryAvailable, b => b.JI_FormattedProcedureInfo);
			builder.SetVisibility(commonBag.PreviousEntryLineNumberCalcEdit, b => b.IsPreviousEntryAvailable, b => b.JI_FormattedProcedureInfo);
			builder.SetVisibility(euBag.ProcessingDescriptionLongTextControl, b => b.IsPreviousEntryAvailable, b => b.JI_FormattedProcedureInfo);
			builder.SetVisibility(euBag.EntryExitPurposeCodeDropEdit, b => b.IsEntryExitPurposeCodeVisibility, b => b.JI_FormattedProcedureInfo);
			builder.SetVisibility(euBag.EntryExitPurposeDetailTextBox, b => b.IsEntryExitPurposeDetailVisibility, b => b.ZG_EntryExitPurposeCodeInfo);

			builder.SetCaption(euBag.AdditionalSupplementaryCodesUserControl, h => Res.GetData("DCA63925-5CD5-44F9-A1AA-ACC363CAE2F2", "Exemption Codes"));
			builder.SetCaption(euBag.PreferenceCodeDropEdit, h => Res.GetData("F7D212AA-9CDB-4ED9-8924-E3786D5F0FE9", "[36] Preference Code"));
			builder.SetCaption(euBag.EntryExitPurposeCodeDropEdit, h => Res.GetData("5C49FF03-A618-4566-9570-42C6C8593D15", "Entry Purpose Code"));
			builder.SetCaption(euBag.EntryExitPurposeDetailTextBox, h => Res.GetData("E7C8A949-855A-4520-A59D-D06638F554CB", "Entry Purpose Detail"));

			return builder.Build();
		}
	}
}
