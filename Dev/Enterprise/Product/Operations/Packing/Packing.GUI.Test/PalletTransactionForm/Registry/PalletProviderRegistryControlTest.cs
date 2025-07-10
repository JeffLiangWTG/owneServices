using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	[TestedType(typeof(PalletProviderRegistryControl))]
	sealed class PalletProviderRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new PalletTypeParent();
		}
	}
}
