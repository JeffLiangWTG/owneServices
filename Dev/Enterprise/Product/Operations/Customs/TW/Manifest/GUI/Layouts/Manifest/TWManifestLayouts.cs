using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public sealed class TWManifestLayouts : IPanelLayoutProvider
	{
		PanelLayout Manifest { get; }
		PanelLayout IPanelLayoutProvider.Layout => Manifest;

		public TWManifestLayouts()
		{
			Manifest = CreateManifestLayout();
		}

		PanelLayout CreateManifestLayout()
		{
			var builder = new TWManifestLayoutBuilder();
			var common = builder.CommonBag;
			var twManifestControlBag = builder.TWManiFestControlBag;
			builder.AddControlBag(twManifestControlBag);

			builder.AddColumn();
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			builder.Add(twManifestControlBag.GoodsLocationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ShortEstArrivalDateEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.LloydsNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierCodeTextBox, ControlWidthClass.Medium);
			builder.Add(common.DeconsolidateAddressControl, ControlWidthClass.Long);
			builder.Add(twManifestControlBag.DeconsolidateVATTextBox, ControlWidthClass.Medium);
			builder.Add(twManifestControlBag.LoginCompanyGuidFindBox, ControlWidthClass.Long);
			builder.Add(twManifestControlBag.MailBoxTextBox, ControlWidthClass.Medium);
			builder.Add(twManifestControlBag.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);

			builder.SetVisibility(common.ShortEstArrivalDateEdit, b => b.IsAir, b => b.AMA_TransportModeInfo);
			builder.SetVisibility(common.VesselCodeFindBox, b => b.IsSea, b => b.AMA_TransportModeInfo);
			builder.SetVisibility(common.LloydsNumberTextBox, b => b.IsSea, b => b.AMA_TransportModeInfo);
			builder.SetVisibility(common.VehicleRegistrationTextBox, b => b.IsSea, b => b.AMA_TransportModeInfo);

			return builder.Build();
		}
	}
}
