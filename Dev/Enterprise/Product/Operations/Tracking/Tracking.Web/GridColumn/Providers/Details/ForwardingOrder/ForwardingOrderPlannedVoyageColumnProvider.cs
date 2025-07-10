using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ForwardingOrderPlannedVoyageColumnProvider : GridColumnProvider
	{
		public ForwardingOrderPlannedVoyageColumnProvider()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((PlannedVoyage)null).VoyageType);
			ZBindToChecker.CheckBindTo((ZString)((PlannedVoyage)null).Vessel);
			ZBindToChecker.CheckBindTo((ZString)((PlannedVoyage)null).Voyage);
			ZBindToChecker.CheckBindTo((ZDateTime)((PlannedVoyage)null).ETD);
			ZBindToChecker.CheckBindTo((ZDateTime)((PlannedVoyage)null).ETA);

			AddToDictionaryAsDefault(new ZTextEditColumn(ZString.Empty, "VoyageType") { ColumnKey = WebTracker.Grids.PlannedVoyage.VoyageType });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d6732548-e9c3-4aad-be97-97178e6723e3", "Vessel"), (NoResString)"Vessel") { ColumnKey = WebTracker.Grids.PlannedVoyage.Vessel }); // Binding name
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("dcc422f9-6fc8-480e-9d2d-27fff8157169", "Voyage/Flight"), (NoResString)"Voyage") { ColumnKey = WebTracker.Grids.PlannedVoyage.Voyage }); // Binding name
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("e4c72b72-3d95-463a-9cf5-f2782705421d", "Estimated Departure"), "ETD", ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.PlannedVoyage.ETD });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("61b31dd2-40f8-4ec2-867c-7dde41f5b9da", "Estimated Arrival"), "ETA", ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.PlannedVoyage.ETA });
		}
	}
}
