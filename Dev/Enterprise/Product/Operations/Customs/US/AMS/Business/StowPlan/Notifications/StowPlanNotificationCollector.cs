using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanNotificationCollector : ZNotificationCollector
	{
		public StowPlanNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage)
			: base(business, includeChildren, includeNotificationTypeInMessage)
		{ }

		public StowPlanNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude)
			: base(business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{ }

		protected override INotification GetNotification(BusinessObject bizObj, INotification notification, string replacedMessage)
		{
			var result = base.GetNotification(bizObj, notification, replacedMessage);
			if (!(result is StowPlanNotification))
			{
				result = new StowPlanNotification((IStowPlanNotificationProvider)bizObj, result.Type, result.Message, result.Message);
			}
			return result;
		}
	}
}
