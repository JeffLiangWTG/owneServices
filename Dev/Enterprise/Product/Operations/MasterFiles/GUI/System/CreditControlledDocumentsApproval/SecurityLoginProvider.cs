using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public class SecurityLoginProvider : ISecurityLoginProvider
	{
		public SecurityLoginProvider(string caption)
		{
			Argument.NotNull(caption, "caption");
			this.caption = caption;
		}
		readonly string caption;

		public void ShowDocumentLoginForDocuments(EventArgs args)
		{
			var e = args as SecurityLoginEventArgs;
			if (e != null)
			{
				ShowDocumentLoginForDocumentsCore(e);
			}
		}

#if DEBUG
		public
#endif

		SecurityLogin loginBisObject;

		void ShowDocumentLoginForDocumentsCore(SecurityLoginEventArgs e)
		{
			var docsEventArgs = e as SecurityLoginEventArgsForDocumentApproval;
			if (loginBisObject == null) // normally null except in tests where the login object is mocked
			{
				loginBisObject = new SecurityLogin(e.SecurityCheckPoints);
			}

			loginBisObject.HideApprovalRequestButton = e.HideApprovalRequestButton;

			InitializeDocumentApprovalHandlers();

			if (docsEventArgs?.ParentBusinessObject != null && docsEventArgs.ParentBusinessObject.Factory.IsInTransaction)
			{
				e.IsAllowedToProceed = false;
				e.MessageToShowWhenNotAllowed = ResString.GetMultilingualString("E58901D6-4889-4237-881A-B19A18D156CD", "Cannot perform document login during data save process. Automatically rejected the login request.");
			}
			else
			{
				if (docsEventArgs != null && docsEventArgs.IsDPSFreightMovementRestricted && docsEventArgs.IsAccountingRestricted)
				{
					ShowDocumentLoginForMultiStep(e);
				}
				else
				{
					var result = DialogResult.No;

					var isDPSRestricted = docsEventArgs != null && ComplianceRiskApprovalHandler.IsRestrictedForMultiStep(docsEventArgs);
					var isExternalAccountingSystemUsed = docsEventArgs != null ? docsEventArgs.IsExternalAccountingSystemUsed : ZBool.False;
					var customMessageBoxEvent = e as ICustomMessageBox;
					if (customMessageBoxEvent?.CustomMessageBox != null)
					{
						result = (DialogResult)customMessageBoxEvent.CustomMessageBox(customMessageBoxEvent.LoginPromptMessage, caption);
					}
					else if (docsEventArgs != null && docsEventArgs.IsCustomsSubmission && (isDPSRestricted || isExternalAccountingSystemUsed))
					{
						if (isDPSRestricted)
						{
							result = ShowCustomsConfirmationDialogForDPSRestrictedDocuments(docsEventArgs);
						}
						else if (isExternalAccountingSystemUsed) //skip showing any yes/no message
						{
							result = DialogResult.Yes;
						}
					}
					else
					{
						result = Globals.Message.Show(e.LoginPromptMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
					}

					e.IsAllowedToProceed = ZBool.False;
					if (result == DialogResult.Yes)
					{
						HandleloginFormDialogForSingleStep(e);
					}
				}

				if (!e.IsAllowedToProceed)
				{
					if (loginBisObject.MessageToShowWhenNotPrinting.IsEmpty)
					{
						e.MessageToShowWhenNotAllowed = MultilingualString.Join(string.Empty, (NoResString)caption, (NoResString)" ", SecurityLogin.CancelledText);
					}
					else
					{
						e.MessageToShowWhenNotAllowed = (NoResString)loginBisObject.MessageToShowWhenNotPrinting;
					}
				}
			}

			loginBisObject = null;
		}

		void InitializeDocumentApprovalHandlers()
		{
			foreach (var handler in ApprovalHandlers)
			{
				handler.Initialize(loginBisObject, ShowLoginDocumentLoginForm);
			}
		}

		LoginDialogResult ShowLoginDocumentLoginForm() => MapDialogResultToLoginDialogResult(ZFormModaliser.ShowDialogAndDispose(new DocumentLoginForm(loginBisObject)));

		void HandleloginFormDialogForSingleStep(SecurityLoginEventArgs e)
		{
			var docApprovalArgs = e as SecurityLoginEventArgsForDocumentApproval;
			if (docApprovalArgs == null)
			{
				if (ShowLoginDocumentLoginForm() == LoginDialogResult.Yes)
				{
					e.IsAllowedToProceed = true;
					e.AuthorisingStaffLogin = loginBisObject.Login;
				}
		
				return;
			}

			foreach (var handler in ApprovalHandlers)
			{
				if (handler.IsRestrictedForSingleStep(docApprovalArgs))
				{
					handler.HandleApprovalRequestForSingleStep(docApprovalArgs);
					return;
				}
			}
		}

		void ShowDocumentLoginForMultiStep(SecurityLoginEventArgs e)
		{
			var docsEventArgs = e as SecurityLoginEventArgsForDocumentApproval;
			bool failedOverride = false;
			e.IsAllowedToProceed = false;
			DialogResult result;
			while (!failedOverride && !e.IsAllowedToProceed)
			{
				SecurityLoginEventArgsWithCustomMessageBox customMessageBoxEvent = e as SecurityLoginEventArgsWithCustomMessageBox;
				if (customMessageBoxEvent != null && customMessageBoxEvent.CustomMessageBox != null)
				{
					result = (DialogResult)customMessageBoxEvent.CustomMessageBox(customMessageBoxEvent.LoginPromptMessage, caption);
				}
				else
				{
					MultilingualString promptMessage = e.LoginPromptMessage;
					promptMessage = MultilingualString.Join(string.Empty, promptMessage, ResString.GetMultilingualString("1112CB3F-7E1B-4BB8-9A32-FF28A4591D66", @"Note: The staff user that may override this must have security rights to override the following restrictions: "), (NoResString)System.Environment.NewLine);
					foreach (var sec in loginBisObject.SecurityCheckpoints)
					{
						promptMessage = MultilingualString.Join(string.Empty, promptMessage, (NoResString)@"       - ", sec(Env.Security).DisplayText, (NoResString)System.Environment.NewLine);
					}
					result = Globals.Message.Show(promptMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				}

				if (result == DialogResult.Yes && docsEventArgs != null && docsEventArgs.IsCustomsSubmission && ComplianceRiskApprovalHandler.IsRestrictedForMultiStep(docsEventArgs))
				{
					result = ShowCustomsConfirmationDialogForDPSRestrictedDocuments(docsEventArgs);
				}

				if (result == DialogResult.Yes)
				{
					failedOverride = HandleLogInDialogForMultiStep(e);
				}
				else
				{
					failedOverride = true;
					e.IsAllowedToProceed = false;
				}
			}
		}

		bool HandleLogInDialogForMultiStep(SecurityLoginEventArgs e)
		{
			var docApprovalArgs = e as SecurityLoginEventArgsForDocumentApproval;
			bool failedOverride = false;
			if (docApprovalArgs == null)
			{
				loginBisObject.LoginInfo.ClearValue();
				loginBisObject.PasswordInfo.ClearValue();
				loginBisObject.ClearAllNotifications();

				var loginFormDialogResult = ZFormModaliser.ShowDialogAndDispose(new DocumentLoginForm(loginBisObject));

				if (loginFormDialogResult == DialogResult.Yes)
				{
					if (loginBisObject.SecurityCheckpoints.Count == 0)
					{
						e.IsAllowedToProceed = true;
						e.AuthorisingStaffLogin = loginBisObject.Login;
					}
				}
				else if (loginFormDialogResult != DialogResult.Ignore)
				{
					failedOverride = true;
					e.IsAllowedToProceed = false;
				}
			}
			else
			{
				bool displayLoginDialogAgain;
				do
				{
					displayLoginDialogAgain = false;
					loginBisObject.LoginInfo.ClearValue();
					loginBisObject.PasswordInfo.ClearValue();
					loginBisObject.ClearAllNotifications();

					var loginFormDialogResult = ZFormModaliser.ShowDialogAndDispose(new DocumentLoginForm(loginBisObject));

					if (loginFormDialogResult == DialogResult.Yes)
					{
						if (loginBisObject.SecurityCheckpoints.Count == 0)
						{
							if (docApprovalArgs.IsExternalAccountingSystemUsed)
							{
								loginFormDialogResult = Globals.Message.Show(WarningMessageForExternalAccountingSystemApproval, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, DialogResult.Yes) == DialogResult.Yes ? DialogResult.Ignore : DialogResult.No;
							}
							else
							{
								e.IsAllowedToProceed = true;
								e.AuthorisingStaffLogin = loginBisObject.Login;
							}
						}
					}

					if (loginFormDialogResult == DialogResult.Ignore)
					{
						if (ComplianceRiskApprovalHandler.IsRestrictedForMultiStep(docApprovalArgs))
						{
							ComplianceRiskApprovalHandler.HandleApprovalRequestForMultiStep(docApprovalArgs);
							displayLoginDialogAgain = true;
						}
						else if (docApprovalArgs.IsAccountingRestricted)
						{
							AccountingApprovalHandler.HandleApprovalRequestForMultiStep(docApprovalArgs);
							failedOverride = true;
							e.IsAllowedToProceed = false;
						}
					}
					else if (loginFormDialogResult != DialogResult.Yes)
					{
						failedOverride = true;
						e.IsAllowedToProceed = false;
					}
				}
				while (displayLoginDialogAgain);
			}
			return failedOverride;
		}

		IEnumerable<IDocumentApprovalRequestHandler> ApprovalHandlers => approvalHandlers ??= ObjectFactory.Get<ListObject>("DocumentApprovalRequestHandlers").Cast<IDocumentApprovalRequestHandler>();
		IEnumerable<IDocumentApprovalRequestHandler> approvalHandlers;

		IComplianceRiskApprovalRequestHandler ComplianceRiskApprovalHandler => complianceRiskApprovalHandler ??= ApprovalHandlers.OfType<IComplianceRiskApprovalRequestHandler>().FirstOrDefault();
		IComplianceRiskApprovalRequestHandler complianceRiskApprovalHandler;

		IAccountingDocumentApprovalRequestHandler AccountingApprovalHandler => accountingApprovalHandler ??= ApprovalHandlers.OfType<IAccountingDocumentApprovalRequestHandler>().FirstOrDefault();
		IAccountingDocumentApprovalRequestHandler accountingApprovalHandler;

		internal DialogResult ShowCustomsConfirmationDialogForDPSRestrictedDocuments(SecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			var loginDialogResult = ComplianceRiskApprovalHandler.ShowCustomsConfirmationDialogForDPSRestrictedDocuments(docApprovalArgs);
			return MapLoginDialogResultToDialogResult(loginDialogResult);
		}

		internal static MultilingualString WarningMessageForExternalAccountingSystemApproval
		{
			get
			{
				return ResString.GetMultilingualString("B52260B0-AEC6-4A24-9D17-ADD501B7482C", @"You cannot proceed with printing as according to your configuration only external system can evaluate a credit status for this operation. Do you want to request approval?");
			}
		}

		internal static LoginDialogResult MapDialogResultToLoginDialogResult(DialogResult dialogResult)
		{
			return Enum.IsDefined(typeof(LoginDialogResult), (int)dialogResult)
				? (LoginDialogResult)dialogResult
				: LoginDialogResult.None;
		}

		internal static DialogResult MapLoginDialogResultToDialogResult(LoginDialogResult loginDialogResult)
		{
			return Enum.IsDefined(typeof(DialogResult), (int)loginDialogResult)
				? (DialogResult)loginDialogResult
				: DialogResult.None;
		}
	}
}
