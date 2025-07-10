using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgProfitShareForm : ZTemplateForm
	{
		public OrgProfitShareForm(OrgAgentRelationship agentRelationship)
			: base(agentRelationship)
		{
		}

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				((OrgAgentRelationship)DataSource).O3_ProfitShareTypeInfo.ValueChanged -= new EventHandler(O3_ProfitShareTypeInfo_ValueChanged);
				RcvAgentOrgControl.DataBindings.RemoveBinding("IsVisibleForBinding");
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				((OrgAgentRelationship)DataSource).O3_ProfitShareTypeInfo.ValueChanged += new EventHandler(O3_ProfitShareTypeInfo_ValueChanged);
				RcvAgentOrgControl.DataBindings.Add(new KBinding("IsVisibleForBinding", DataSource, "AgentAgreement"));
				SetupLayout();
			}
		}

		void O3_ProfitShareTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupLayout();
		}

		void SetupLayout()
		{
			var profitShareType = ((OrgAgentRelationship)BusinessEntity).O3_ProfitShareType;
			var isAgencyProfile = profitShareType == OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			SendAgentOrgControl.Text = isAgencyProfile ? Res.GetString("OrgProfitShareForm|AgencyOffice", "Agency Office") : Res.GetString("OrgProfitShareForm|SendingAgent", "Sending Agent");

			var scaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(RcvAgentOrgControl.Location.X + RcvAgentOrgControl.Width) + 8;
			var scaleY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(RcvAgentOrgControl.Location.Y);

			HeadOfficeOrgControl.Location = isAgencyProfile ? RcvAgentOrgControl.Location : ControlDpiScalingHelper.NewScaledPoint(scaleX, scaleY);

			ProfitShareControl
				.ProfitShareGrid?
				.SetAvailability(isAgencyProfile, AutoOrgProfitShareDetails.Schema.O4_GatewayProfitApportionmentMethod);
			ClientSpecificProfitShareControl
				.ProfitShareGrid?
				.SetAvailability(isAgencyProfile, AutoOrgProfitShareDetails.Schema.O4_GatewayProfitApportionmentMethod);

			// first time loading they should not be visible, later if needed they will be visible
			ProfitShareControl
				.PartyDetailsControl
				.SetGatewayConsolProfitRedistributionTabPageVisibility(false);
			ClientSpecificProfitShareControl
				.PartyDetailsControl
				.SetGatewayConsolProfitRedistributionTabPageVisibility(false);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (BusinessEntity != null)
				{
					((OrgAgentRelationship)BusinessEntity).O3_ProfitShareTypeInfo.ValueChanged -= new EventHandler(O3_ProfitShareTypeInfo_ValueChanged);
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
