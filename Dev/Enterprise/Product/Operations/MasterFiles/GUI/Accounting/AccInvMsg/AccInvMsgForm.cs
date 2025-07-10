using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccInvMsgForm : ZForm
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ShowOrHideTaxGroupCode();
		}

		void ShowOrHideTaxGroupCode()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var hasRows = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).Any();
				if (!hasRows)
				{
					A9_TaxGroupZDropEdit.Visible = false;
					GovtCodeText.Visible = false;
				}
			}
		}

		public AccInvMsgForm()
		{
			InitializeComponent();
		}

		public AccInvMsgForm(AccInvMsg bO)
			: base(bO)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
		}
	}
}
