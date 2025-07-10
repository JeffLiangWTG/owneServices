using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CartageLegColumnProvider : GridColumnProvider
	{
		public CartageLegColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a1a05fdb-19a2-4a07-821d-a60752023b8f", "Pickup"), "PickupFromDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartageLegs.Pickup });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("7b9b4663-fc3b-42c1-8660-8614d439797d", "Delivery"), "DeliverToDocAddress+AddressSummary") { ColumnKey = WebTracker.Grids.TrackingCartageLegs.Delivery });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ef395444-2d30-4b38-be8e-c4d9862fbd07", "Container #"), "Container+JC_ContainerNum") { ColumnKey = WebTracker.Grids.TrackingCartageLegs.ContainerNumber });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("219f7971-415c-470d-8f09-7c4a8cfd644c", "Planned Pic. From"), CommonCartageLeg.Schema.JU_PlannedPickupTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.PlannedPickupTime });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("dbce74fb-357f-4e1c-acca-ec072007c220", "Delivery Planned"), CommonCartageLeg.Schema.JU_EstimatedDeliveryTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.DeliveryPlanned });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("666e9a9d-41eb-40ce-9bf6-deb8af10d7df", "Pic. Time In"), CommonCartageLeg.Schema.JU_PickupTimeIn, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.PickupTimeIn });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("52b32c7c-b5e8-464e-97b2-fb896cf73652", "Pic. Time Out"), CommonCartageLeg.Schema.JU_PickupTimeOut, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.PickupTimeOut });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("8138825c-70ff-4bae-abed-5afeb6ef4001", "Del. Time In"), CommonCartageLeg.Schema.JU_DeliverTimeIn, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.DeliverTimeIn });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("e20d0fd0-cb84-4423-8f89-79bc5221c8f0", "Del. Time Out"), CommonCartageLeg.Schema.JU_DeliverTimeOut, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.DeliverTimeOut });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("93be2005-7240-4b8b-afce-51016491b54d", "Leg Notes"), CommonCartageLeg.Schema.JU_LegNotes) { ColumnKey = WebTracker.Grids.TrackingCartageLegs.LegNotes });
		}
	}
}
