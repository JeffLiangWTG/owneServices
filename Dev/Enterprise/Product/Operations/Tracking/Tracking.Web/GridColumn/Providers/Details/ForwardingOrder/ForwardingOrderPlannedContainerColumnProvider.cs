using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ForwardingOrderPlannedContainerColumnProvider : GridColumnProvider
	{
		public ForwardingOrderPlannedContainerColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c27c70ae-c368-4a80-b610-236cada801ff", "Container No."), JobOrderContainerSchema.J1_ContainerNumber.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("9067596d-4490-461c-9e53-c41425d9132f", "Count"), JobOrderContainerSchema.J1_ContainerCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });

			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("60b26fa6-34eb-4df7-ba6b-22277e0ef89e", "Type"), JobOrderContainerSchema.J1_RC.Name, "J1_RC_List", typeof(RefContainer)) { ColumnKey = WebTracker.Grids.TrackingContainers.Type });
		}
	}
}
