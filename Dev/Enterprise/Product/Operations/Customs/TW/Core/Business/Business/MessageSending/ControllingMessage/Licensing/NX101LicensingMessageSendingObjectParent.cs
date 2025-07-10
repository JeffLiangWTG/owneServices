using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX101LicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX101LicensingMessageSendingObjectParent(JobDeclaration declaration, string menuCaption = "")
			: base(declaration, ControllingMessageTypeList.Codes.NX101, menuCaption)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return new NX101MessageSendingObject(messageHeader);
		}

		protected override LicensingMessageSendingObjectAdditionalMessageErrorCollector GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore()
		{
			return new NX101MessageSendingObjectAdditionalMessageErrorCollector(this);
		}

		public override bool IsSupportingDocumentsNeededMessage => false;

		protected override LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new NX101LicensingMessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}
	}
}
