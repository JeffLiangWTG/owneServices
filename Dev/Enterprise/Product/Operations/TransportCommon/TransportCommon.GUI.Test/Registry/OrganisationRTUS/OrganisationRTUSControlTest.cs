using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.TransportCommon.GUI.Registry;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Testing
{
	[TestedType(typeof(OrganisationRTUSControl))]
	class OrganisationRTUSControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OrganisationRTUSCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((OrganisationRTUSControl)control).GridReadonly;
		}
	}
}
