using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI
{
	public partial class InBondContainerTrackingUserControl : Freight.GUI.ContainersUserControl
	{
		public InBondContainerTrackingUserControl()
		{
			InitializeComponent();
		}

		public JobDeclaration Declaration
		{
			get { return fDeclaration; }
			set
			{
				fDeclaration = value;
			}
		}
		JobDeclaration fDeclaration;

		protected override void AddDependentPlugIn(ZArchitecture.ZGrid containerBoundGrid)
		{
			base.AddDependentPlugIn(containerBoundGrid);
			if (Declaration.IsPluggedIntoShipment)
			{
				DetailTabControl.PlugIns.AddCurrentDependentPlugIn(Enterprise.ZArchitecture.Modules.ControllerIDs.Customs.US.ContainerAdditionalReferenceNumbers, containerBoundGrid);
			}
		}
	}
}

