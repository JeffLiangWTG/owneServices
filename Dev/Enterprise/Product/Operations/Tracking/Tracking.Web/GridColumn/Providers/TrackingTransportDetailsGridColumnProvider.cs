using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingTransportDetailsGridColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZByte)((Transport)null).JW_LegOrder);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Leg, Transport.Schema.JW_LegOrder) { ColumnKey = WebTracker.Grids.TrackingTransports.Leg });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).JW_TransportMode);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Mode, Transport.Schema.JW_TransportMode) { ColumnKey = WebTracker.Grids.TrackingTransports.Mode });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).JW_TransportType);
			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((Transport)null).JW_TransportType_List);
			ZDropDownListColumn transportTypeColumn =
				new ZDropDownListColumn(
					TransportHelper.ColumnHeaders.Type,
					Transport.Schema.JW_TransportType,
					Transport.Schema.JW_TransportType_List)
				{ ColumnKey = WebTracker.Grids.TrackingTransports.Type };
			transportTypeColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(transportTypeColumn);

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).JW_ParentConsignmentRef);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Parent, Transport.Schema.JW_ParentConsignmentRef) { ColumnKey = WebTracker.Grids.TrackingTransports.Parent });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).BillOfLadingWithSuppression);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Bill, Transport.Schema.BillOfLadingWithSuppression) { ColumnKey = WebTracker.Grids.TrackingTransports.Bill });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).JW_Vessel);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Vessel, Transport.Schema.JW_Vessel) { ColumnKey = WebTracker.Grids.TrackingTransports.Vessel });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).VoyageFlightWithSuppression);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("1b2f1fa8-6266-4dbc-9120-ef63796a7393", "Voyage/Flight"), Transport.Schema.VoyageFlightWithSuppression) { ColumnKey = WebTracker.Grids.TrackingTransports.Voyage });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).LoadPort.RL_PortName);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Load, "LoadPort+" + RefUNLOCO.Schema.RL_PortName) { ColumnKey = WebTracker.Grids.TrackingTransports.Load });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).DiscPort.RL_PortName);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Discharge, "DiscPort+" + RefUNLOCO.Schema.RL_PortName) { ColumnKey = WebTracker.Grids.TrackingTransports.Discharge });

			ZBindToChecker.CheckBindTo((ZDateTime)((Transport)null).ATDWithSuppression);
			ZBindToChecker.CheckBindTo((ZDateTime)((Transport)null).ETDWithSuppression);
			AddToDictionaryAsDefault(
				new ZTimelineColumn(
					TransportHelper.ColumnHeaders.Departure,
					Transport.Schema.ATDWithSuppression,
					Transport.Schema.ETDWithSuppression,
					ZDateTimePickerFormat.Short)
				{ ColumnKey = WebTracker.Grids.TrackingTransports.Departure });

			ZBindToChecker.CheckBindTo((ZDateTime)((Transport)null).ATAWithSuppression);
			ZBindToChecker.CheckBindTo((ZDateTime)((Transport)null).ETAWithSuppression);
			AddToDictionaryAsDefault(
				new ZTimelineColumn(
					TransportHelper.ColumnHeaders.Arrival,
					Transport.Schema.ATAWithSuppression,
					Transport.Schema.ETAWithSuppression,
					ZDateTimePickerFormat.Short)
				{ ColumnKey = WebTracker.Grids.TrackingTransports.Arrival });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).JW_Calc_Status);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Status, Transport.Schema.JW_Calc_Status) { ColumnKey = WebTracker.Grids.TrackingTransports.Status });

			ZBindToChecker.CheckBindTo((ZString)((Transport)null).CarrierWithSuppression);
			AddToDictionaryAsDefault(new ZTextEditColumn(TransportHelper.ColumnHeaders.Carrier, Transport.Schema.CarrierWithSuppression) { ColumnKey = WebTracker.Grids.TrackingTransports.Carrier });
		}
	}
}
