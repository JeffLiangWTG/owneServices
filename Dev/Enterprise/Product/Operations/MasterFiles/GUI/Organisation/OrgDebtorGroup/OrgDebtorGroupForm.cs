using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgDebtorGroupForm : ZForm
	{
		public OrgDebtorGroupForm()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		public OrgDebtorGroupForm(OrgDebtorGroup businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ZArchitecture.Modules.ControllerIDs.Audit);
		}

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
