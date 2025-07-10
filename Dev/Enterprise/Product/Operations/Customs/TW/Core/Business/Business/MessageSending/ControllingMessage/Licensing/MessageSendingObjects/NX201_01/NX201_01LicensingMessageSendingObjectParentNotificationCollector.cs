using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business;

public class NX201_01LicensingMessageSendingObjectParentNotificationCollector : LicensingMessageSendingObjectParentNotificationCollector
{
	public NX201_01LicensingMessageSendingObjectParentNotificationCollector(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
	{
	}

	protected override IEnumerable<string> GetExcludedAdditionalNotificationsFromDeclarationPropertiesCore()
	{
		yield return JobDeclaration.Schema.DeclarationNumberDisplay;
	}
}
