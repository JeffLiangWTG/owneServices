using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RevenueRecognitionCollection))]
	sealed class RevenueRecognitionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RevenueRecognitionCollection>
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

		protected override RevenueRecognitionCollection GetCollectionToTest()
		{
			return new RevenueRecognitionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RevenueRecognition();
		}

		#endregion
	}
}
