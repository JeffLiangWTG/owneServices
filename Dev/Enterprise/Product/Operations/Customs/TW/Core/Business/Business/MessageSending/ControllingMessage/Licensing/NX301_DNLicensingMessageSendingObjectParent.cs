using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_DNLicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX301_DNLicensingMessageSendingObjectParent(JobDeclaration declaration) : base(declaration, ControllingMessageTypeList.Codes.NX301_DN)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return new NX301_DNMessageSendingObject(messageHeader);
		}

		protected override LicensingMessageSendingObjectAdditionalMessageErrorCollector GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore()
		{
			return new NX301_DNMessageSendingObjectAdditionalMessageErrorCollector(this);
		}

		protected override LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new NX301_DNLicensingMessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}
	}
}
