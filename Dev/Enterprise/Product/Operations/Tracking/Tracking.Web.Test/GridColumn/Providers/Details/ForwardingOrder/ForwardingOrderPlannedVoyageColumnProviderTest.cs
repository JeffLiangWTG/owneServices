using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ForwardingOrderPlannedVoyageColumnProvider))]
	sealed class ForwardingOrderPlannedVoyageColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn(ZString.Empty, "VoyageType") { ColumnKey = WebTracker.Grids.PlannedVoyage.VoyageType });
			AddDefaultsColumn(new ZTextEditColumn("Vessel", "Vessel") { ColumnKey = WebTracker.Grids.PlannedVoyage.Vessel });
			AddDefaultsColumn(new ZTextEditColumn("Voyage/Flight", "Voyage") { ColumnKey = WebTracker.Grids.PlannedVoyage.Voyage });
			AddDefaultsColumn(new ZDateTimeColumn("Estimated Departure", "ETD", ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.PlannedVoyage.ETD });
			AddDefaultsColumn(new ZDateTimeColumn("Estimated Arrival", "ETA", ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.PlannedVoyage.ETA });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ForwardingOrderPlannedVoyageColumnProvider();
		}
	}
}
