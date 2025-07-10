using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class SGBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public SGBillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var sgBag = SGBillControlBag.Instance;
			builder.AddControlBag(sgBag);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsEntryNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.CustomsEntryNumberTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Long);
			builder.Add(sgBag.MessageStatusDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CusJobNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(sgBag.SGPartyIDTextBox, ControlWidthClass.Long);
			builder.Add(sgBag.SGPartyStatusDropEdit, ControlWidthClass.Auto);
			builder.Add(sgBag.SGPayeeIndicatorDropEdit, ControlWidthClass.Auto);
			builder.Add(sgBag.SGGstAmountCalcEdit, ControlWidthClass.Auto);
			builder.Add(sgBag.SGDutyAmountCalcEdit, ControlWidthClass.Auto);
			builder.Add(sgBag.CycleDateDateEdit, ControlWidthClass.Auto);
			builder.Add(sgBag.CycleNumberDropEditWithFixedWidth, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(sgBag.GSTNReferenceNoTextBox, ControlWidthClass.Long);

			builder.SetVisibility(sgBag.SGPartyStatusDropEdit, b => b.Header?.IsImport ?? false, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(sgBag.SGPayeeIndicatorDropEdit, b => b.Header?.IsImport ?? false, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(sgBag.SGGstAmountCalcEdit, b => b.Header?.IsImport ?? false, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(sgBag.SGDutyAmountCalcEdit, b => b.Header?.IsImport ?? false, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(sgBag.CycleDateDateEdit, b => b.Header?.IsImport ?? false, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(sgBag.CycleNumberDropEditWithFixedWidth, b => b.Header?.IsImport ?? false, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(sgBag.GSTNReferenceNoTextBox, b => b.Header?.IsOVRApplicable ?? false, b => b.Header?.AMA_ManifestTypeInfo);

			return builder.Build();
		}
	}
}
