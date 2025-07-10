using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RevenueRecognitionByChargeGroupCollection))]
	sealed class RevenueRecognitionByChargeGroupCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RevenueRecognitionByChargeGroupCollection>
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

		protected override RevenueRecognitionByChargeGroupCollection GetCollectionToTest()
		{
			return new RevenueRecognitionByChargeGroupCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RevenueRecognitionByChargeGroup();
		}

		#endregion
	}
}
