using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(DelayAlertDeliveryControl))]
	sealed class DelayAlertDeliveryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DelayAlertDeliveryRuleCollection();
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new DelayAlertDeliveryControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DelayAlertDeliveryRuleCollection)businessEntity).ReadOnly;
		}

		#endregion
	}
}
