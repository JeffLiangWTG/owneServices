using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TNPAAccountNumberCollection))]
	sealed class TNPAAccountNumberCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TNPAAccountNumberCollection>
	{
		public void TestDuplicateRegistryEntry()
		{
			var collection = new TNPAAccountNumberCollection();
			var registryEntry = new TNPAAccountNumber();
			registryEntry.Port = "AUSYD";
			registryEntry.ImportNumber = "I001";
			registryEntry.ExportNumber = "E001";
			registryEntry.CoastwiseNumber = "C001";

			collection.Add(registryEntry);

			var duplicateRegistryEntry = new TNPAAccountNumber();
			duplicateRegistryEntry.Port = "AUSYD";
			duplicateRegistryEntry.ImportNumber = "I002";
			duplicateRegistryEntry.ExportNumber = "E002";
			duplicateRegistryEntry.CoastwiseNumber = "C002";

			AssertEquals(true, collection.IsDuplicateItem(duplicateRegistryEntry));
			AssertEquals(false, collection.IsDuplicateItem(null));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override TNPAAccountNumberCollection GetCollectionToTest()
		{
			return new TNPAAccountNumberCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TNPAAccountNumber();
		}

		#endregion
	}
}
