using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Workflow.ValidationAction
{
	public class CustomsMessageValidationProcessor : IProcessor
	{
		public CustomsMessageValidationProcessor(ProcessTaskNotification triggerAction, BusinessObject parent)
		{
			this.triggerAction = Argument.NotNull(triggerAction, "triggerAction");
			this.parent = parent;
			this.Factory = parent.Factory;
			var trigger = triggerAction.Parent;
			if (trigger != null)
			{
				var entity = parent as IValidateForCustomsMessagingSupporter;
				this.entityToValidate = (entity == null) ? null : entity.GetEntityToValidate(triggerAction.PQ_TriggerType);
			}
		}

		readonly ProcessTaskNotification triggerAction;
		readonly BusinessObject parent;
		readonly BusinessObjectFactory Factory;
		readonly BusinessObject entityToValidate;

		public void Process(INotifications notifications, CancellationToken unused
#if DEBUG
				= new CancellationToken()
#endif
		)
		{
			if (entityToValidate == null)
			{
				notifications.AddWarning(Res.GetString("20d65006-6f62-477b-aae9-7805c3f9b217", "Could not find a Parent BO for Trigger Action with PK:{0}", triggerAction.PK.ToString()));
				return;
			}

			if (triggerAction.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn)
			{
				ValidateOutturns();
			}
			else
			{
				ValidateCustomsMessaging();
			}
		}

		void ValidateCustomsMessaging()
		{
			var logs = entityToValidate.GetLogs();
			var creditCheckMessage = ZString.Empty;
			if (entityToValidate is IBaseJobDeclaration declaration)
			{
				creditCheckMessage = declaration.GetCreditCheckMessage();
				if (!creditCheckMessage.IsEmpty)
				{
					logs.AddNew(Events.CreditCheckFailed, creditCheckMessage.Replace("\r\n", ZString.Empty));
				}
			}

			var additionalValidationMessages = Array.Empty<string>();
			if (entityToValidate is EU.NCTS.INctsHeaderWithAdditionalMessagingValidation
				nctsHeaderWithAddtionalValidation)
			{
				additionalValidationMessages = nctsHeaderWithAddtionalValidation.GetAdditionalValidationErrorMessages();
			}

			var errors = GetAllNotificationsExceptWarnings();

			if (errors.Length == 0 && creditCheckMessage.IsEmpty && additionalValidationMessages.Length == 0)
			{
				logs.AddNew(Events.MessageValidationPassed);
			}
			else
			{
				var recipentAddress = triggerAction.PQ_EmailAddr;
				if (!creditCheckMessage.IsEmpty)
				{
					errors = errors.Append(creditCheckMessage.ToString()).ToArray();
				}

				if (additionalValidationMessages.Length > 0)
				{
					errors = errors.Append(additionalValidationMessages).ToArray();
				}

				SendNotificationEmail(recipentAddress, errors);
				logs.AddNew(Events.MessageValidationFailed, ZString.Format((NoResString)"Please check the email sent to {0} for more details.", recipentAddress));
			}
		}

		void ValidateOutturns()
		{
			if (entityToValidate is Enterprise.Integration.Customs.AU.ICusMAWB mawb)
			{
				var logs = ((IStmALogParent)mawb).Logs;
				var errorMessage = mawb.GetOutturnsReconciliationMessage();
				logs.AddNew(errorMessage.IsEmpty ? Events.ReconcileOutturnPassed : Events.ReconcileOutturnFailed);
			}
		}

		string[] GetAllNotificationsExceptWarnings()
		{
			entityToValidate.MarkAsNeedingValidationIncludingChildren();
			entityToValidate.RunPreSaveValidation();
			return
				new CustomsNotificationCollector(entityToValidate, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName)
					.Where(notification => notification.Type != CargoWise.EntityFramework.NotificationType.Warning)
					.Take(100)
					.Select(notification => notification.Message)
					.OrderBy(message => message)
					.ToArray();
		}

		void SendNotificationEmail(string emailAddress, string[] errors)
		{
			if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(emailAddress))
			{
				var notifier = new ValidationNotifier(emailAddress, entityToValidate, Factory);
				notifier.SendValidationFailureNotification(errors, ObjectFactory.Get<IShowEditFormUrlCreator>().Create(parent as IControllerIDProvider));
			}
		}
	}
}
