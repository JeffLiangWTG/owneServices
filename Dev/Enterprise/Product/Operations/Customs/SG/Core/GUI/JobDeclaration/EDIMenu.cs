using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		#region Add Menu Items

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			separatorMenuItem = new ZMenuItem("-");
			MenuItems.Add(0, separatorMenuItem);

			if (GlbStaff.CurrentUser.GS_IsDeveloper)
			{
				resetToWorkingMenuItem = AddMenuItem("Reset to Working", ResetToWorkingMenuItem_Click);
			}
			sendCancellationMenuItem = AddMenuItem("Send Cancellation", SendCancellationMenuItem_Click);
			sendRefundMenuItem = AddMenuItem("Send Refund", SendRefundMenuItem_Click);
			sendAmendmentMenuItem = AddMenuItem("Send Amendment", SendAmendmentMenuItem_Click);
			sendDeclarationMenuItem = AddMenuItem("Send Declaration", SendDeclarationMenuItem_Click);
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMessagingMenuItemsVisibility(new[]
			{
				separatorMenuItem,
				resetToWorkingMenuItem,
				sendCancellationMenuItem,
				sendRefundMenuItem,
				sendAmendmentMenuItem,
				sendDeclarationMenuItem
			});
		}

		void SetMessagingMenuItemsVisibility(MenuItem[] messagingItems)
		{
			var isDeclarationIntegrated = Declaration?.IsDeclarationIntegrated ?? false;
			foreach (var messagingItem in messagingItems)
			{
				if (messagingItem != null)
				{
					messagingItem.Visible = !isDeclarationIntegrated;
				}
			}
		}

		internal MenuItem separatorMenuItem;
		internal MenuItem resetToWorkingMenuItem;
		internal MenuItem sendCancellationMenuItem;
		internal MenuItem sendRefundMenuItem;
		internal MenuItem sendAmendmentMenuItem;
		internal MenuItem sendDeclarationMenuItem;

		MenuItem AddMenuItem(string name, EventHandler clickMethod)
		{
			return AddMenuItem(name, clickMethod, 0);
		}

		MenuItem AddMenuItem(string name, EventHandler clickMethod, int index)
		{
			MenuItem menuItem = new ZMenuItem(name, new EventHandler(clickMethod));
			menuItem.Name = menuItem.Text;
			MenuItems.Add(index, menuItem);
			return menuItem;
		}

		#endregion

		#region Menuitems

		void SendDeclarationMenuItem_Click(object sender, EventArgs e)
		{
			DoActions((x, y) => SendDeclaration(x, y));
		}

		void SendAmendmentMenuItem_Click(object sender, EventArgs e)
		{
			DoActions((x, y) => SendAmendment(x, y));
		}

		void SendRefundMenuItem_Click(object sender, EventArgs e)
		{
			DoActions((x, y) => SendRefund(x, y));
		}

		void SendCancellationMenuItem_Click(object sender, EventArgs e)
		{
			DoActions((x, y) => SendCancellation(x, y));
		}

		void DoActions(Action<IMessageManager, string> action)
		{
			action(Declaration.SG4MessageManager, SGEDIMessage.SG4ApplicationCode);
		}

		#endregion

		#region Send Declaration

		void SendDeclaration(IMessageManager messageManager, ZString applicationCode)
		{
			if (CanSendDeclaration(Declaration) && CanSendAnyMessage(Declaration, true))
			{
				SaveDocManager();

				var additionalMessageInformation = new AdditionalMessageInformation(Declaration.GetLastSentMessage_PermitOnly(), Declaration.AllEDocs, AdditionalMessageInformation.BoundFormTypes.Declaration, Declaration.Factory, applicationCode);
				Declaration.AdditionalMessageInformation = additionalMessageInformation;
				using (var form = new PromptDeclarationForm(additionalMessageInformation))
				{
					ShowPromptForm(messageManager.SendOriginalMessages, form, "The declaration has been queued to send.\nDo you want to close the form?", "Declaration Queued");
				}
			}
		}

		void ShowPromptForm(Func<Customs.Business.ISendsMessagesToCustoms, bool> messageAction, BasePromtForm form, ZString popupMessage, ZString caption)
		{
			MainMenu mainMenu = null;
			Form declarationForm = null;
			if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
			{
				var messageActionSent = messageAction(Declaration.MessageInitiator);
				if (messageActionSent && !Globals.IsTest && Declaration.MessageInitiator.YesNoQuery(popupMessage, caption))
				{
					try
					{
						mainMenu = GetMainMenu();
						declarationForm = mainMenu.GetForm();
						declarationForm.Close();
					}
					catch (NullReferenceException ex)
					{
						ErrorReporter.ReportOnce("C268A991-9719-49CE-977C-C8E844081E76", $@"{ex.Message}, 
mainMenu is null: {mainMenu == null},
declarationForm is null: {declarationForm == null}");
					}
				}
			}
		}

		bool CanSendDeclaration(JobDeclaration declaration)
		{
			if (declaration.ActiveEntryHeaders.Count > 0)
			{
				foreach (CusEntryHeader cusEntryHeader in declaration.ActiveEntryHeaders)
				{
					if (cusEntryHeader.IsWaitingForResponse)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nCurrently awaiting a response from Singapore Customs.");
						return false;
					}

					if (!cusEntryHeader.CanSendOriginal)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nA permit has already been received.");
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region Send Amendment

		void SendAmendment(IMessageManager messageManager, ZString applicationCode)
		{
			var declaration = Declaration;

			if (CanSendAmendment(declaration) && CanSendAnyMessage(declaration, true))
			{
				SaveDocManager();

				var additionalMessageInformation = new AdditionalMessageInformation(declaration.GetLastSentMessage_PermitOnly(), declaration.AllEDocs, AdditionalMessageInformation.BoundFormTypes.Amendment, declaration.Factory, applicationCode);
				declaration.AdditionalMessageInformation = additionalMessageInformation;

				using (var form = new PromtAmendmentForm(additionalMessageInformation, declaration.SupportExtendingAmendmentReason))
				{
					ShowPromptForm(messageManager.SendAmendmentMessages, form, "The amendment has been queued to send.\nDo you want to close the form?", "Amendment Queued");
				}
			}
		}

		bool CanSendAmendment(JobDeclaration declaration)
		{
			if (declaration.ActiveEntryHeaders.Count > 0)
			{
				foreach (CusEntryHeader cusEntryHeader in declaration.ActiveEntryHeaders)
				{
					if (cusEntryHeader.IsWaitingForResponse)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nCurrently awaiting a response from Singapore Customs.");
						return false;
					}

					if (cusEntryHeader.CanSendOriginal && cusEntryHeader.CH_Status != Core.SGConstants.DeclarationStatus.CancellationAccepted)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nA permit has not yet been received.");
						return false;
					}

					if (cusEntryHeader.IsCancelledByStatus)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nThe permit has already been cancelled.");
						return false;
					}
				}
			}

			if (declaration.ActiveEntryHeaders.Count == 0)
			{
				declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nA declaration has not yet been sent.");
				return false;
			}

			return true;
		}

		#endregion

		#region Send Refund

		void SendRefund(IMessageManager messageManager, ZString applicationCode)
		{
			if (CanSendRefund(Declaration) && CanSendAnyMessage(Declaration, false))
			{
				SaveDocManager();

				var additionalMessageInformation = new RefundAdditionalMessageInformation(Declaration.GetLastSentMessage_PermitOnly(), Declaration, Declaration.AllEDocs, Declaration.Factory, applicationCode);
				Declaration.AdditionalMessageInformation = additionalMessageInformation;

				using (PromptRefundForm form = new PromptRefundForm(additionalMessageInformation))
				{
					ShowPromptForm(messageManager.SendRefundMessage, form, "The refund request has been queued to send.\nDo you want to close the form?", "Refund Queued");
				}
			}
		}

		bool CanSendRefund(JobDeclaration declaration)
		{
			if (declaration.JE_MessageType != MessageTypeCodeList.Codes.IPT)
			{
				declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nRefund is only valid for a inward payment (IPT) declaration.");
				return false;
			}

			if (declaration.ActiveEntryHeaders.Count > 0)
			{
				foreach (CusEntryHeader cusEntryHeader in declaration.ActiveEntryHeaders)
				{
					if (cusEntryHeader.IsWaitingForResponse)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nCurrently awaiting a response from Singapore Customs.");
						return false;
					}

					if (cusEntryHeader.CanSendOriginal && cusEntryHeader.CH_Status != Core.SGConstants.DeclarationStatus.CancellationAccepted)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nA permit has not yet been received.");
						return false;
					}

					if (cusEntryHeader.IsCancelledByStatus)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nThe permit has already been cancelled.");
						return false;
					}
				}
			}

			if (declaration.ActiveEntryHeaders.Count == 0)
			{
				declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nA declaration has not yet been sent.");
				return false;
			}

			return true;
		}

		#endregion

		#region Send Cancellation

		void SendCancellation(IMessageManager messageManager, ZString applicationCode)
		{
			if (CanSendCancellation(Declaration) && CanSendAnyMessage(Declaration, false))
			{
				SaveDocManager();

				var additionalMessageInformation = new AdditionalMessageInformation(Declaration.GetLastSentMessage_PermitOnly(), Declaration.AllEDocs, AdditionalMessageInformation.BoundFormTypes.Cancellation, Declaration.Factory, applicationCode);
				Declaration.AdditionalMessageInformation = additionalMessageInformation;

				using (var form = new PromtCancellationForm(additionalMessageInformation))
				{
					ShowPromptForm(messageManager.SendCancellationMessages, form, "The cancellation has been queued to send.\nDo you want to close the form?", "Cancellation Queued");
				}
			}
		}

		bool CanSendCancellation(JobDeclaration declaration)
		{
			if (declaration.ActiveEntryHeaders.Count > 0)
			{
				foreach (CusEntryHeader cusEntryHeader in declaration.ActiveEntryHeaders)
				{
					if (cusEntryHeader.IsWaitingForResponse)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nCurrently awaiting a response from Singapore Customs.");
						return false;
					}

					if (cusEntryHeader.IsCancelledByStatus)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nThe permit has already been cancelled.");
						return false;
					}

					if (!cusEntryHeader.CanSendWithdrawal)
					{
						declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nA permit has not yet been received.");
						return false;
					}
				}
			}

			if (declaration.ActiveEntryHeaders.Count == 0)
			{
				declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("Sending is not allowed.\r\n\r\nA declaration has not yet been sent.");
				return false;
			}

			return true;
		}

		#endregion

		#region Reset To Working

		void ResetToWorkingMenuItem_Click(object sender, EventArgs e)
		{
			Declaration.ResetToWorking();
		}

		#endregion

		#region Calculate Payables

		protected override bool DisplayGenerateEntriesMenuOption
		{
			get { return true; }
		}

		protected override string GenerateEntriesMenuOptionText
		{
			get { return "Calculate Payables (Merge)"; }
		}

		#endregion

		#region Validation

		bool CanSendAnyMessage(JobDeclaration declaration, bool checkMessageErrors)
		{
			bool result = declaration != null && ValidateAgentAndBrokerDetails();

			if (result)
			{
				declaration.LoadChildEditableObjects();
				declaration.RunPreSaveValidation();
				if (declaration.MergedLinesCount > SGConstants.MaxEntryLines)
				{
					Globals.Message.Show(SGConstants.MaxLinesValidation.CannotSendExceedsMaxLines, "Cannot Send Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
					result = false;
				}
				else if (declaration.HasErrors)
				{
					Globals.Message.Show("The message cannot be sent. Please fix all errors on the form before trying to send a message", "Cannot Send Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
					result = false;
				}
				else if (checkMessageErrors && declaration.HasMessageErrors)
				{
					string message = "It is advisable that you fix all Message Errors on the form before sending the message to Customs\r\n"
					 + "If you fail to do this, it is most likely that your message will be rejected by Customs\r\n\r\n"
					 + "Do you want to proceed?";

					if (Globals.Message.ShowConfirmation(message, "Cannot Send Message", "send", MessageBoxIcon.Warning) == DialogResult.Cancel)
					{
						result = false;
					}
				}
			}

			return result;
		}

		bool ValidateAgentAndBrokerDetails()
		{
			StringBuilder stringBuilder = new StringBuilder();

			if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
			{
				stringBuilder.Append("\r\n");
				stringBuilder.Append("This company does not have a Unique Entity Number (UEN).\r\n");
				stringBuilder.Append("Please edit the Company record, (Customs Reg No:), to add your Unique Entity Number.\r\n");
			}

			OrgHeader orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			ZString companyCRN = orgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber);
			ZString companyUEN = orgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber);

			if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo == companyCRN)
			{
				stringBuilder.Append("\r\n");
				stringBuilder.Append("The 'Customs Reg No:' field on the Company record has not been updated to your company Unique Entity Number (UEN).\r\n");
				stringBuilder.Append("In order to send messages to TradeNet following the SG Government implementation change over to use of UEN identification values, your Company record must be adjusted.\r\n");
				stringBuilder.Append("Please edit the Company record, (Customs Reg No:), to change this value from you CRN to your UEN.\r\n");
			}
			else if (companyUEN.IsEmpty && (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.Length != 9 && GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.Length != 10))
			{
				stringBuilder.Append("\r\n");
				stringBuilder.Append("The 'Customs Reg No:' field on the Company record must have your company Unique Entity Number (UEN).\r\n");
				stringBuilder.Append("Please edit the Company record, (Customs Reg No:), to change this value to your Unique Entity Number.\r\n");
			}

			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(0, 1);
				Globals.Message.Show(stringBuilder.ToString(), "Cannot Send Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return stringBuilder.Length == 0;
		}

		#endregion

		#region Save EDocs

		void SaveDocManager()
		{
			try
			{
				if (Declaration.JE_JS == ZGuid.Empty)
				{
					Declaration.DocManagerInfo.MasterFactory.Save();
				}
				else
				{
					Declaration.Shipment.DocManagerInfo.MasterFactory.Save();
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		#endregion
	}
}
