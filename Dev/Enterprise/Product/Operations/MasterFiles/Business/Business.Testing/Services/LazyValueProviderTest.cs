using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LazyValueProviderTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			AssertNoExceptionThrown(() => { LazyValueProvider<BusinessObject> lazy = new LazyValueProvider<BusinessObject>(); });
			AssertNoExceptionThrown(() => { LazyValueProvider<BusinessObject> lazy = new LazyValueProvider<BusinessObject>(() => Factory.New<DummyBusinessObject>()); });
		}

		public void TestValue()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			LazyValueProvider<BusinessObject> lazy1 = new LazyValueProvider<BusinessObject>(() => dummy);

			AssertEquals(false, lazy1.IsValueCreated);
			AssertEquals(dummy, lazy1.Value);
			AssertEquals(true, lazy1.IsValueCreated);

			lazy1.ValueProvider = null;

			AssertEquals(false, lazy1.IsValueCreated);
			AssertEquals(null, lazy1.Value);
			AssertEquals(true, lazy1.IsValueCreated);

			LazyValueProvider<int> lazy2 = new LazyValueProvider<int>();

			AssertEquals(false, lazy2.IsValueCreated);
			AssertEquals(0, lazy2.Value);
			AssertEquals(true, lazy2.IsValueCreated);
		}
	}
}
