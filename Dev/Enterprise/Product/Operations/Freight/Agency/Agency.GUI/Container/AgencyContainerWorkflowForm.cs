using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class AgencyContainerWorkflowForm : ZTemplateForm
	{
		public AgencyContainerWorkflowForm(AgencyShipmentContainer container)
			: base(container)
		{
			InitializeComponent();
			ControllerID = ControllerIDs.AgencyBooking;

			WorkflowTabPage.Initialize(container);

			MainTabControl.TabPages.Remove(MainTabPage);
		}

		#region Overrides

		public override string FormCaption
		{
			get { return AgencyContainer.JC_ContainerCode + " " + Res.GetString("ce612c8c-04ee-451d-84aa-d3181f497f69", "Workflow / eDocs"); }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		#endregion

		#region AgencyContainer

		AgencyShipmentContainer AgencyContainer
		{
			get { return (AgencyShipmentContainer)BusinessEntity; }
		}

		#endregion
	}
}




