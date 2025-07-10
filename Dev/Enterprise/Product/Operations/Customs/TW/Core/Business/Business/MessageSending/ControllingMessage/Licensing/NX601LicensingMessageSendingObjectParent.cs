using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX601LicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX601LicensingMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration, ControllingMessageTypeList.Codes.NX601)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return new NX601MessageSendingObject(messageHeader);
		}

		protected override LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new NX601MessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		protected override LicensingMessageSendingObjectAdditionalMessageErrorCollector GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore()
		{
			return new NX601MessageSendingObjectAdditionalMessageErrorCollector(this);
		}
	}
}
