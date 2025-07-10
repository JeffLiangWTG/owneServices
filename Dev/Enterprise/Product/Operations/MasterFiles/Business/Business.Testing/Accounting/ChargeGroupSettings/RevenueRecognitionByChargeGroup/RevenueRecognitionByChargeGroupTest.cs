using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RevenueRecognitionByChargeGroup))]
	sealed class RevenueRecognitionByChargeGroupTest : ChargeGroupSetupTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new RevenueRecognitionByChargeGroup();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RevenueRecognitionByChargeGroup();
		}

		#endregion
	}
}
