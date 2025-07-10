using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestProcessTaskCollectionA : ProcessTaskCollection
	{
		public TestProcessTaskCollectionA(BusinessObject parent) : base(parent)
		{
		}

		public TestProcessTaskCollectionA(BusinessObjectFactory factory) : base(factory)
		{
		}

		public TestProcessTaskCollectionA(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		public TestProcessTaskCollectionA(BusinessObject parent, ZQuery additionalFilter) : base(parent, additionalFilter)
		{
		}
	}

	sealed class TestProcessTaskCollectionB : ProcessTaskCollection
	{
		public TestProcessTaskCollectionB(BusinessObject parent) : base(parent)
		{
		}

		public TestProcessTaskCollectionB(BusinessObjectFactory factory) : base(factory)
		{
		}

		public TestProcessTaskCollectionB(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		public TestProcessTaskCollectionB(BusinessObject parent, ZQuery additionalFilter) : base(parent, additionalFilter)
		{
		}
	}

	sealed class ProcessTaskCollectionFactoryTests : TestCaseWithFactory
	{
		public void TestFactoryFactory()
		{
			var fact1 = ProcessTaskCollectionFactoryFactory.NewInstance;
			var fact2 = ProcessTaskCollectionFactoryFactory.NewInstance;

			AssertNotEquals(fact1, fact2);
		}

		public void TestFactoryReturnsSameInstanceSamePK()
		{
			var factory = ProcessTaskCollectionFactoryFactory.NewInstance;
			var pk = ZGuid.NewZGuid();

			var collection1 = factory.GetOrCreate(pk, () => new TestProcessTaskCollectionA(Factory));
			var collection2 = factory.GetOrCreate(pk, () => new TestProcessTaskCollectionA(Factory));
			AssertEquals(collection1, collection2);
		}

		public void TestFactoryReturnsDifferentInstanceDifferentPK()
		{
			var factory = ProcessTaskCollectionFactoryFactory.NewInstance;
			var collection1 = factory.GetOrCreate(ZGuid.NewZGuid(), () => new TestProcessTaskCollectionA(Factory));
			var collection2 = factory.GetOrCreate(ZGuid.NewZGuid(), () => new TestProcessTaskCollectionA(Factory));
			AssertNotEquals(collection1, collection2);
		}

		public void TestFactoryReturnsSameInstanceSamePKSameExtraConstraint()
		{
			var factory = ProcessTaskCollectionFactoryFactory.NewInstance;
			var pk = ZGuid.NewZGuid();

			var collection1 = factory.GetOrCreate(pk, "a key", () => new TestProcessTaskCollectionA(Factory));
			var collection2 = factory.GetOrCreate(pk, "a key", () => new TestProcessTaskCollectionA(Factory));
			AssertEquals(collection1, collection2);
		}

		public void TestFactoryReturnsDifferentInstanceSamePKDifferentExtraConstraint()
		{
			var factory = ProcessTaskCollectionFactoryFactory.NewInstance;
			var pk = ZGuid.NewZGuid();

			var collection1 = factory.GetOrCreate(pk, "a key", () => new TestProcessTaskCollectionA(Factory));
			var collection2 = factory.GetOrCreate(pk, "diff key", () => new TestProcessTaskCollectionA(Factory));
			AssertNotEquals(collection1, collection2);
		}

		public void TestFactoryProcessTaskCollectionTypeUniqueness()
		{
			var factory = ProcessTaskCollectionFactoryFactory.NewInstance;
			var pk = ZGuid.NewZGuid();

			var collection1 = factory.GetOrCreate(pk, "a key", () => new TestProcessTaskCollectionA(Factory));
			var collection2 = factory.GetOrCreate(pk, "a key", () => new TestProcessTaskCollectionB(Factory));
			AssertNotEquals(collection1, collection2);
		}

		public void TestFactoryExtraConstraintTypeDifference()
		{
			var factory = ProcessTaskCollectionFactoryFactory.NewInstance;
			var pk = ZGuid.NewZGuid();

			var collection1 = factory.GetOrCreate(pk, "a key", () => new TestProcessTaskCollectionA(Factory));
			var collection2 = factory.GetOrCreate(pk, 5, () => new TestProcessTaskCollectionA(Factory));
			AssertNotEquals(collection1, collection2);
		}
	}
}
