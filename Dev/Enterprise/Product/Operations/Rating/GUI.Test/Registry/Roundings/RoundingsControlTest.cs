using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(RoundingsControl))]
	class RoundingsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DefaultRoundingsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((RoundingsControl)control).IsControlOrBusinessEntityReadOnly;
		}
	}
}
