using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Security;
using static CargoWise.EntityFramework.ZNotificationCollector;

namespace Enterprise.Customs.Business
{
	public abstract class BaseMessageSendingObjectParent<T> : BaseMessageSendingObjectParent
		where T : BaseMessageSendingObject
	{
		protected BaseMessageSendingObjectParent(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new NonPersistentBusinessObjectCollection<T> SendingObjectsCollection
			=> (NonPersistentBusinessObjectCollection<T>)base.SendingObjectsCollection;

		protected sealed override IBusinessObjectCollection GetSendingObjectsCollection() => GetSendingObjectsCollectionCore();

		protected abstract NonPersistentBusinessObjectCollection<T> GetSendingObjectsCollectionCore();
	}

	public abstract class BaseMessageSendingObjectParent : NonPersistentBusinessObject
	{
		protected BaseMessageSendingObjectParent(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public abstract BusinessObject TopLevelBusinessObject { get; }

		public event EventHandler SelectedSendingObjectsChanged;

		public IBusinessObjectCollection SendingObjectsCollection
		{
			get
			{
				if (sendingObjectsCollection == null)
				{
					sendingObjectsCollection = GetSendingObjectsCollection();
					RegisterEditableChildObject(sendingObjectsCollection);

					foreach (BaseMessageSendingObject bo in sendingObjectsCollection)
					{
						HookMessageSendingObjectEvents(bo);
					}
				}
				return sendingObjectsCollection;
			}
		}

		public abstract SecurityCheckpoint SecurityCheckpointToSendWithMessageError { get; }

		public virtual IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => Enumerable.Empty<MessageSendingObjectProperty>();

		public IEnumerable<BaseMessageSendingObject> SelectedSendingObjects => SendingObjectsCollection.Cast<BaseMessageSendingObject>().Where(x => x.ShouldSend);

		[ResourceStringData("Enterprise.Customs.Business.BaseMessageSendingObjectParent|BizObjValidationMessageErrors", Caption = "Validation Errors")]
		public ZString BizObjValidationMessageErrors => (bizObjValidationMessageErrors ?? (bizObjValidationMessageErrors = GetBizObjValidationMessageErrors())).Value;

		public ZPropertyInfo BizObjValidationMessageErrorsInfo => GetZPropertyInfo(nameof(BizObjValidationMessageErrors));

		[ResourceStringData("Enterprise.Customs.Business.BaseMessageSendingObjectParent|AdditionalWarnings", Caption = "Additional Warnings")]
		public ZString AdditionalWarnings => (additionalWarnings ?? (additionalWarnings = GetAdditionalWarnings())).Value;

		public ZPropertyInfo AdditionalWarningsInfo => GetZPropertyInfo(nameof(AdditionalWarnings));

		MessageSendingValidation messageSendingValidation;
		public MessageSendingValidation MessageSendingValidation => messageSendingValidation ?? (messageSendingValidation = GetNewMessageSendingValidation());

		public virtual bool AllowEmptyDeclaration => false;
		public virtual bool HasAnyObjectToSend => SendingObjectsCollection.Cast<BaseMessageSendingObject>().Any(x => x.ShouldSend);

		protected virtual void HookMessageSendingObjectEvents(BaseMessageSendingObject bo)
		{
			bo.ShouldSendInfo.ValueChanged += ShouldSendInfo_ValueChanged;
		}

		protected abstract IBusinessObjectCollection GetSendingObjectsCollection();

		protected void ResetValidationMessages()
		{
			messageSendingValidation = null;
			bizObjValidationMessageErrors = null;
			additionalWarnings = null;

			BizObjValidationMessageErrorsInfo.RefreshBinding();
			AdditionalWarningsInfo.RefreshBinding();
		}

		protected virtual ZString GetBizObjValidationMessageErrors()
		{
			var result = string.Empty;
			if (SelectedSendingObjects.Any())
			{
				result = Regex.Replace(MessageSendingValidation.CheckBusinessObjectLevelValidation().NotificationsAsString(), "(?<!\r)\n", "\r\n");
			}
			return result;
		}

		protected virtual MessageSendingValidation GetNewMessageSendingValidation()
			=> MessageSendingValidation.New(TopLevelBusinessObject, GetNewMessageErrorCollector(), SecurityCheckpointToSendWithMessageError ?? Env.Security.CustomsDeclarationSendWithMessageErrors);

		protected CustomsNotificationCollector NotificationCollector => new CustomsNotificationCollector(TopLevelBusinessObject, true, false, PropertyDescriptionType.HumanReadableName);

		protected virtual IEnumerable<INotification> GetNewMessageErrorCollector() => NotificationCollector.GetMessageErrors();

		protected ZString GetAdditionalWarnings() => GetAdditionalWarningsCore();

		protected virtual ZString GetAdditionalWarningsCore() => ZString.Empty;

		void ShouldSendInfo_ValueChanged(object sender, EventArgs e)
		{
			ResetValidationMessages();

			SelectedSendingObjectsChanged?.Invoke(sender, e);
		}

		IBusinessObjectCollection sendingObjectsCollection;
		ZString? bizObjValidationMessageErrors;
		ZString? additionalWarnings;
	}
}
