using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(SameChargeCodeDifferentProviderControl))]
	class SameChargeCodeDifferentProviderControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SameChargeCodeDifferentProviderCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((SameChargeCodeDifferentProviderControl)control).IsControlOrBusinessEntityReadOnly;
		}
	}
}
