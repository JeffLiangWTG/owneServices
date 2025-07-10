using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AutoRateDateByChargeGroup))]
	sealed class AutoRateDateByChargeGroupTest : ChargeGroupSetupTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new AutoRateDateByChargeGroup();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AutoRateDateByChargeGroup();
		}

		#endregion
	}
}
