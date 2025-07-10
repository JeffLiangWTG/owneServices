using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SupplyTypeConfigurationCollection))]
	sealed class SupplyTypeConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SupplyTypeConfigurationCollection>
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

		protected override SupplyTypeConfigurationCollection GetCollectionToTest()
		{
			return new SupplyTypeConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SupplyTypeConfiguration();
		}

		#endregion
	}
}
