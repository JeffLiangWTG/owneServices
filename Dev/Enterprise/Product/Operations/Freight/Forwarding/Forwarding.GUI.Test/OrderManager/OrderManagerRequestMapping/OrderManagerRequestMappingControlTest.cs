using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(OrderManagerRequestMappingControl))]
	sealed class OrderManagerRequestMappingControlTest : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OrderManagerRequestMappingCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var typesControl = (OrderManagerRequestMappingControl)control;
			return typesControl.orderManagerRequestMappingGrid.ReadOnly;
		}
	}
}
