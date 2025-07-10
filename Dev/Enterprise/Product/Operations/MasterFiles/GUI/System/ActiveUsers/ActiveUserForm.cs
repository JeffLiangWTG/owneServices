using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ActiveUserForm : ZForm
	{
		public ActiveUserForm()
		{
			InitializeComponent();
		}

		public ActiveUserForm(ActiveUser user) : base(user)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtons);
		}

		void buttonRefresh_Click(object sender, EventArgs e)
		{
			((ActiveUser)BusinessEntity).RefreshActiveSemaphores();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			((ActiveUser)BusinessEntity).RefreshActiveSemaphores();
		}
	}
}
