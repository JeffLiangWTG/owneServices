using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AutoRateDateByChargeGroupCollection))]
	sealed class AutoRateDateByChargeGroupCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AutoRateDateByChargeGroupCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override AutoRateDateByChargeGroupCollection GetCollectionToTest()
		{
			return new AutoRateDateByChargeGroupCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AutoRateDateByChargeGroup();
		}

		#endregion
	}
}
