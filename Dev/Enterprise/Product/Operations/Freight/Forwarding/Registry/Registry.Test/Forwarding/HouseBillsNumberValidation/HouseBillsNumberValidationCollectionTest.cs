using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(HouseBillsNumberValidationCollection))]
	internal sealed class HouseBillsNumberValidationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<HouseBillsNumberValidationCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override HouseBillsNumberValidationCollection GetCollectionToTest()
		{
			return new HouseBillsNumberValidationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HouseBillsNumberValidation();
		}

		#endregion
	}
}
