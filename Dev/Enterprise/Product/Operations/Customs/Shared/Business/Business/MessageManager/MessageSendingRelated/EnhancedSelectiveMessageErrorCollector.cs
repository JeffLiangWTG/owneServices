using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class EnhancedSelectiveMessageErrorCollector : CustomsNotificationCollector
	{
		public EnhancedSelectiveMessageErrorCollector(BusinessObject topBusinessObject, IEnumerable<BusinessObject> additionalBusinessObjects, IEnumerable<BusinessObject> selectedChildren)
			: base(topBusinessObject, true, false, PropertyDescriptionType.HumanReadableName)
		{
			this.topBusinessObject = topBusinessObject;
			this.additionalBusinessObjects = additionalBusinessObjects;
			this.selectedChildren = selectedChildren;
		}
		readonly BusinessObject topBusinessObject;
		readonly IEnumerable<BusinessObject> selectedChildren;
		readonly IEnumerable<BusinessObject> additionalBusinessObjects;

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject) =>
			base.ShouldIncludeNotificationsFromObject(businessObject)
			&& (businessObject.Equals(topBusinessObject) || additionalBusinessObjects.Contains(businessObject) || selectedChildren.Contains(businessObject));
	}
}
