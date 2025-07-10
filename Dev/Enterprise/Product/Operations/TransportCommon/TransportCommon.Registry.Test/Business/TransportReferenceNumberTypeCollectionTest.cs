using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(TransportReferenceNumberTypeCollection))]
	class TransportReferenceNumberTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<TransportReferenceNumberTypeCollection>
	{
		public void TestAddSetsParent()
		{
			TransportReferenceNumberTypeCollection collection = new TransportReferenceNumberTypeCollection();
			TransportReferenceNumberType transportReferenceNumberType1 = collection.AddNew();

			AssertEquals(collection, transportReferenceNumberType1.Parent);

			TransportReferenceNumberType transportReferenceNumberType2 = new TransportReferenceNumberType();
			collection.Add(transportReferenceNumberType2);

			AssertEquals(collection, transportReferenceNumberType1.Parent);
		}

		public void TestAddSystemDefined()
		{
			var collection = new TransportReferenceNumberTypeCollection();
			var transportReferenceNumberType1 = collection.AddSystemDefined("A", (NoResString)"B", isUnique: true);

			AssertEquals("Code", "A", transportReferenceNumberType1.Code);
			AssertEquals("Description", "B", transportReferenceNumberType1.Description);
			Assert("IsUnique", transportReferenceNumberType1.IsUnique);
			Assert("SystemDefined", transportReferenceNumberType1.SystemDefined);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override TransportReferenceNumberTypeCollection GetCollectionToTest()
		{
			return new TransportReferenceNumberTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TransportReferenceNumberType();
		}
	}
}
