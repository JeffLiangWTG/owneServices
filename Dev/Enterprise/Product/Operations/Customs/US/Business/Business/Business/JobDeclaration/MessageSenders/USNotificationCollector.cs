using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCustomsNotificationCollector : Customs.Business.CustomsNotificationCollector
	{
		public USCustomsNotificationCollector(IMessageNotificationsProvider business, ZBool includeChildren, ZBool includeNotificationTypeInMessage)
			: base(business, includeChildren, includeNotificationTypeInMessage)
		{
			topLevelBusinessObject = business;
		}

		public USCustomsNotificationCollector(IMessageNotificationsProvider business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude)
			: base(business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
			topLevelBusinessObject = business;
		}
		readonly IMessageNotificationsProvider topLevelBusinessObject;

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
		{
			return base.ShouldIncludeNotificationsFromObject(businessObject) &&
				!topLevelBusinessObject.ExcludedChildrenAndTheirDescendentsFromMessageNotifications.Any(x => x == businessObject);
		}
	}
}
