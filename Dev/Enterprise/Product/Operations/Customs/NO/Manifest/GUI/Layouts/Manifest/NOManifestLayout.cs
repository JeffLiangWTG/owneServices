using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class NOManifestLayout : IPanelLayoutProvider
{
	public NOManifestLayout()
	{
		ManifestLayout = CreateManifestLayout();
	}

	PanelLayout ManifestLayout { get; }

	PanelLayout IPanelLayoutProvider.Layout => ManifestLayout;

	static PanelLayout CreateManifestLayout()
	{
		var builder = new ManifestLayoutBuilder();
		var noBag = NOManifestControlBag.Instance;
		builder.AddControlBag(noBag);
		var common = builder.CommonBag;

		builder.AddColumn();
		builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
		builder.Add(common.RegistrationDateEdit, ControlWidthClass.Medium);
		builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
		builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
		builder.Add(common.AgentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(noBag.TransportMeansCodeFindBox, ControlWidthClass.Long);
		builder.Add(noBag.VehicleRegistrationAndNationalityUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(noBag.DriverNameTextBox, ControlWidthClass.Long);
		builder.Add(noBag.DriverCommunicationIdTextBox, ControlWidthClass.Long);
		builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
		builder.Add(common.BuyersConsolidationCheckBox, ControlWidthClass.Long);
		builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
		builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.EstArrivalDateEdit, ControlWidthClass.Auto);
		builder.Add(common.CustomsOfficeDropEdit, ControlWidthClass.Long);
		builder.Add(common.DateAtCustomsOfficeDateEdit, ControlWidthClass.Auto);
		builder.Add(noBag.ScheduledDateOfArrCustOfficeDateEdit, ControlWidthClass.Medium);

		builder.AddColumn();
		builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
		builder.Add(common.MessageStatusTextBox, ControlWidthClass.Medium);
		builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
		builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
		builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
		builder.Add(common.MasterBOLTextBox, ControlWidthClass.Medium);
		builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
		builder.Add(noBag.RepresentativeAddressControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(noBag.MasterBillGroupBox, ControlWidthClass.LongControl);

		builder.AddControlBehaviour<VehicleRegistrationAndNationalityUserControl>(noBag.VehicleRegistrationAndNationalityUserControl, (c, _) => c.VehicleRegistrationTextBox.UpdateCaption(), h => h.AMA_TransportModeInfo);

		return builder.Build();
	}
}
