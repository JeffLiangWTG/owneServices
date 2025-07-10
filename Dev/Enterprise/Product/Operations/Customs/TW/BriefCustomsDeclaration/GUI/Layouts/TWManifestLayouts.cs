using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public sealed class TWManifestLayouts : IPanelLayoutProvider
	{
		PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		public TWManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new TWManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;
			var tw = TWManifestControlBag.Instance;
			builder.AddControlBag(tw);

			builder.AddColumn();
			builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestTypeShortCodeLengthDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsOfficeShortCodeLengthDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.MasterBillTextBox, ControlWidthClass.Long);
			builder.Add(common.CarrierReferenceTextBox, ControlWidthClass.Long);
			builder.Add(common.PaymentMethodDropEdit, ControlWidthClass.Long);
			builder.Add(tw.DeclarationDateEdit, ControlWidthClass.Medium);
			builder.Add(common.ETADateEdit, ControlWidthClass.Medium);
			builder.Add(common.GuaranteeTextBox, ControlWidthClass.Long);
			builder.Add(tw.ImporterAddressUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(tw.ExporterAddressUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(tw.EntryNumberUserControl, ControlWidthClass.Long);
			builder.Add(tw.BagNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.LloydsNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.RadioCallSignTextBox, ControlWidthClass.Medium);
			builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			builder.Add(common.ManifestNumberTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.RecipientReferenceDropEdit, ControlWidthClass.Long);
			builder.Add(tw.CustomsAgentCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.CustomsProfileDropEdit, ControlWidthClass.Long);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(tw.PersonGuidFindBox, ControlWidthClass.Long);

			builder.SetVisibility(tw.ImporterAddressUserControl, b => b.AMA_Nature == Universal.Helper.ShipmentTypeList.Codes.Import23, b => b.AMA_NatureInfo);
			builder.SetVisibility(tw.ExporterAddressUserControl, b => b.AMA_Nature == Universal.Helper.ShipmentTypeList.Codes.Export22, b => b.AMA_NatureInfo);

			builder.SetVisibility(common.GuaranteeTextBox, b => b.AMA_Nature == Universal.Helper.ShipmentTypeList.Codes.Import23, b => b.AMA_NatureInfo);
			builder.SetVisibility(common.ETADateEdit, b => b.AMA_Nature == Universal.Helper.ShipmentTypeList.Codes.Import23, b => b.AMA_NatureInfo);
			builder.SetVisibility(common.PaymentMethodDropEdit, b => b.AMA_Nature == Universal.Helper.ShipmentTypeList.Codes.Import23, b => b.AMA_NatureInfo);
			builder.SetVisibility(common.CarrierReferenceTextBox, b => b.AMA_Nature == Universal.Helper.ShipmentTypeList.Codes.Export22 && b.AMA_TransportMode == Core.Constants.TransportModes.Sea, b => b.AMA_NatureInfo, b => b.AMA_TransportModeInfo);
			builder.SetVisibility(common.VehicleRegistrationTextBox, b => b.IsSea, b => b.AMA_TransportModeInfo);
			builder.SetVisibility(common.ManifestNumberTextBox, b => b.IsSea && b.IsImport, b => b.AMA_TransportModeInfo, b => b.AMA_NatureInfo);
			builder.SetVisibility(tw.BagNumberTextBox, b => b.IsAir, b => b.AMA_TransportModeInfo);

			return builder.Build();
		}
	}
}
