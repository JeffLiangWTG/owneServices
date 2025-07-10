using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public class ACEManifestLayouts : IPanelLayoutProvider
	{
		PanelLayout ManifestDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		public ACEManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new ManifestLayoutBuilder<AsycudaManifestHeader>();
			var common = builder.CommonBag;
			var ace = ACEManifestControlBag.Instance;
			builder.AddControlBag(ace);
			builder.AddColumn();

			builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.BuyersConsolidationCheckBox, ControlWidthClass.Long);
			builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(common.RadioCallSignTextBox, ControlWidthClass.Medium);
			builder.Add(common.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.MastersNameTextBox, ControlWidthClass.Long);
			builder.Add(common.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			builder.Add(common.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.DeconsolidateAddressControl, ControlWidthClass.Long);
			builder.Add(ace.FIRMSTextBox, ControlWidthClass.Medium);
			builder.Add(ace.ExpressCourierCheckBox, ControlWidthClass.Medium);

			builder.AddColumn();

			builder.Add(common.JobReferenceTextBox, ControlWidthClass.Medium);
			builder.Add(common.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			builder.Add(common.MasterBOLTextBox, ControlWidthClass.Medium);
			builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierCodeTextBox, ControlWidthClass.Medium);
			builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(ace.EstDateAtFirstArrivalDateEdit, ControlWidthClass.Auto);
			builder.Add(common.ShortEstArrivalDateEdit, ControlWidthClass.Auto);
			builder.Add(ace.BillStatusTextBox, ControlWidthClass.Auto);
			builder.Add(ace.BillStatusDescriptionTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
