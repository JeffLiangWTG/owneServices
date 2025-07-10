using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CartageLegColumnProvider))]
	sealed class CartageLegColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Pickup", "PickupFromDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartageLegs.Pickup });
			AddDefaultsColumn(new ZTextEditColumn("Delivery", "DeliverToDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartageLegs.Delivery });
			AddDefaultsColumn(new ZTextEditColumn("Container #", "Container+JC_ContainerNum") { ColumnKey = WebTracker.Grids.TrackingCartageLegs.ContainerNumber });
			AddDefaultsColumn(new ZDateTimeColumn("Planned Pic. From", CommonCartageLeg.Schema.JU_PlannedPickupTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.PlannedPickupTime });
			AddDefaultsColumn(new ZDateTimeColumn("Delivery Planned", CommonCartageLeg.Schema.JU_EstimatedDeliveryTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.DeliveryPlanned });
			AddDefaultsColumn(new ZDateTimeColumn("Pic. Time In", CommonCartageLeg.Schema.JU_PickupTimeIn, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.PickupTimeIn });
			AddDefaultsColumn(new ZDateTimeColumn("Pic. Time Out", CommonCartageLeg.Schema.JU_PickupTimeOut, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.PickupTimeOut });
			AddDefaultsColumn(new ZDateTimeColumn("Del. Time In", CommonCartageLeg.Schema.JU_DeliverTimeIn, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.DeliverTimeIn });
			AddDefaultsColumn(new ZDateTimeColumn("Del. Time Out", CommonCartageLeg.Schema.JU_DeliverTimeOut, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.DeliverTimeOut });
			AddDefaultsColumn(new ZTextEditColumn("Leg Notes", CommonCartageLeg.Schema.JU_LegNotes) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.LegNotes });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CartageLegColumnProvider();
		}
	}
}
