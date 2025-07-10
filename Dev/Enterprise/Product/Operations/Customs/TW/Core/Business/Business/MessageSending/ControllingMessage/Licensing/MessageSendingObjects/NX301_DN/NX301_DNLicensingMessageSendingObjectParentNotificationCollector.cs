using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_DNLicensingMessageSendingObjectParentNotificationCollector : LicensingMessageSendingObjectParentNotificationCollector
	{
		public NX301_DNLicensingMessageSendingObjectParentNotificationCollector(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude)
			: base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
		}

		protected override IEnumerable<string> GetExcludedAdditionalNotificationsFromDeclarationPropertiesCore()
		{
			yield return AutoJobDeclaration.Schema.JE_MasterBill;
			yield return AutoJobDeclaration.Schema.JE_VesselName;
			yield return AutoJobDeclaration.Schema.JE_VoyageFlightNo;
			yield return AutoJobDeclaration.Schema.JE_TotalWeight;
			yield return AutoJobDeclaration.Schema.JE_TotalNoOfPacks;
		}
	}
}
