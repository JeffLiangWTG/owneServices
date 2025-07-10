using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Registry
{
	[TestedType(typeof(ServiceTaskCreatorOptionControl))]
	class ServiceTaskCreatorOptionControl_Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ServiceTaskCreatorOptionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ServiceTaskCreatorOptionControl)control).ServiceTaskCreatorOptionGrid.ReadOnly;
		}
	}
}
