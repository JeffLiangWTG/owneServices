using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingContainerDetailsColumnProvider : TrackingContainerColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionary(new ZCalcEditColumn(Res.GetString("77b3d489-19d5-40d2-a81d-3801b10e9855", "Number of Containers"), JobContainerSchema.JC_ContainerCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });
			TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			if (siteUser != null && !siteUser.IsShipmentQuickViewUser)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9281b3d1-7780-4041-83e6-235e978d4382", "Tare Weight"), TrackingContainer.Schema.JC_TareWeightWithSuppression) { ColumnKey = WebTracker.Grids.TrackingContainers.TareWeight });
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b624e189-72c9-4b27-9f5c-e0383eda688e", "Weight"), TrackingContainer.Schema.JC_TotalWeightWithSuppression) { ColumnKey = WebTracker.Grids.TrackingContainers.Weight });
			}

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("31dc991a-33b1-4996-81c3-ff3061c7c45e", "Delivery Mode"), JobContainerSchema.JC_DeliveryMode.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.DeliveryMode });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("dad41414-4b21-4942-a739-313edbf045c9", "Est. Delivery"), TrackingContainer.Schema.JC_ArrivalEstimatedDelivery, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.EstimatedDelivery });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("85379506-13d0-4f10-8b1a-be1477e413d4", "Est. Return"), TrackingContainer.Schema.JC_EmptyReturnedBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.EstimatedReturn });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("f3ea4fe7-5e1b-4c59-9203-af3e132dc7b3", "Act. Return"), TrackingContainer.Schema.JC_ContainerYardEmptyReturnGateIn, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.ActualReturn });
		}
	}
}
