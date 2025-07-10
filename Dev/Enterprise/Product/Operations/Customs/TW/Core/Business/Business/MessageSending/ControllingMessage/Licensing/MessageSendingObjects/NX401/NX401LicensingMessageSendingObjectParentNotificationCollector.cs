using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX401LicensingMessageSendingObjectParentNotificationCollector : LicensingMessageSendingObjectParentNotificationCollector
	{
		public NX401LicensingMessageSendingObjectParentNotificationCollector(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
		}

		protected override IEnumerable<string> GetExcludedAdditionalNotificationsFromDeclarationPropertiesCore()
		{
			yield return AutoJobDeclaration.Schema.JE_TotalNoOfPacks;
			yield return AutoJobDeclaration.Schema.JE_TotalWeight;
		}
	}
}
