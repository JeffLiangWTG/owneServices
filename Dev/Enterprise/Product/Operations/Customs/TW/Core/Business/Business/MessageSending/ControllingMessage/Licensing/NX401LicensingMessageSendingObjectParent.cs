using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX401LicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX401LicensingMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration, ControllingMessageTypeList.Codes.NX401)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return Declaration.IsExport ? new NX401ExportMessageSendingObject(messageHeader) : new NX401ImportMessageSendingObject(messageHeader);
		}

		protected override LicensingMessageSendingObjectAdditionalMessageErrorCollector GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore()
		{
			return new NX401MessageSendingObjectAdditionalMessageErrorCollector(this);
		}

		protected override LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new NX401LicensingMessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}
	}
}
