using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using AsycudaBill = Enterprise.Customs.ZA.Manifest.Business.AsycudaBill;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	public class ZABillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public ZABillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var zaBag = ZABillControlBag.Instance;
			builder.AddControlBag(zaBag);

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
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.RegistrationDateEdit, ControlWidthClass.Auto);
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.BillIssuerTextBox, ControlWidthClass.Long);
			builder.Add(common.BillIssuerNameTextBox, ControlWidthClass.Long);
			builder.Add(common.BillIssuerCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
			builder.Add(zaBag.CargoReleaseStatusDropEdit, ControlWidthClass.Long);
			builder.Add(zaBag.CargoReleaseStatusOtherDescriptionTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.ForwarderAddressControl, ControlWidthClass.Long);
			builder.Add(common.UCRNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.CustomsNumbersGroupBox, ControlWidthClass.Long);
			builder.Add(zaBag.CaseNumberBillsGroupBox, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(common.AssociatedPacksGroupBox, ControlWidthClass.LongNoCaption, common.CustomsNumbersGroupBox);

			builder.SetVisibility(zaBag.CargoReleaseStatusDropEdit, b => b.IsCargoReleaseStatusVisible, b => b.CargoReleaseStatusInfo, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(zaBag.CargoReleaseStatusOtherDescriptionTextBox, b => b.IsCargoReleaseStatusOtherDescriptionVisible, b => b.CargoReleaseStatusInfo, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(zaBag.CaseNumberBillsGroupBox, b => b.CaseNumbersVisible, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.MessageStatusTextBox, b => IsBillLevelManifestType(b), b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.BillStatusDropEdit, b => IsBillLevelManifestType(b), b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.RegistrationDateEdit, b => IsBillLevelManifestType(b), b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.GoodsLocationDropEditWithFixedWidth, b => !b.Header?.IsRFM ?? false, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.ForwarderAddressControl, b => !b.Header?.IsRFM ?? false, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.CustomsNumbersGroupBox, b => (b.Header?.SupportMultipleCustomsNumbers ?? false), b => b.Header?.AMA_TransportModeInfo, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.AssociatedPacksGroupBox, b => (b.Header?.SupportAssociatedPacks ?? false), b => b.Header?.AMA_TransportModeInfo, b => b.Header?.AMA_ManifestTypeInfo);

			return builder.Build();
		}

		bool IsBillLevelManifestType(AsycudaBill bill) => bill.Header?.IsBillLevelManifestType ?? false;
	}
}
