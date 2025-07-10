using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_07LicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX201_07LicensingMessageSendingObjectParent(JobDeclaration declaration) : base(declaration, ControllingMessageTypeList.Codes.NX201_07)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return new NX201_07MessageSendingObject(messageHeader);
		}

		protected override LicensingMessageSendingObjectAdditionalMessageErrorCollector GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore()
		{
			return new NX201_07MessageSendingObjectAdditionalMessageErrorCollector(this);
		}

		protected override bool GetShowReasonDescriptionCore() => true;

		protected override LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new NX201_07LicensingMessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}
	}
}
