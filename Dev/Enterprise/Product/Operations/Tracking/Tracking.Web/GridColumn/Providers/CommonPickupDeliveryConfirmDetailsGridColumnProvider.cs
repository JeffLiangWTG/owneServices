using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CommonPickupDeliveryConfirmDetailsGridColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			ZBindToChecker.CheckBindTo((ZDateTime)((CommonPickupDeliveryConfirm)null).EU_PickupDeliveryTime);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("2A420E0D-B90B-4527-A6EF-B02F4F259EC8", "Dispatched At"), CommonPickupDeliveryConfirm.Schema.EU_PickupDeliveryTime) { ColumnKey = WebTracker.Grids.DeliveryInformation.Dispatched });

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_DriversName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("FD2380B1-35B6-4DD3-AA71-65ABE6ECB46E", "Driver"), CommonPickupDeliveryConfirm.Schema.EU_DriversName) { ColumnKey = WebTracker.Grids.DeliveryInformation.Driver });

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_TransportCoName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("2E115EE8-12BA-4694-938A-DC0977C1AFAC", "Transport Co Name"), CommonPickupDeliveryConfirm.Schema.EU_TransportCoName) { ColumnKey = WebTracker.Grids.DeliveryInformation.TransportCompanyName });

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_VehicleRegistration);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("C97C8420-41BB-4A29-8486-E1AED7FC0CFD", "Vehicle Registration"), CommonPickupDeliveryConfirm.Schema.EU_VehicleRegistration) { ColumnKey = WebTracker.Grids.DeliveryInformation.VehicleRegistration });

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).FullGatePass);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("A9C0817D-7BED-435F-AE26-CCE32DE08CBF", "Gate Pass ID"), CommonPickupDeliveryConfirm.Schema.FullGatePass) { ColumnKey = WebTracker.Grids.DeliveryInformation.GatePassID });
		}
	}
}
