using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SupplyTypeConfigurationByChargeGroup))]
	sealed class SupplyTypeConfigurationByChargeGroupTest : ChargeGroupSetupTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new SupplyTypeConfigurationByChargeGroup();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SupplyTypeConfigurationByChargeGroup();
		}

		#endregion
	}
}
