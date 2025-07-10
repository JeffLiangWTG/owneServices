using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class StatementActionMethodApplicator : OperationalActionMethodApplicator
	{
		public StatementActionMethodApplicator(BusinessObjectFactory factory)
			: base("Send ACH Payment Authorization operational action", factory)
		{
			statementPaymentActions = new StatementPaymentActionCollection(factory);
			SkipValidationJustForTesting = false;
		}

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			if (statementPaymentActions.Count == 0)
			{
				statementPaymentActions.PopulateStatementHeaderElements(selectItemPKs);
			}
		}

		List<ActionsNotificationsHelper> notificationsHelpers;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			isCancelled = false;
			sendMessageWithOutError = false;
			actionsWithErrors.Clear();
			var manager = new AutomatedClearinghouseMessageManager();
			var validator = new StatementMessageSendingValidator();
			notificationsHelpers = new List<ActionsNotificationsHelper>();

			if (!SkipValidationJustForTesting && HasNotificationsOnObjectsOnAction())
			{
				var dialogResult = Globals.Message.Show(ThereIsANotification, "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel);
				if (dialogResult == DialogResult.Cancel)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "There is a notification, Cancel send messages.");
					isCancelled = true;
				}
				else if (dialogResult == DialogResult.OK)
				{
					sendMessageWithOutError = true;
				}
			}

			if (!isCancelled)
			{
				var sendItems = sendMessageWithOutError ? StatementPaymentActions.Cast<StatementPaymentAction>().Where(x => !actionsWithErrors.Contains(x.PK)) : StatementPaymentActions.Cast<StatementPaymentAction>();

				foreach (var action in sendItems)
				{
					var statementHeader = action.StatementHeader;
					if (statementHeader != null)
					{
						var notificationsHelper = GetNotifications(action, statementHeader, validator);
						if (SkipValidationJustForTesting || notificationsHelper.CanSendMessage)
						{
							manager.SendAuthorisation(action);
							try
							{
								statementHeader.Factory.Save();
								notificationsHelper.UpdateCanSendMessage(true, "Message(s) Sent");
								notificationsHelpers.Add(notificationsHelper);
							}
							catch (Exception ex) when (!ex.IsCriticalException())
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}
						else
						{
							notificationsHelpers.Add(notificationsHelper);
						}
					}
				}
			}
		}

		ActionsNotificationsHelper GetNotifications(StatementPaymentAction action, CusStatementHeader statementHeader, StatementMessageSendingValidator validator)
		{
			var notificationsHelper = new ActionsNotificationsHelper(GetStatementLink(statementHeader));
			notificationsHelper.CheckIfValidToSendAuthorizationOrPayment(statementHeader);

			var notificationsForCriticalCheck = validator.GetNotificationsForPayment(statementHeader);
			var canSendMessageForCriticalCheck = !notificationsForCriticalCheck.ContainsError() && !notificationsForCriticalCheck.ContainsWarning();
			var messageText = notificationsForCriticalCheck.NotificationsAsString();
			notificationsHelper.UpdateCanSendMessage(canSendMessageForCriticalCheck, messageText);

			var headerMessageErrorAsString = validator.CheckBusinessObjectLevelValidation(action).NotificationsAsString();
			notificationsHelper.UpdateCanSendMessage(string.IsNullOrEmpty(headerMessageErrorAsString), headerMessageErrorAsString);

			return notificationsHelper;
		}

		public const string ThereIsANotification = "There is a notification on operational action. Are you sure you wish to continue? \r\nIf you click 'OK', the system will send the ACH Payment Authorizations without errors.";

		protected override bool SupportsSummaryCore => true;

		protected override void SummaryLogCore(IOperationalActionSectionLog log)
		{
			if (!isCancelled)
			{
				base.SummaryLogCore(log);

				if (notificationsHelpers != null && notificationsHelpers.Count > 0)
				{
					foreach (var notificationsHelper in notificationsHelpers)
					{
						var errorLevel = notificationsHelper.CanSendMessage ? OperationalActionLogErrorLevel.Informational : OperationalActionLogErrorLevel.Warning;
						var messageText = notificationsHelper.CanSendMessage ? notificationsHelper.NotificationsAsString : ZString.Format("Cannot Send Message(s): \r\n{0}", notificationsHelper.NotificationsAsString);

						log.NotifyFormat(errorLevel, "{0} - " + messageText, new object[] { notificationsHelper.JobNumberLink });
					}

					log.Notify(OperationalActionLogErrorLevel.Informational, "Click the above Statement(s) to see the message.\r\n");
				}
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Informational, "Cancelled.\r\n");
			}
		}

		bool isCancelled;
		bool sendMessageWithOutError;

		public StatementPaymentActionCollection StatementPaymentActions
		{
			get => statementPaymentActions;
		}
		readonly StatementPaymentActionCollection statementPaymentActions;

		bool HasNotificationsOnObjectsOnAction()
		{
			foreach (var sendingObject in StatementPaymentActions.Cast<StatementPaymentAction>())
			{
				sendingObject.RunPreSaveValidation();
				if (sendingObject.HasMessageErrors())
				{
					actionsWithErrors.Add(sendingObject.PK);
				}
			}
			return actionsWithErrors.Count > 0;
		}

		readonly List<ZGuid> actionsWithErrors = new List<ZGuid>();

		LogControllerLink GetStatementLink(CusStatementHeader statementHeader)
		{
			return new LogControllerLink(statementHeader.B2_StatementNumber, ControllerIDs.Customs.CustomsStatement, statementHeader.PK);
		}

		internal bool SkipValidationJustForTesting;
	}
}
