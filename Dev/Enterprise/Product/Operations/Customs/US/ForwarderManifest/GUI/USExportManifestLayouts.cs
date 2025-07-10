using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public sealed class USExportManifestLayouts : IPanelLayoutProvider
	{
		public USExportManifestLayouts()
		{
			ManifestDetails = CreateManifestDetailsLayout();
		}
		PanelLayout IPanelLayoutProvider.Layout => ManifestDetails;

		PanelLayout ManifestDetails { get; }

		PanelLayout CreateManifestDetailsLayout()
		{
			var builder = new USExportManifestLayoutBuilder<USExportAsycudaManifestHeader>();
			var common = builder.CommonBag;
			var usExportManifestControlBag = USExportManifestControlBag.Instance;
			builder.AddControlBag(usExportManifestControlBag);

			builder.AddColumn();
			builder.Add(common.CountryTextBox, ControlWidthClass.Medium);
			builder.Add(common.NatureDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(common.BuyersConsolidationCheckBox, ControlWidthClass.Long);
			builder.Add(common.VesselCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.VoyageFlightTextBox, ControlWidthClass.Medium);
			builder.Add(common.LloydsNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(usExportManifestControlBag.DeparturePortUNLOCOCodeFindBox, ControlWidthClass.Long);
			builder.Add(usExportManifestControlBag.ScheduleDCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.EstDepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(common.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.PortOfDischargeCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CarrierAddressControl, ControlWidthClass.Long);
			builder.Add(usExportManifestControlBag.IssuerSCACUserControl, ControlWidthClass.Auto);
			builder.Add(common.IssueDateDateEdit, ControlWidthClass.Auto);
			builder.SetVisibility(usExportManifestControlBag.IssuerSCACUserControl, h => h.IsConsolidator, h => h.AMA_ApplicationCodeInfo);
			builder.AddControlBehaviour<ZUserControl>(usExportManifestControlBag.IssuerSCACUserControl, UpdateIssuerSCACGroupBoxControlBehaviourAction);

			return builder.Build();
		}

		void UpdateIssuerSCACGroupBoxControlBehaviourAction(Control control, USExportAsycudaManifestHeader header)
		{
			ControlDpiScalingHelper.SetLeft(ref control, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(control.Left) - 90, true);
		}
	}
}
