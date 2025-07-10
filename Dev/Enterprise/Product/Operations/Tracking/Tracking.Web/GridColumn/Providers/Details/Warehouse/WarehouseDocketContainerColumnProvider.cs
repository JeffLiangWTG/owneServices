using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public class WarehouseDocketContainerColumnProvider : GridColumnProvider
	{
		public WarehouseDocketContainerColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c45638eb-0351-4933-b285-abcd05770cc9", "Container #"), WhsDocketContainerSchema.WC_ContainerNum.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b43a681d-90d1-4de5-a60c-7c7651e17913", "Seal #"), WhsDocketContainerSchema.WC_SealNum.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.SealNumber });
			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("13e69a1f-53a9-4caf-9d52-dc8d0f567b6c", "Type"), WhsDocketContainerSchema.WC_RC.Name, "Lookups.RefContainers")
			{
				ColumnKey = WebTracker.Grids.TrackingContainers.Type,
				ValueFieldName = "PK",
				ModuleID = WebModuleIDs.RefContainer
			});
			AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("d62e305b-0fda-4b59-8807-c8826baffea8", "Palletized"), WhsDocketContainerSchema.WC_IsPalletised.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.Palletized });
			AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("5ddbf28c-c8db-4b8c-b9ea-df9dbc1c2a50", "Chargeable"), WhsDocketContainerSchema.WC_IsChargeable.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.Chargeable });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("85840c22-df2f-4bfc-bf5a-cf0b1eda00c0", "Items"), WhsDocketContainerSchema.WC_ItemCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.Items });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("a39ffbf9-deec-4c1b-b6fb-5bbc11d68f01", "Pallets"), WhsDocketContainerSchema.WC_PalletCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.Pallets });
		}
	}
}
