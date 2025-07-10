using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingImageCollection))]
	public class BillOfLadingImageCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BillOfLadingImageCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BillOfLadingImageCollection GetCollectionToTest()
		{
			return new BillOfLadingImageCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BillOfLadingImage();
		}

		#endregion

	}
}
