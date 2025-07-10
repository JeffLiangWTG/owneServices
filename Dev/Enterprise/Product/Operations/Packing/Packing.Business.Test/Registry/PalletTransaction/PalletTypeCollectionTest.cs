using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PalletTypeCollection))]
	public class PalletTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<PalletTypeCollection>
	{
		protected sealed override bool RequiresFactory
		{
			get { return false; }
		}

		protected sealed override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PalletType();
		}

		protected override PalletTypeCollection GetCollectionToTest()
		{
			return new PalletTypeCollection();
		}
	}
}
