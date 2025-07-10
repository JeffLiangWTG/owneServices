using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class NonMatchingAgentsDialog : ZChildForm
	{
		public NonMatchingAgentsDialog(NonMatchingAgentsSecurity agentSecurity)
			: base(agentSecurity)
		{
			InitializeComponent();
			DialogResult = DialogResult.Cancel;

			MessageLabel.Text = agentSecurity.Message;

			if (IsCurrentUserAllowedToAttach)
			{
				AuthorizationGroupBox.Enabled = false;
				AuthorizationGroupBox.Visible = false;
			}
			else
			{
				var errorMessage = Env.Security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.ErrorMessageForNotAllowed;
				MessageLabel.Text += "\r\n\r\n" + errorMessage + "\r\n\r\n" + Res.GetString("61aece58-87de-4182-9f25-212ef1a5cb55", "Supervisor access is required to proceed with the save or cancel to make the necessary changes.");
			}
		}

		public static bool CheckAndConfirm(IEnumerable<CommonConsol> consols, IEnumerable<CommonShipment> shipments)
		{
			bool result = true;
			if (consols.Any() && shipments.Any())
			{
				NonMatchingAgentsSecurity agentSecurity = new NonMatchingAgentsSecurity(consols, shipments);

				if (!agentSecurity.Message.IsEmpty)
				{
					result = (ZFormModaliser.ShowDialogAndDispose(new NonMatchingAgentsDialog(agentSecurity)) == DialogResult.OK);
				}
			}

			return result;
		}

		NonMatchingAgentsSecurity AgentsSecurity
		{
			get { return (NonMatchingAgentsSecurity)BusinessEntity; }
		}

		#region IsAllowed

		static bool IsCurrentUserAllowedToAttach
		{
			get { return IsAllowedToAttach(Env.Security); }
		}

		static bool IsAllowedToAttach(SecurityCore security)
		{
			return security != null && security.AllowAttachShipmentsWithNonMatchingSendingReceivingAgents.IsAllowed;
		}

		#endregion

		#region Validate And Attach

		void ValidateAndAttach()
		{
			if (!CheckCredentials())
			{
				AgentsSecurity.Password = "";
			}
			else
			{
				AgentsSecurity.Password = "";
				DialogResult = DialogResult.OK;
				Close();
			}
		}
		bool CheckCredentials()
		{
			if (IsCurrentUserAllowedToAttach)
			{
				return true;
			}

			if (AgentsSecurity.UserSecurity == null)
			{
				Globals.Message.ShowError(Res.GetString("02ac28c1-db45-4a6f-ade0-6f42e48c5e48", "Invalid username / password or expired password."));
			}
			else if (!IsAllowedToAttach(AgentsSecurity.UserSecurity))
			{
				Globals.Message.ShowError(Res.GetString("aede5e10-d9a0-4136-b614-610635a57b28", "{0} is not authorized to attach shipments with consols.", AgentsSecurity.Login));
			}
			else
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Events

		void AttachButton_Click(object sender, EventArgs e)
		{
			ValidateAndAttach();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
			Close();
		}

		#endregion

		#region Implementation

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion
	}
}
