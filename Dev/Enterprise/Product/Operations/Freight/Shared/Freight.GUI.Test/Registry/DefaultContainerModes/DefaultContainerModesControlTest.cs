using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.GUI
{
	[TestedType(typeof(DefaultContainerModesControl))]
	sealed class DefaultContainerModesControlTest : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DefaultContainerModesCollection(Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DefaultContainerModesControl)control).DefaultContainerModesGrid.ReadOnly;
		}
	}
}
