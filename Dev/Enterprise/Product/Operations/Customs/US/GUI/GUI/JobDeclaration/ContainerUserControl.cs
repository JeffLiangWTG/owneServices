using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ContainerUserControl : BaseCustomsCusContainersWithTrackingUserControl
	{
		public ContainerUserControl()
		{
			InitializeComponent();
		}

		#region Overrides

		public override Customs.Business.BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				base.JobDeclaration = value;
				if (containersUserControl1 != null)
				{
					((InBondContainerTrackingUserControl)containersUserControl1).Declaration = (JobDeclaration)value;
				}
			}
		}

		protected override ContainersUserControl GetContainerTrackingUserControl()
		{
			return new InBondContainerTrackingUserControl();
		}
		#endregion
	}
}
