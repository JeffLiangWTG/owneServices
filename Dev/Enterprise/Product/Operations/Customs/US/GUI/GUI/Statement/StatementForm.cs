using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	public partial class StatementForm : ZTemplateForm, IPostingButtonsProvider
	{
		public StatementForm(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
			this.statementHeader = statementHeader;

			paymentAuthorizationMessage = new ZMenuItem("Send ACH Payment Authorization Message", new EventHandler(PayStatement_Click));
			deletePaymentAuthorization = new ZMenuItem("Delete Payment Authorization", new EventHandler(DeletePaymentAuthorization_Click));
			statementDeleteAddMessage = new ZMenuItem("Statement Delete/Add Message", new EventHandler(StatementDeleteAdd_Click));
			resetPaymentStatusMenu = new ZMenuItem("Reset Payment Status", new EventHandler(ResetPaymentStatusMenu_Click));
			paymentAuthorizationMessageApproval = new ZMenuItem(GetPaymentAuthorizationMenuCaption(), new EventHandler(PaymentAuthorization_Click));

			ZFormMenuStrategy.AddActionsMenuItem(this, paymentAuthorizationMessageApproval);
			ZFormMenuStrategy.AddActionsMenuItem(this, resetPaymentStatusMenu);
			ZFormMenuStrategy.AddActionsMenuItem(this, paymentAuthorizationMessage);
			ZFormMenuStrategy.AddActionsMenuItem(this, deletePaymentAuthorization);
			ZFormMenuStrategy.AddActionsMenuItem(this, statementDeleteAddMessage);

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			WorkflowTabPage.Initialize(statementHeader);

			switchLineStatusMenuItem = new ZMenuItem("Change Line Status", new EventHandler(SwitchLineItemStatus_Click));
			StatementLinesGrid.ContextMenu.MenuItems.Add(StatementLinesGrid.ContextMenu.MenuItems.Count, switchLineStatusMenuItem);
			StatementLinesGrid.ContextMenu.Popup += new EventHandler(UpdateLineStatusMenuItemCaptionAndVisibility);
		}

		readonly CusStatementHeader statementHeader;
		internal readonly MenuItem paymentAuthorizationMessage;
		internal readonly MenuItem deletePaymentAuthorization;
		internal readonly MenuItem statementDeleteAddMessage;
		internal readonly MenuItem resetPaymentStatusMenu;
		internal readonly MenuItem paymentAuthorizationMessageApproval;
		internal readonly MenuItem switchLineStatusMenuItem;
		const string GrantPermissionMenuName = "Grant Authorization Permission";
		const string RevokePermissionMenuName = "Revoke Authorization Permission";

		public override string FormCaption
		{
			get { return (statementHeader.IsPeriodicDailyStatement ? "Periodic " : "") + "Daily Statement - " + statementHeader.B2_StatementNumber; }
		}

		#region Mark Statement Lines as Deleted/Active

		internal void UpdateLineStatusMenuItemCaptionAndVisibility(object sender, EventArgs e)
		{
			switchLineStatusMenuItem.Visible = false;

			var currentLine = (CusStatementLine)StatementLinesGrid.ListManager.GetCurrent();
			if (currentLine != null)
			{
				if (currentLine.IsActive)
				{
					switchLineStatusMenuItem.Text = Res.GetString("98D7FC9C-0F06-43A9-AA22-9033509582B8", "&Mark Line Status as Deleted");
					switchLineStatusMenuItem.Visible = true;
				}
				else if (currentLine.IsStatusDeleted || currentLine.IsDeletionPending)
				{
					switchLineStatusMenuItem.Text = Res.GetString("FD2AC1AE-F250-473E-BDC6-5925B5A7A0B3", "&Mark Line Status as Active");
					switchLineStatusMenuItem.Visible = true;
				}
			}
		}

		void SwitchLineItemStatus_Click(object sender, EventArgs e)
		{
			if (CheckIfValidToChangeLineStatus() && StatementLinesGrid.ListManager.GetCurrent() != null)
			{
				if (StatementLinesGrid.SelectedElements.Length > 1)
				{
					Globals.Message.ShowError(Res.GetString("F9641E56-9751-4BB2-9FF1-B63D2E4F6A70", "You have selected many lines. Please select only one line."));
				}
				else if (!Env.Security.USCustomsImportStatementModify.IsAllowed)
				{
					Env.Security.USCustomsImportStatementModify.ShowError();
				}
				else
				{
					var currentLine = (CusStatementLine)StatementLinesGrid.ListManager.GetCurrent();
					if (currentLine.IsActive)
					{
						currentLine.ChangeLineStatus(false);
					}
					else if (currentLine.IsStatusDeleted || currentLine.IsDeletionPending)
					{
						currentLine.ChangeLineStatus(true);
					}
				}
			}
		}

		bool CheckIfValidToChangeLineStatus()
		{
			if (statementHeader.IsFinal)
			{
				Globals.Message.ShowError(Res.GetString("6CB97CC1-E9EE-410D-910C-AED8E1EFC7B8", "This statement is final, and changing line status is not allowed."));
				return false;
			}
			return true;
		}

		#endregion

		#region Send Payment

		protected void PayStatement_Click(object sender, EventArgs e)
		{
			if (CheckIfValidToSendAuthorizationOrPayment())
			{
				var manager = new AutomatedClearinghouseMessageManager();
				var notifications = new StatementMessageSendingValidator().GetNotificationsForPayment(statementHeader);
				if (StatementMessagesHandler.ContinueWithNotifications(notifications))
				{
					var action = GetStatementAction(statementHeader);
					action.RunPreSaveValidation();
					if (ShouldContinueToSendPaymentMessage(action))
					{
						manager.SendAuthorisation(action);
						try
						{
							statementHeader.Factory.Save();
							Globals.Message.ShowInformation(messageToShowAfterPaymentSent, "Message(s) Sent");
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
		}

		bool CheckIfValidToSendAuthorizationOrPayment()
		{
			var result = true;
			var errorMessageText = statementHeader.GetCheckIfValidToSendAuthorizationOrPaymentMessage();
			if (errorMessageText != null && !errorMessageText.Item1.IsEmpty)
			{
				Globals.Message.ShowError(errorMessageText.Item1, errorMessageText.Item2);
				result = false;
			}
			return result;
		}

		protected virtual StatementPaymentAction GetStatementAction(CusStatementHeader statementHeader)
		{
			return new StatementPaymentAction(statementHeader);
		}

		protected virtual bool ShouldContinueToSendPaymentMessage(StatementPaymentAction action)
		{
			bool result = false;

			using (StatementPaymentForm form = new StatementPaymentForm(action))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				result = !form.IsCancelled;
			}

			return result;
		}

		protected readonly string messageToShowAfterPaymentSent = "Statement Payment message Sent. Statement Header Status set to Payment In Progress\r\nEntry will be updated when these messages are responded successfully.";
		public const string ThereIsANotification = "There is a notification. Are you sure you wish to continue?";

		protected void DeletePaymentAuthorization_Click(object sender, EventArgs e)
		{
			if (CheckIfValidToSendAuthorizationOrPayment())
			{
				var manager = new AutomatedClearinghouseMessageManager();
				var notifications = new StatementMessageSendingValidator().GetNotificationsForDeletePaymentAuthorization(statementHeader);
				if (StatementMessagesHandler.ContinueWithNotifications(notifications))
				{
					var action = StatementPaymentAction.NewForNegation(statementHeader);

					manager.SendAuthorisation(action);
					try
					{
						statementHeader.Factory.Save();
						Globals.Message.ShowInformation(DeletePaymentAuthorizationSent, "Message(s) Sent");
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}
		protected readonly string DeletePaymentAuthorizationSent = "Delete Payment Authorization message Sent. \r\nEntry will be updated when this message is responded successfully.";

		#endregion

		#region Statement Delete/Add

		protected void StatementDeleteAdd_Click(object sender, EventArgs e)
		{
			StatementDeleteAndSendingActionCollection messageSendingActions = GetActionCollection(statementHeader);
			MessageSendingNotificationCollection notifications = new StatementMessageSendingValidator().GetNotificationsForStatementDeleteAdd(statementHeader);
			StatementMessagesHandler.SendDeleteAddMessage(messageSendingActions, notifications, statementHeader.Factory);
		}

		StatementDeleteAndSendingActionCollection GetActionCollection(CusStatementHeader statementHeader)
		{
			return new StatementDeleteAndSendingActionCollection(statementHeader);
		}

		#endregion

		#region ResetPaymentStatusMenu_Click

		void ResetPaymentStatusMenu_Click(object sender, EventArgs e)
		{
			MessageSendingNotificationCollection notifications = new StatementMessageSendingValidator().GetNotificationsForResettingPaymentStatus(statementHeader);

			bool isOKToProceed = true;

			if (notifications.ContainsWarning())
			{
				isOKToProceed = false;

				if (Globals.Message.ShowConfirmation(notifications.WarningNotificationsAsString(), "Reset Payment Status", "Yes", MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
				{
					isOKToProceed = true;
				}
			}

			if (isOKToProceed)
			{
				statementHeader.ResetPaymentStatus();
				try
				{
					statementHeader.Factory.Save();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		#endregion

		#region ApprovePaymentAuthorization_Click

		string GetPaymentAuthorizationMenuCaption()
		{
			return PaymentSendingAuthorized ? RevokePermissionMenuName : GrantPermissionMenuName;
		}

		bool PaymentSendingAuthorized
		{
			get { return statementHeader != null && statementHeader.ApprovalActionAuthorised; }
		}

		void PaymentAuthorization_Click(object sender, EventArgs e)
		{
			var companyPK = statementHeader.Company != null ? statementHeader.Company.PK : GlbCompany.CurrentCompany.PK;
			if (USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				if (!Env.Security.USCustomsImportStatementModify.IsAllowed)
				{
					Env.Security.USCustomsImportStatementModify.ShowError();
				}
				else
				{
					var action = GetStatementAction(statementHeader);
					var notifications = new StatementMessageSendingValidator().CheckBusinessObjectLevelValidation(action);
					if (StatementMessagesHandler.ContinueWithNotifications(notifications, MessageErrorsExistHeaderText, MessageErrorConfirmationQuestionText))
					{
						statementHeader.AddAuthorisationLog();
						paymentAuthorizationMessageApproval.Text = GetPaymentAuthorizationMenuCaption();
						var notificationText = PaymentSendingAuthorized ? "Permission to Authorize Payment Granted." : "Permission to Authorize Payment Revoked.";
						Globals.Message.ShowInformation(notificationText, "Action Authorised");
					}
				}
			}
			else
			{
				var notification = "This action is inactive. To activate, set up the registry item in Customs -> United States of America -> Import -> ABI -> Statement -> Require Statement Authorization Approval.";
				Globals.Message.ShowInformation(notification, "Action Not Applicable");
			}
		}

		const string MessageErrorsExistHeaderText = "Please Review the following message errors before complete this action:";
		const string MessageErrorConfirmationQuestionText = "Do you want to continue despite these errors?";

		#endregion

		#region Implementation

		StatementMessagesHandler StatementMessagesHandler
		{
			get { return statementMessagesHandler ?? (statementMessagesHandler = new StatementMessagesHandler()); }
		}
		StatementMessagesHandler statementMessagesHandler;

		#endregion

		#region IPostingButtonsProvider Members

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}

		#endregion

		internal ZTabControl MainTabControlForTesting => MainTabControl;
	}
}
