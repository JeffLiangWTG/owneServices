using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using TariffFindBox = Enterprise.Customs.Universal.GUI.TariffFindBox;

namespace Enterprise.Customs.NO.GUI;

sealed class InvoiceLineDetailsLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateLayout();
	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
		var commonBag = builder.CommonBag;
		var noBag = InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(noBag);

		builder.AddColumn();
		builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Medium);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.StateOrRegionOfOriginDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TariffFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.PrimaryPreferenceDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.TaxTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(noBag.GoodsMarksLongTextControl, ControlWidthClass.Auto);
		builder.Add(noBag.ProcedureCodeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsFifthQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(noBag.PackageTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(noBag.SupplementaryCode1DropEdit, ControlWidthClass.Auto);
		builder.Add(noBag.SupplementaryCode2DropEdit, ControlWidthClass.Auto);
		builder.Add(noBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Auto);
		builder.Add(noBag.ReducedCustomsFlagDropEdit, ControlWidthClass.Auto);
		builder.Add(noBag.CustomsRateOverrideUserControl, ControlWidthClass.Long);
		builder.Add(noBag.RtRateOverrideCalcEdit, ControlWidthClass.Auto);
		builder.Add(noBag.MergeOverrideTextBox, ControlWidthClass.Long);

		builder.SetVisibility(commonBag.StateOrRegionOfOriginDropEdit, i => i.IsExport, i => i.Declaration?.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.TaxTypeDropEdit, i => i.IsImport, i => i.Declaration?.JE_MessageTypeInfo);
		builder.SetVisibility(noBag.PackageTypeDropEdit, i => i.NO_PackageTypeVisible, i => i.JI_TariffInfo);
		builder.SetVisibility(noBag.ReducedCustomsFlagDropEdit, i => i.IsImport, i => i.Declaration?.JE_MessageTypeInfo);
		builder.SetVisibility(noBag.CustomsRateOverrideUserControl, i => i.IsImport, i => i.Declaration?.JE_MessageTypeInfo);
		builder.SetVisibility(noBag.RtRateOverrideCalcEdit, i => i.IsImport, i => i.Declaration?.JE_MessageTypeInfo);

		builder.AddControlBehaviour<TariffFindBox>(
			controlReference: commonBag.TariffFindBox,
			updateControlBehaviourAction: (control, invoiceLine) => control.GetEffectiveDate = () => invoiceLine.EffectiveDateForDutyRate,
			dependencies: i => i.EntryInstruction?.CEI_DateForDutyInfo);

		return builder.Build();
	}
}
