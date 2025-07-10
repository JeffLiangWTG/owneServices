using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FactoryExtensionsTest : TransactionedTestCase
	{
		public void TestImportFromAnotherFactorySafe()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			AssertNull(factory1.ImportFromAnotherFactorySafe<DummyBusinessObject>(null));

			var dummy = factory1.NewWithValidTestData<DummyBusinessObject>();
			factory1.Save();

			factory2.ImportFromAnotherFactorySafe(dummy);
			var dummyImported = factory2.ImportFromAnotherFactorySafe(dummy);

			AssertEquals(dummyImported.PK, dummy.PK);
			AssertEquals(dummy, factory1.ImportFromAnotherFactorySafe(dummy));
			AssertEquals(factory2.Load<DummyBusinessObject>(dummy.PK).PK, dummy.PK);
			AssertEquals(0, factory2.DatabaseLoadCount);
		}

		public void TestSet()
		{
			BusinessObjectFactory factory = null;
			AssertExceptionThrown("factory", typeof(ArgumentNullException), () => factory.SetValue<IA, A1>());

			factory = new BusinessObjectFactory();

			IA value = factory.GetValue<IA>();

			AssertNull(value);

			factory.SetValue<IA, A1>();

			value = factory.GetValue<IA>();

			AssertNotNull(value);
			Assert(value is A1);

			factory.SetValue<IA, A2>();

			value = factory.GetValue<IA>();

			AssertNotNull(value);
			Assert(value is A2);
		}

		public void TestSetWithProvider()
		{
			BusinessObjectFactory factory = null;
			AssertExceptionThrown("factory", typeof(ArgumentNullException), () => factory.SetValue<IA>(() => new A1()));

			factory = new BusinessObjectFactory();

			A1 impl = new A1();
			factory.SetValue<IA>(() => impl);

			AssertEquals(impl, factory.GetValue<IA>());
		}

		public void TestGet()
		{
			BusinessObjectFactory factory = null;
			AssertExceptionThrown("factory", typeof(ArgumentNullException), () => factory.GetValue<IA>());

			factory = new BusinessObjectFactory();

			AssertNull(factory.GetValue<IA>());
			AssertNull(factory.GetValue<IB>());

			factory.SetValue<IA, A1>();

			IA value1 = factory.GetValue<IA>();
			IB value2 = factory.GetValue<IB>();

			AssertNotNull(value1);
			AssertNull(value2);
			Assert(value1 is A1);

			factory.SetValue<IB, B1>();

			value1 = factory.GetValue<IA>();
			value2 = factory.GetValue<IB>();

			AssertNotNull(value1);
			AssertNotNull(value2);
			Assert(value1 is A1);
			Assert(value2 is B1);

			factory.SetValue<IA, A2>();

			value1 = factory.GetValue<IA>();
			value2 = factory.GetValue<IB>();

			AssertNotNull(value1);
			AssertNotNull(value2);
			Assert(value1 is A2);
			Assert(value2 is B1);
		}

		public void TestUsesSameInstance()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			factory.SetValue<IA, A1>();

			IA value1 = factory.GetValue<IA>();

			AssertNotNull(value1);

			IA value2 = factory.GetValue<IA>();

			AssertEquals(value1, value2);

			factory.SetValue<IA, A2>();

			IA value3 = factory.GetValue<IA>();

			AssertNotEquals(value1, value3);
		}

		public void TestRemove()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			AssertNull(factory.GetValue<IA>());

			factory.SetValue<IA, A1>();

			AssertNotNull(factory.GetValue<IA>());

			factory.RemoveValue<IA>();

			AssertNull(factory.GetValue<IA>());
		}

		public void TestLazy()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			int providerCalls = 0;

			factory.SetValue<IA>(() => { providerCalls++; return new A1(); });
			AssertEquals("should not call provider on set", 0, providerCalls);

			factory.GetValue<IA>();
			AssertEquals("should call provider on get", 1, providerCalls);

			factory.GetValue<IA>();
			AssertEquals("value is cached so provider should be called only once", 1, providerCalls);
		}

		interface IA
		{
		}

		interface IB : IA
		{
		}

		class A1 : IA
		{
		}

		class A2 : IA
		{
		}

		class B1 : IB
		{
		}
	}
}
