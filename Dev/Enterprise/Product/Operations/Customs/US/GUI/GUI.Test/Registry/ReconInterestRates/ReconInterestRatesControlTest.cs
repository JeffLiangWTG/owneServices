using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(ReconInterestRatesControl))]
	sealed class ReconInterestRatesControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ReconInterestRateCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			ReconInterestRatesControl reconInterestRatesControl = (ReconInterestRatesControl)control;
			return reconInterestRatesControl.RatesGrid.ReadOnly;
		}
	}
}
