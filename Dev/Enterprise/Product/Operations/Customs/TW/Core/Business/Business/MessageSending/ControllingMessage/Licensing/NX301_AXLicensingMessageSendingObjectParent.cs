using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_AXLicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX301_AXLicensingMessageSendingObjectParent(JobDeclaration declaration) : base(declaration, ControllingMessageTypeList.Codes.NX301_AX)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return new NX301_AXMessageSendingObject(messageHeader);
		}

		protected override LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new NX301_AXMessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}
	}
}
