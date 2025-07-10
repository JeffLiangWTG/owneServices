using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbResourceForm : ZForm
	{
		public GlbResourceForm(GlbStaff resource)
			: base(resource)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			if (!ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				ComponentMembershipTabPage.TabVisible = false;
			}
		}

		public GlbResourceForm()
			: base()
		{
			InitializeComponent();
		}

		public GlbStaff Resource
		{
			get { return (GlbStaff)base.BusinessEntity; }
		}
	}
}
