using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CartageContainerColumnProvider : GridColumnProvider
	{
		public CartageContainerColumnProvider()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((CommonContainer)null).JC_ContainerNum);
			ZBindToChecker.CheckBindTo((ZShort)((CommonContainer)null).JC_ContainerCount);
			ZBindToChecker.CheckBindTo((ZString)((CommonContainer)null).JC_SealNum);
			ZBindToChecker.CheckBindTo((ZString)((CommonContainer)null).JC_ContainerMode);
			ZBindToChecker.CheckBindTo((ZGuid)((CommonContainer)null).JC_RC);
			ZBindToChecker.CheckBindTo((RefContainerCollection)((CommonContainer)null).RefContainer_List);
			ZBindToChecker.CheckBindTo((ZString)((CommonContainer)null).JC_DeliveryMode);
			ZBindToChecker.CheckBindTo((ZDecimal)((CommonContainer)null).JC_Calc_ActualGrossWeightInKgs);

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("bb2aa8ee-df1b-4523-bb37-eea90ca5392b", "Container No."), CommonContainer.Schema.JC_ContainerNum) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("1fafd0b6-e0d8-4866-a999-f035fe0b86cd", "Count"), CommonContainer.Schema.JC_ContainerCount) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("04789aa8-2d31-4ad4-89cb-829a780da8a5", "Seal/Rate Class"), CommonContainer.Schema.JC_SealNum) { ColumnKey = WebTracker.Grids.TrackingContainers.SealNumber });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ee3510a5-cdeb-4e52-8109-d259a8d319a1", "Mode"), CommonContainer.Schema.JC_ContainerMode) { ColumnKey = WebTracker.Grids.TrackingContainers.Mode });
			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("84407729-1d7b-4c36-af85-0abed061998d", "Type"), CommonContainer.Schema.JC_RC, "RefContainer_List") { ColumnKey = WebTracker.Grids.TrackingContainers.Type });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9eefd79f-d896-4e43-bbb9-b054b198235e", "Delivery Mode"), CommonContainer.Schema.JC_DeliveryMode) { ColumnKey = WebTracker.Grids.TrackingContainers.DeliveryMode });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("134fe0d8-bfc7-4826-885d-c8a9d1aa0ebd", "Gross Weight (Kg)"), CommonContainer.Schema.JC_Calc_ActualGrossWeightInKgs) { ColumnKey = WebTracker.Grids.TrackingContainers.Weight });
		}
	}
}
