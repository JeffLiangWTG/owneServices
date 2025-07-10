using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.GUI
{
	public partial class ContainerUserControl : CustomsCusContainersWithTrackingAndAdditionalSealUserControl
	{
		public ContainerUserControl()
		{
			InitializeComponent();
			CusContainersBoundGrid.InnerGrid.SetColumnVisible(true, CusContainerSchema.CO_SecondSeal.Name);
		}

		protected override Freight.GUI.ContainersUserControl GetContainerTrackingUserControl()
		{
			return new ContainerDetailsUserControl();
		}

		protected new JobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set => base.JobDeclaration = value;
		}
	}
}
