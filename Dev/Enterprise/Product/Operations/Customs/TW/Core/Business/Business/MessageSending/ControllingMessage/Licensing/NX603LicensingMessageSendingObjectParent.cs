using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX603LicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX603LicensingMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration, ControllingMessageTypeList.Codes.NX603)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return new NX603MessageSendingObject(messageHeader);
		}

		protected override LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new NX603LicensingMessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}
	}
}
