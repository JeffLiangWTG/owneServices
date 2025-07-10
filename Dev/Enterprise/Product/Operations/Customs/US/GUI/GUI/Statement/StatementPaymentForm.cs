using System;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using SharedCustomsGUI = Enterprise.Customs.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class StatementPaymentForm : ZChildForm
	{
		public StatementPaymentForm()
		{
		}

		public StatementPaymentForm(StatementPaymentAction action)
			: base(action)
		{
			IsCancelled = true;
			statementHeader = action.StatementHeader;
		}
		readonly CusStatementHeader statementHeader;

		public override string FormCaption
		{
			get { return "Payment Authorization"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		internal void OKButton_Click(object sender, EventArgs e)
		{
			IsCancelled = false;
			var notifications = new StatementMessageSendingValidator().CheckBusinessObjectLevelValidation((StatementPaymentAction)BusinessEntity);
			if (((StatementPaymentAction)BusinessEntity).HasMessageErrors || notifications.Count > 0)
			{
				if (!Env.Security.USCustomsImportStatementSendWithMessageErrors.IsAllowed)
				{
					Env.Security.USCustomsImportStatementSendWithMessageErrors.ShowError();
					IsCancelled = true;
				}
				else
				{
					if (!StatementMessagesHandler.ContinueWithNotifications(notifications, MessageErrorsExistHeaderText, MessageErrorConfirmationQuestionText))
					{
						IsCancelled = true;
					}
				}
			}

			if (!SharedCustomsGUI.SupervisorOverridesHelper.IsSupervisorApproved(SupervisorOverrides, statementHeader.Logs))
			{
				IsCancelled = true;
			}

			Close();
		}
		public bool IsCancelled;

		protected virtual SupervisorOverrides SupervisorOverrides
		{
			get { return new SupervisorOverrides(BusinessEntity, SupervisorOverridesContext.SendingPaymentAuthorizationMessage); }
		}

		internal void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		StatementMessagesHandler StatementMessagesHandler
		{
			get { return statementMessagesHandler ?? (statementMessagesHandler = new StatementMessagesHandler()); }
		}
		StatementMessagesHandler statementMessagesHandler;

		internal const string MessageErrorsExistHeaderText = "It is likely that your message(s) have the following message errors:";
		internal const string MessageErrorConfirmationQuestionText = "Do you want to send the message(s) despite these errors?";
	}
}
