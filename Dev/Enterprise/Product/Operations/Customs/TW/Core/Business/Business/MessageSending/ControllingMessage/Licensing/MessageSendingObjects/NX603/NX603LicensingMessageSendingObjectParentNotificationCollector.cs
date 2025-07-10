using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX603LicensingMessageSendingObjectParentNotificationCollector : LicensingMessageSendingObjectParentNotificationCollector
	{
		public NX603LicensingMessageSendingObjectParentNotificationCollector(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
		}

		protected override IEnumerable<string> GetExcludedAdditionalNotificationsFromInvoicePropertiesCore()
		{
			yield return AutoJobComInvoiceHeader.Schema.JZ_NetWeight;
		}

		protected override IEnumerable<string> GetExcludedAdditionalNotificationsFromDeclarationPropertiesCore()
		{
			yield return AutoJobDeclaration.Schema.JE_TotalNoOfPacks;
			yield return AutoJobDeclaration.Schema.JE_TotalWeight;
		}
	}
}
