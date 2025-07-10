using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ForwardingBookingContainerColumnProvider : GridColumnProvider
	{
		public ForwardingBookingContainerColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("64da4701-e611-400b-b865-133bd3c6afdf", "Container#"), JobContainerSchema.JC_ContainerNum.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });
#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo(((MasterFiles.Business.RefContainerCollection)(((Freight.Forwarding.Business.ForwardingContainer)(null)).RefContainer_List)));
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("4f9dd22b-a93f-4d74-9255-632e24aba053", "Type"), JobContainerSchema.JC_RC.Name, "RefContainer_List")
			{
				ColumnKey = WebTracker.Grids.TrackingContainers.Type,
				ValueFieldName = MasterFiles.Business.RefContainer.Schema.RC_Code,
				TextFieldName = MasterFiles.Business.RefContainer.Schema.RC_Description
			});
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("da21c22e-3d21-41a4-bd5e-5723b1b96f4d", "Count"), JobContainerSchema.JC_ContainerCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });
		}
	}
}
