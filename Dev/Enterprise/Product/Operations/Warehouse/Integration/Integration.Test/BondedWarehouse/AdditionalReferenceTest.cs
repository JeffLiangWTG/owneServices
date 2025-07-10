using NUnit.Framework;

namespace Enterprise.Warehouse.Integration.BondedWarehouse.Testing
{
	class AdditionalReferenceTest : TestCase
	{
		public void TestConstruction()
		{
			AdditionalReference reference = new AdditionalReference("Type", "Value");
			AssertEquals(reference.Type, "Type");
			AssertEquals(reference.Value, "Value");
		}
	}
}