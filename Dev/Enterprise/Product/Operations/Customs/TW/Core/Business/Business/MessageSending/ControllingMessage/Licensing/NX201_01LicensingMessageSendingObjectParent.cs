using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01LicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX201_01LicensingMessageSendingObjectParent(JobDeclaration declaration, string menuCaption = "")
			: base(declaration, ControllingMessageTypeList.Codes.NX201_01, menuCaption)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return new NX201_01MessageSendingObject(messageHeader);
		}

		protected override MessageSendingValidation GetNewMessageSendingValidation()
		{
			MessageSendingValidation validation;
			if (Declaration.IsImport)
			{
				validation = new NX201_01ImportMessageSendingObjectParentValidation(this, Declaration, GetNewMessageErrorCollector(), SecurityCheckpointToSendWithMessageError);
			}
			else if (Declaration.IsExport)
			{
				validation = new NX201_01ExportMessageSendingObjectParentValidation(this, Declaration, GetNewMessageErrorCollector(), SecurityCheckpointToSendWithMessageError);
			}
			else
			{
				validation = base.GetNewMessageSendingValidation();
			}

			return validation;
		}

		protected override bool GetShowReasonDescriptionCore() => true;

		protected override LicensingMessageSendingObjectParentNotificationCollector GetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			return new NX201_01LicensingMessageSendingObjectParentNotificationCollector(SendingObjectsCollection, TopLevelBusinessObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}
	}
}
