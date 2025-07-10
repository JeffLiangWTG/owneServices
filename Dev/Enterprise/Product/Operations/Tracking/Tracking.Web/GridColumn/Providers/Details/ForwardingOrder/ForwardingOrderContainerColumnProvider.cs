using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ForwardingOrderContainerColumnProvider : GridColumnProvider
	{
		public ForwardingOrderContainerColumnProvider()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((CommonContainer)null).JC_ContainerNum);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("06125319-40c7-4c37-8cf4-e6dbaa20423c", "Container No."), CommonContainer.Schema.JC_ContainerNum) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });

			ZBindToChecker.CheckBindTo((ZShort)((CommonContainer)null).JC_ContainerCount);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("bfac466d-591b-4e11-aac7-8d582aefb22b", "Count"), CommonContainer.Schema.JC_ContainerCount) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });

			ZBindToChecker.CheckBindTo((ZString)((CommonContainer)null).JC_SealNum);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6aad4068-c0cb-4480-8289-ea7103903359", "Seal/Rate Class"), CommonContainer.Schema.JC_SealNum) { ColumnKey = WebTracker.Grids.TrackingContainers.SealNumber });

			ZBindToChecker.CheckBindTo((ZString)((CommonContainer)null).JC_ContainerMode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6ee775e7-7cb0-4c79-9db0-f4718bf20255", "Mode"), CommonContainer.Schema.JC_ContainerMode) { ColumnKey = WebTracker.Grids.TrackingContainers.Mode });

			ZBindToChecker.CheckBindTo((ZGuid)((CommonContainer)null).JC_RC);
			ZBindToChecker.CheckBindTo((RefContainerCollection)((CommonContainer)null).RefContainer_List);
			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("b75f02da-41d5-4935-a2a5-e2007f7146c8", "Type"), CommonContainer.Schema.JC_RC, "RefContainer_List") { ColumnKey = WebTracker.Grids.TrackingContainers.Type });

			ZBindToChecker.CheckBindTo((ZString)((CommonContainer)null).JC_DeliveryMode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("fb499638-d480-48cf-b756-78ea25afe1a2", "Delivery Mode"), CommonContainer.Schema.JC_DeliveryMode) { ColumnKey = WebTracker.Grids.TrackingContainers.DeliveryMode });

			ZBindToChecker.CheckBindTo((ZDecimal)((CommonContainer)null).JC_Calc_ActualGrossWeightInKgs);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("32d6a159-67fd-480a-8447-377e112038f3", "Gross Weight (Kg)"), CommonContainer.Schema.JC_Calc_ActualGrossWeightInKgs) { ColumnKey = WebTracker.Grids.TrackingContainers.Weight });
		}
	}
}
