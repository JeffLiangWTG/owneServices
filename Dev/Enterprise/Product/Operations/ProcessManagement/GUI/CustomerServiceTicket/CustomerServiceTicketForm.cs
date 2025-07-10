using System;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.EConversation.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class CustomerServiceTicketForm : RelatedItemsSupportableFormBase
	{
		public CustomerServiceTicketForm(WorkRequest request)
			: base(request)
		{
			InitializeComponent();

			if (request != null)
			{
				WorkflowTabPage.Initialize(request);
			}

			SetUpRelatedTabPage();
			PlugIns.Add(ControllerIDs.eConversationPlugIn);
			PlugIns.Add(ControllerIDs.JobInvoicing);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			ActionsMenuItem.Popup += ActionsMenuItem_Popup;
		}

		protected new WorkRequest BusinessEntity => (WorkRequest)base.BusinessEntity;

		#region ZTemplateForm Overrides

		protected override bool SupportsEDocs => true;

		public override string FormCaption => BusinessEntity?.HumanReadableName ?? WorkRequest.SingularName;

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(factories);
			DetailsTabControl.SetConversationBinding(BusinessEntity);
		}

		protected override void CustomisePluginTab(ZTabPagePlugIn tabPage, ControllerID pluginControllerID)
		{
			base.CustomisePluginTab(tabPage, pluginControllerID);

			if (pluginControllerID == ControllerIDs.eConversationPlugIn)
			{
				tabPage.Text = Res.GetString("43690d98-4c71-4386-876e-555d8c0f4a08", "eConversation Participants");
				((EConversationPlugin)tabPage.PlugIn).ViewMode = EConversationViewMode.ShowOnlyParticipants;
			}
		}

		#endregion

		#region Menu Items

		void ActionsMenuItem_Popup(object sender, EventArgs e)
		{
			var ticket = BusinessEntity;

			if (ticket != null)
			{
				if (ticket.WKR_Status == TicketStatusList.Codes.Cancelled)
				{
					AddActionsMenuItem(UnCancelMenuItemText, UnCancelTicket, ref unCancelMenuItem, ref cancelMenuItem);
				}
				else
				{
					AddActionsMenuItem(CancelMenuItemText, CancelTicket, ref cancelMenuItem, ref unCancelMenuItem);
				}
			}
		}

		ZMenuItem cancelMenuItem;
		ZMenuItem unCancelMenuItem;

		void AddActionsMenuItem(string text, Action clickAction, ref ZMenuItem menuItemToAdd, ref ZMenuItem menuItemToRemove)
		{
			if (menuItemToAdd == null)
			{
				menuItemToAdd = new ZMenuItem(text, (s, e) => clickAction());

				ActionsMenuItem.MenuItems.Add(menuItemToAdd);
			}

			if (menuItemToRemove != null)
			{
				ActionsMenuItem.MenuItems.Remove(menuItemToRemove);
				menuItemToRemove.Dispose();
				menuItemToRemove = null;
			}
		}

		void CancelTicket()
		{
			var ticket = BusinessEntity;

			if (ticket != null)
			{
				if (ticket.HasIncompleteWorkItems)
				{
					var userResponse = Globals.Message.Show(
						message: Res.GetString("CustomerServiceTicketForm.CancelTicketMessage", "There are incomplete Work Items attached to this ticket. Should these also be canceled?"),
						caption: Res.GetString("CustomerServiceTicketForm.CancelTicketCaption", "Cancel incomplete Work Items?"),
						buttons: MessageBoxButtons.YesNoCancel,
						icon: MessageBoxIcon.Exclamation,
						defaultResult: DialogResult.No
						);

					if (userResponse == DialogResult.Yes)
					{
						ticket.Cancel(shouldCancelAttachedWorkItems: true);
					}
					else if (userResponse == DialogResult.No)
					{
						ticket.Cancel(shouldCancelAttachedWorkItems: false);
					}
				}
				else
				{
					ticket.Cancel(shouldCancelAttachedWorkItems: false);
				}
			}
		}

		void UnCancelTicket()
		{
			BusinessEntity?.UnCancel();
		}

		static string CancelMenuItemText => Res.GetString("CustomerServiceTicketForm.Cancel", "Cancel Ticket");
		static string UnCancelMenuItemText => Res.GetString("CustomerServiceTicketForm.UnCancel", "Un-Cancel Ticket");

		#endregion
	}
}
