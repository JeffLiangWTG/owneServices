using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business
{
	public class CustomsNotificationCollector : ZNotificationCollector
	{
		public CustomsNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage)
			: base(business, includeChildren, includeNotificationTypeInMessage)
		{
		}

		public CustomsNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude)
			: base(business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
		}

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
		{
			return businessObject.GetType() != typeof(Transport);
		}
	}
}
