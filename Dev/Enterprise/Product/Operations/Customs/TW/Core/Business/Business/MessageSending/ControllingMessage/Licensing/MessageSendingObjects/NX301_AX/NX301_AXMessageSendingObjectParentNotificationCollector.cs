using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX301_AXMessageSendingObjectParentNotificationCollector : LicensingMessageSendingObjectParentNotificationCollector
	{
		public NX301_AXMessageSendingObjectParentNotificationCollector(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude)
			: base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
		}

		protected override IEnumerable<string> GetExcludedAdditionalNotificationsFromCusEntryHeaderPropertiesCore()
		{
			yield return CusEntryHeader.Schema.CH_TotalNetWeightInKilograms;
		}

		protected override IEnumerable<string> GetExcludedAdditionalNotificationsFromDeclarationPropertiesCore()
		{
			yield return AutoJobDeclaration.Schema.JE_TotalNoOfPacks;
			yield return AutoJobDeclaration.Schema.JE_TotalWeight;
		}
	}
}
