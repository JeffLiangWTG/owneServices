using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public abstract class LicensingMessageSendingObjectParent : ControllingMessageSendingObjectParent
	{
		protected LicensingMessageSendingObjectParent(JobDeclaration declaration, string messageType, string menuCaption = "")
			: base(declaration, messageType, menuCaption)
		{
		}

		public static LicensingMessageSendingObjectParent GetLicensingMessageSendingObjectParent(JobDeclaration declaration, string messageType, string menuCaption = "")
		{
			switch (messageType)
			{
				case ControllingMessageTypeList.Codes.NX101:
					return new NX101LicensingMessageSendingObjectParent(declaration, menuCaption);
				case ControllingMessageTypeList.Codes.NX201_01:
					return new NX201_01LicensingMessageSendingObjectParent(declaration, menuCaption);
				case ControllingMessageTypeList.Codes.NX201_07:
					return new NX201_07LicensingMessageSendingObjectParent(declaration);
				case ControllingMessageTypeList.Codes.NX301:
					return new NX301LicensingMessageSendingObjectParent(declaration);
				case ControllingMessageTypeList.Codes.NX301_AX:
					return new NX301_AXLicensingMessageSendingObjectParent(declaration);
				case ControllingMessageTypeList.Codes.NX301_DN:
					return new NX301_DNLicensingMessageSendingObjectParent(declaration);
				case ControllingMessageTypeList.Codes.NX401:
					return new NX401LicensingMessageSendingObjectParent(declaration);
				case ControllingMessageTypeList.Codes.NX601:
					return new NX601LicensingMessageSendingObjectParent(declaration);
				case ControllingMessageTypeList.Codes.NX603:
					return new NX603LicensingMessageSendingObjectParent(declaration);
				default:
					return null;
			}
		}

		protected override NonPersistentBusinessObjectCollection<ControllingMessageSendingObject> GetNewSendingObjectCollection(BusinessObjectFactory factory) => new LicensingMessageSendingObjectCollection(factory);

		public new LicensingMessageSendingObjectCollection SendingObjectsCollection => base.SendingObjectsCollection as LicensingMessageSendingObjectCollection;

		protected override IEnumerable<INotification> GetNewMessageErrorCollector() => GetLicensingMessageSendingObjectParentNotificationCollectorCore().GetMessageErrors()
			.Concat(GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore());

		public bool ShowReasonDescription => GetShowReasonDescriptionCore();

		protected virtual bool GetShowReasonDescriptionCore() => false;

		protected virtual LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new LicensingMessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		protected virtual LicensingMessageSendingObjectAdditionalMessageErrorCollector GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore()
		{
			return new LicensingMessageSendingObjectAdditionalMessageErrorCollector(this);
		}

		public virtual bool IsSupportingDocumentsNeededMessage => true;

		protected override ZString GetAdditionalWarningsCore() => GetNotificationsMessage(WarningsMessage.ToUniqueMessageListString);

		IEnumerable<INotification> WarningsMessage => GetLicensingMessageSendingObjectParentNotificationCollectorCore().GetWarnings();

		ZString GetNotificationsMessage(Func<string> getNotificationsMessage)
		{
			var result = string.Empty;
			if (SelectedSendingObjects.Any())
			{
				result = Regex.Replace(getNotificationsMessage(), "(?<!\r)\n", "\r\n");
			}
			return result;
		}
	}
}
