using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(SellRatesDecimalsControl))]
	class SellRatesDecimalsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SellRatesDecimalsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((SellRatesDecimalsControl)control).IsControlOrBusinessEntityReadOnly;
		}
	}
}
