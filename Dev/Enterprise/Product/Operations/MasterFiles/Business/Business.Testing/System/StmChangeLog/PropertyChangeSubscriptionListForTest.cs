using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class PropertyChangeSubscriptionListForTest : PropertyChangeSubscriptionList
	{
		public new static PropertyChangeSubscriptionListForTest GetInstance(BusinessObjectFactory factory)
		{
			PropertyChangeSubscriptionListForTest result = PropertyChangeSubscriptionList.GetInstance(factory) as PropertyChangeSubscriptionListForTest;
			if (result == null)
			{
				result = new PropertyChangeSubscriptionListForTest();
				PropertyChangeSubscriptionList.SetInstance(factory, result);
			}
			return result;
		}

		public new static ICollection<string> WorkflowTriggerPropertyNamesThatMayBeLogged
		{
			get { return PropertyChangeSubscriptionList.WorkflowTriggerPropertyNamesThatMayBeLogged; }
		}

		public readonly List<string> SubscribedProperties = new List<string>();

		public override bool ShouldLogChanges(ZPropertyInfo property)
		{
			return base.ShouldLogChanges(property) || SubscribedProperties.Contains(property.Name);
		}
	}
}
