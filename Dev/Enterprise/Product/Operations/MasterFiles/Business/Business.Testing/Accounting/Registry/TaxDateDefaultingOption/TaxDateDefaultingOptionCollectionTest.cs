using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxDateDefaultingOptionCollection))]
	sealed class TaxDateDefaultingOptionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TaxDateDefaultingOptionCollection>
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

		protected override TaxDateDefaultingOptionCollection GetCollectionToTest()
		{
			return new TaxDateDefaultingOptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TaxDateDefaultingOption();
		}

		#endregion
	}
}
