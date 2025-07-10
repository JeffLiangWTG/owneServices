using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class SelectiveMessageErrorCollector : CustomsNotificationCollector
	{
		public SelectiveMessageErrorCollector(BusinessObject topBusinessObject, IEnumerable<BusinessObject> selectedChildren) : base(topBusinessObject, true, false, PropertyDescriptionType.HumanReadableName)
		{
			this.topBusinessObject = topBusinessObject;
			this.selectedChildren = selectedChildren;
		}

		readonly BusinessObject topBusinessObject;
		readonly IEnumerable<BusinessObject> selectedChildren;

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
		{
			var result = base.ShouldIncludeNotificationsFromObject(businessObject);

			if (result)
			{
				result = businessObject == topBusinessObject
					|| selectedChildren.Contains(businessObject)
					|| selectedChildren.Any(x => IsDescedent(businessObject, x));
			}
			return result;
		}

		bool IsDescedent(BusinessObject child, IBusiness parent)
		{
			bool result = parent.Children.Contains(child);

			if (!result)
			{
				var immediateChildren = parent.Children;
				foreach (var immediateChild in immediateChildren)
				{
					result = IsDescedent(child, immediateChild);
					if (result)
					{
						break;
					}
				}
			}

			return result;
		}
	}
}
