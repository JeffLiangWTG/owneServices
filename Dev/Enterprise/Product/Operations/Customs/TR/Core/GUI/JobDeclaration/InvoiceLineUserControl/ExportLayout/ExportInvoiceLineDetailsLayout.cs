using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new ExportInvoiceLineDetailsLayoutBuilder();
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
			builder.Add(trBag.SupplementaryCode2DropEdit, ControlWidthClass.Long);
			builder.Add(euBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
			builder.Add(euBag.CommercialPaymentCodeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.UnicodeDescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.ValuationCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ManufacturerAddressControl, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.PriceTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.InwardProcessingLicenseLineNumberTextBox, ControlWidthClass.Long);
			builder.Add(euBag.ReturningGoodsReasonCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.ReturningGoodsReasonDetailTextBox, ControlWidthClass.Long);
			builder.Add(euBag.EntryExitPurposeCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.EntryExitPurposeDetailTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(trBag.BrandNameTextBox, ControlWidthClass.Medium);
			builder.Add(euBag.BorderTradeStateCodeFindBox, ControlWidthClass.Medium);
			builder.Add(euBag.ExportUnionAdditionalTariffCodeFindBox, ControlWidthClass.Medium);
			builder.Add(euBag.ExportUnionDeferredInstallmentTextBox, ControlWidthClass.Medium);
			builder.Add(euBag.ExportUnionEcologicalCheckBox, ControlWidthClass.Medium);
			builder.Add(commonBag.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
			builder.Add(commonBag.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);
			builder.Add(euBag.ProcessingDescriptionLongTextControl, ControlWidthClass.Medium);
			builder.Add(euBag.ExportUnionPackCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.ExportUnionThreadCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.ExportUnionProductionYearCalcEdit, ControlWidthClass.Auto);

			builder.SetVisibility(euBag.ReturningGoodsReasonDetailTextBox, b => b.ReturningGoodsReasonCodeVisibility, b => b.ZG_ReturningGoodsReasonCodeInfo);
			builder.SetVisibility(euBag.EntryExitPurposeCodeDropEdit, b => b.IsEntryExitPurposeCodeVisibility, b => b.JI_FormattedProcedureInfo);
			builder.SetVisibility(euBag.EntryExitPurposeDetailTextBox, b => b.IsEntryExitPurposeDetailVisibility, b => b.ZG_EntryExitPurposeCodeInfo);

			builder.SetVisibility(commonBag.PreviousEntryNumberTextBox, b => b.IsPreviousEntryAvailable, b => b.JI_FormattedProcedureInfo);
			builder.SetVisibility(commonBag.PreviousEntryLineNumberCalcEdit, b => b.IsPreviousEntryAvailable, b => b.JI_FormattedProcedureInfo);
			builder.SetVisibility(euBag.ProcessingDescriptionLongTextControl, b => b.IsPreviousEntryAvailable, b => b.JI_FormattedProcedureInfo);

			builder.SetCaption(euBag.AdditionalSupplementaryCodesUserControl, h => Res.GetData("7D651F16-3624-4596-A0A2-728B5F3E3585", "Exemption Codes"));
			builder.SetCaption(euBag.EntryExitPurposeCodeDropEdit, h => h.EntryExitPurposeCodeDependingMessageType, h => h.Declaration?.JE_MessageTypeInfo);
			builder.SetCaption(euBag.EntryExitPurposeDetailTextBox, h => h.EntryExitPurposeDetailDependingMessageType, h => h.Declaration?.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
