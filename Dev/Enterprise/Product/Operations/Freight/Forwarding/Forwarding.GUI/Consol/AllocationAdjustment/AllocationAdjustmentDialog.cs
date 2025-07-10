using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class AllocationAdjustmentDialog : ZChildForm
	{
		public AllocationAdjustmentDialog(AllocationAdjustmentsSecurity adjustmentsSecurity)
			: base(adjustmentsSecurity)
		{
			InitializeComponent();
			DialogResult = DialogResult.Cancel;

			MessageLabel.Text = Res.GetString("723bd231-26fc-4288-ab2e-31b5f397de16", "For the following consolidation(s) some of the pre-allocated values exceeds the registry-specified percentage. To continue saving you can increase pre-allocated values to stay within permitted percentage of pre-allocations or you can cancel the save and put a shipment on another consol.");

			if (IsCurrentUserAllowedToAdjustAllocations(Env.Security))
			{
				AuthorizationGroupBox.Enabled = false;
				AuthorizationGroupBox.Visible = false;
			}
			else
			{
				MessageLabel.Text += "\r\n\r\n" + Res.GetString("59991f9f-cd6a-412f-a8bf-a788a7c41f8e", "If you want to increase the pre-allocated value, you will need authorization from someone with permission to adjust the pre-allocations.");
				MessageLabel.Text += "\r\n\r\n" + Res.GetString("FE81C8FA-67A2-48A5-A7F9-2A73E26013F9", "Note: Only gateway agent is authorized to edit pre-allocations.");
			}
		}

		public static bool ConfirmAllocationAdjustments(IEnumerable<ForwardingConsol> consols)
		{
			if (!consols.Any())
			{
				return true;
			}

			var adjustmentsSecurity = new AllocationAdjustmentsSecurity(consols);
			return ZFormModaliser.ShowDialogAndDispose(new AllocationAdjustmentDialog(adjustmentsSecurity)) == DialogResult.OK;
		}

		AllocationAdjustmentsSecurity AdjustmentsSecurity
		{
			get { return (AllocationAdjustmentsSecurity)BusinessEntity; }
		}

		bool IsCurrentUserAllowedToAdjustAllocations(SecurityCore security)
		{
			return (AdjustmentsSecurity.Adjustments.All(x => ((AllocationAdjustment)x).Consol.IsGatewaySendingAgent) && IsAllowedToAdjustGatewayAllocations(security))
				   || (AdjustmentsSecurity.Adjustments.All(x => !((AllocationAdjustment)x).Consol.IsGatewaySendingAgent) && IsAllowedToAdjustAllocations(security))
				   || (IsAllowedToAdjustGatewayAllocations(security) && IsAllowedToAdjustAllocations(security));
		}

		static bool IsAllowedToAdjustAllocations(SecurityCore security)
		{
			return security != null && security.ConsolPreAllocationEditing.IsAllowed;
		}

		static bool IsAllowedToAdjustGatewayAllocations(SecurityCore security)
		{
			return security != null && security.GatewayConsolPreAllocationEditing.IsAllowed;
		}

		#region Validate And Adjust

		void ValidateAndAdjust()
		{
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				if (!CheckCredentials())
				{
					AdjustmentsSecurity.Password = ZString.Empty;
				}
				else
				{
					var authorizer = GlbStaff.CurrentUser;
					if (!IsCurrentUserAllowedToAdjustAllocations(Env.Security))
					{
						authorizer = new BusinessObjectFactoryProvider().Current.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, AdjustmentsSecurity.Login));
					}

					AdjustmentsSecurity.AdjustAll(authorizer);

					AdjustmentsSecurity.Password = ZString.Empty;
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		bool CheckCredentials()
		{
			if (IsCurrentUserAllowedToAdjustAllocations(Env.Security))
			{
				return true;
			}

			if (AdjustmentsSecurity.UserSecurity == null)
			{
				Globals.Message.ShowError(Res.GetString("90e2fd93-c0ac-4e81-8762-cb0252910332", "Invalid username / password or expired password."));
			}
			else if (!IsCurrentUserAllowedToAdjustAllocations(AdjustmentsSecurity.UserSecurity))
			{
				Globals.Message.ShowError(Res.GetString("aa974f1f-95fc-45f9-8b58-9280faedea53", "{0} is not authorized to adjust the allocations.", AdjustmentsSecurity.Login));
			}
			else
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Events

		void AdjustButton_Click(object sender, EventArgs e)
		{
			ValidateAndAdjust();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#region Implementation

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion
	}
}
