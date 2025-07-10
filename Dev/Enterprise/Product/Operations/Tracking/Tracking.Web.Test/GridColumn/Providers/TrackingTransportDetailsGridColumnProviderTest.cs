using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingTransportDetailsGridColumnProvider))]
	sealed class TrackingTransportDetailsGridColumnProviderTest : GridColumnProviderTest
	{
		protected override bool SupportsOldLayoutFix
		{
			get
			{
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Leg", Transport.Schema.JW_LegOrder) { ColumnKey = WebTracker.Grids.TrackingTransports.Leg });
			AddDefaultsColumn(new ZTextEditColumn("Mode", Transport.Schema.JW_TransportMode) { ColumnKey = WebTracker.Grids.TrackingTransports.Mode });

			ZDropDownListColumn transportTypeColumn = new ZDropDownListColumn("Type",
				Transport.Schema.JW_TransportType,
				Transport.Schema.JW_TransportType_List)
			{ ColumnKey = WebTracker.Grids.TrackingTransports.Type };
			transportTypeColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddDefaultsColumn(transportTypeColumn);

			AddDefaultsColumn(new ZTextEditColumn("Parent", Transport.Schema.JW_ParentConsignmentRef) { ColumnKey = WebTracker.Grids.TrackingTransports.Parent });
			AddDefaultsColumn(new ZTextEditColumn("Bill", Transport.Schema.BillOfLadingWithSuppression) { ColumnKey = WebTracker.Grids.TrackingTransports.Bill });
			AddDefaultsColumn(new ZTextEditColumn("Vessel", Transport.Schema.JW_Vessel) { ColumnKey = WebTracker.Grids.TrackingTransports.Vessel });
			AddDefaultsColumn(new ZTextEditColumn("Voyage/Flight", Transport.Schema.VoyageFlightWithSuppression) { ColumnKey = WebTracker.Grids.TrackingTransports.Voyage });
			AddDefaultsColumn(new ZTextEditColumn("Load", "LoadPort+" + RefUNLOCO.Schema.RL_PortName) { ColumnKey = WebTracker.Grids.TrackingTransports.Load });
			AddDefaultsColumn(new ZTextEditColumn("Discharge", "DiscPort+" + RefUNLOCO.Schema.RL_PortName) { ColumnKey = WebTracker.Grids.TrackingTransports.Discharge });

			AddDefaultsColumn(new ZTimelineColumn("Departure",
				Transport.Schema.ATDWithSuppression,
				Transport.Schema.ETDWithSuppression,
				ZDateTimePickerFormat.Short)
			{ ColumnKey = WebTracker.Grids.TrackingTransports.Departure });

			AddDefaultsColumn(new ZTimelineColumn("Arrival",
				Transport.Schema.ATAWithSuppression,
				Transport.Schema.ETAWithSuppression,
				ZDateTimePickerFormat.Short)
			{ ColumnKey = WebTracker.Grids.TrackingTransports.Arrival });

			AddDefaultsColumn(new ZTextEditColumn("Status", Transport.Schema.JW_Calc_Status) { ColumnKey = WebTracker.Grids.TrackingTransports.Status });
			AddDefaultsColumn(new ZTextEditColumn("Carrier", Transport.Schema.CarrierWithSuppression) { ColumnKey = WebTracker.Grids.TrackingTransports.Carrier });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingTransportDetailsGridColumnProvider();
		}
	}
}
