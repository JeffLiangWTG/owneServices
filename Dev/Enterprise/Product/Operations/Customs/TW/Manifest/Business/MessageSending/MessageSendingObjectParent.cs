using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class MessageSendingObjectParent : Customs.Business.BaseMessageSendingObjectParent<MessageSendingObject>
	{
		public MessageSendingObjectParent(AsycudaManifestHeader manifestHeader) : base(manifestHeader.Factory)
		{
			this.manifestHeader = manifestHeader;
		}
		readonly AsycudaManifestHeader manifestHeader;

		public MessageSendingObjectParent(AsycudaManifestHeader manifestHeader, ZString menuCaption) : this(manifestHeader)
		{
			MenuCaption = menuCaption;
		}

		public override BusinessObject TopLevelBusinessObject => manifestHeader;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.TWManifestSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<MessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var sendingObjectsCollection = new MessageSendingObjectCollection(Factory);
			foreach (var bill in manifestHeader.Bills.Cast<AsycudaBill>())
			{
				var sendingObject = new MessageSendingObject(bill);
				sendingObjectsCollection.Add(sendingObject);
			}
			RegisterEditableChildObject(sendingObjectsCollection);
			return sendingObjectsCollection;
		}

		public ZBool AllowSendWithError
		{
			get => fAllowSendWithError;
			set => SetNonPersistentPropertyValue(AllowSendWithErrorInfo, ref fAllowSendWithError, value);
		}
		ZBool fAllowSendWithError;

		public ZPropertyInfo AllowSendWithErrorInfo => GetZPropertyInfo(nameof(AllowSendWithError));

		#region IsFinalManifest
		public ZBool IsFinalManifest
		{
			get => isFinalManifest;
			set => SetNonPersistentPropertyValue(IsFinalManifestInfo, ref isFinalManifest, value);
		}
		ZBool isFinalManifest;

		public ZPropertyInfo IsFinalManifestInfo => GetZPropertyInfo(nameof(IsFinalManifest));
		#endregion

		public ZString MenuCaption { get; private set; }

		protected override ZString GetBizObjValidationMessageErrors() => GetNotificationsMessage(() => MessageErrorsCollection.ToUniqueMessageListString());

		protected override ZString GetAdditionalWarningsCore() => GetNotificationsMessage(() => WarningsCollection.ToUniqueMessageListString());

		void CheckCertificateExpired(List<INotification> list)
		{
			if (manifestHeader.ForwarderCredential is GlbCompanyCredential credential)
			{
				var expiryDate = credential.GP_ExpiryDate;
				if (expiryDate.IsValid)
				{
					if (expiryDate.Date < ZDate.Today)
					{
						list.Add(new Notification(CargoWise.EntityFramework.NotificationType.MessageError, TW.Business.ValidationConstants.Declaration.CertificateExpired));
					}
				}
			}
		}

		void CheckCertificateWillExpire(List<INotification> list)
		{
			if (manifestHeader.ForwarderCredential is GlbCompanyCredential credential)
			{
				var expiryDate = credential.GP_ExpiryDate;
				if (expiryDate.IsValid)
				{
					var date = expiryDate.Date;
					var today = ZDate.Today;
					if (date >= today && date < today.AddDays(31))
					{
						list.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, TW.Business.ValidationConstants.Declaration.CertificateWillExpire(expiryDate.Date.ToISO8601ShortDateString())));
					}
				}
			}
		}

		IEnumerable<INotification> MessageErrorsCollection
		{
			get
			{
				var result = new List<INotification>();
				result.AddRange(new ZNotificationCollector(TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors());
				result.AddRange(new ZNotificationCollector(this, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors());
				CheckCertificateExpired(result);
				return result;
			}
		}

		IEnumerable<INotification> WarningsCollection
		{
			get
			{
				var result = new List<INotification>();
				result.AddRange(new ZNotificationCollector(TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetWarnings());
				result.AddRange(new ZNotificationCollector(this, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetWarnings());
				CheckCertificateWillExpire(result);
				return result;
			}
		}

		ZString GetNotificationsMessage(Func<string> getNotificationsString)
		{
			var result = string.Empty;
			if (SelectedSendingObjects.Any())
			{
				TopLevelBusinessObject.MarkAsNeedingValidationIncludingChildren();
				TopLevelBusinessObject.RunPreSaveValidation();
				SendingObjectsCollection.Cast<MessageSendingObject>().ForEach(c => c.RunPreSaveValidation());
				result = Regex.Replace(getNotificationsString(), "(?<!\r)\n", "\r\n");
			}
			return result;
		}
	}
}
