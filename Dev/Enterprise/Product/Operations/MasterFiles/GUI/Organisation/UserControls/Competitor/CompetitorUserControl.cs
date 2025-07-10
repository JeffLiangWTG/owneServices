using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CompetitorUserControl : OrganisationSecurityContainerControl
	{
		public CompetitorUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var org = CurrentDataItem as OrgHeader;
			if (org != null && org.OH_IsCompetitor)
			{
				SetupTabPageSecurity();
			}
		}

		#region Security

		internal void SetupTabPageSecurity()
		{
			CIDetailsTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.OrgCompetitorView, Env.Security.CompetitorIntelligenceView }, Env.Licence.RelationshipClientIntelligence);
			CIProfileTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.OrgCompetitorView, Env.Security.CompetitorIntelligenceView }, Env.Licence.RelationshipClientIntelligence);
		}

		internal void DetachLicenceCheckpoints()
		{
			CIDetailsTabPage.LicenceCheckpoint = null;
			CIProfileTabPage.LicenceCheckpoint = null;
		}

		#endregion

		#region Dispose

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
