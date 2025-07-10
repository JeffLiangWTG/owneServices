using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SupplyTypeConfigurationByChargeGroupCollection))]
	sealed class SupplyTypeConfigurationByChargeGroupCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SupplyTypeConfigurationByChargeGroupCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override SupplyTypeConfigurationByChargeGroupCollection GetCollectionToTest()
		{
			return new SupplyTypeConfigurationByChargeGroupCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SupplyTypeConfigurationByChargeGroup();
		}

		#endregion
	}
}
