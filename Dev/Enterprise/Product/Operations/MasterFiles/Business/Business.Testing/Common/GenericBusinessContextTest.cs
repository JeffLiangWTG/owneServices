using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GenericBusinessContextTest : TestCaseWithFactory
	{
		public void TestHasContext()
		{
			AssertFactoryHasContext(false);

			var bizO = Factory.New<DummyBusinessObject>();
			AssertBusinessObjectHasContext(bizO, false);

			Factory.SetContext(MyTestContexts.Context1);
			AssertBusinessObjectHasContext(bizO, true);
			AssertFactoryHasContext(true);

			// Can set context more than once but should remove it the same number of times
			Factory.SetContext(MyTestContexts.Context1);
			AssertBusinessObjectHasContext(bizO, true);
			AssertFactoryHasContext(true);

			Factory.RemoveContext(MyTestContexts.Context1);
			// Still have context
			AssertBusinessObjectHasContext(bizO, true);
			AssertFactoryHasContext(true);

			// Remove it from the Factory
			Factory.RemoveContext(MyTestContexts.Context1);
			AssertFactoryHasContext(false);
			AssertBusinessObjectHasContext(bizO, false);

			// Set context on BizO
			bizO.SetContext(MyTestContexts.Context1);
			AssertBusinessObjectHasContext(bizO, true);
			// Does not affect factory context
			AssertFactoryHasContext(false);

			// Can set context more than once but should remove it the same number of times
			bizO.SetContext(MyTestContexts.Context1);
			AssertBusinessObjectHasContext(bizO, true);

			bizO.RemoveContext(MyTestContexts.Context1);
			// Still have context
			AssertBusinessObjectHasContext(bizO, true);

			bizO.RemoveContext(MyTestContexts.Context1);
			AssertBusinessObjectHasContext(bizO, false);
		}

		public void TestCanSetMoreThanOneContext()
		{
			AssertFactoryHasContext(false, false);

			var bizO = Factory.New<DummyBusinessObject>();
			AssertBusinessObjectHasContext(bizO, false, false);

			Factory.SetContext(MyTestContexts.Context1);
			AssertFactoryHasContext(true, false);
			AssertBusinessObjectHasContext(bizO, true, false);

			Factory.SetContext(MyTestContexts.Context2);
			AssertFactoryHasContext(true, true);
			AssertBusinessObjectHasContext(bizO, true, true);

			Factory.RemoveContext(MyTestContexts.Context1);
			AssertFactoryHasContext(false, true);
			AssertBusinessObjectHasContext(bizO, false, true);

			bizO.SetContext(MyTestContexts.Context1);
			AssertFactoryHasContext(false, true);
			AssertBusinessObjectHasContext(bizO, true, true);

			Factory.RemoveContext(MyTestContexts.Context2);
			AssertFactoryHasContext(false, false);
			AssertBusinessObjectHasContext(bizO, true, false);

			bizO.SetContext(MyTestContexts.Context2);
			AssertFactoryHasContext(false, false);
			AssertBusinessObjectHasContext(bizO, true, true);

			bizO.RemoveContext(MyTestContexts.Context1);
			AssertBusinessObjectHasContext(bizO, false, true);

			bizO.RemoveContext(MyTestContexts.Context2);
			AssertBusinessObjectHasContext(bizO, false, false);
		}

		public void TestHasContextWithoutFactoryFallback()
		{
			AssertFactoryHasContext(false);

			var bizO = Factory.New<DummyBusinessObject>();
			AssertBusinessObjectHasContext(bizO, false);

			Factory.SetContext(MyTestContexts.Context1);
			AssertBusinessObjectHasContextWithoutFactoryFallback(bizO, false);
			AssertFactoryHasContext(true);

			// Can set context more than once but should remove it the same number of times
			AssertBusinessObjectHasContextWithoutFactoryFallback(bizO, false);
			AssertFactoryHasContext(true);

			Factory.RemoveContext(MyTestContexts.Context1);
			bizO.SetContext(MyTestContexts.Context1);

			AssertBusinessObjectHasContextWithoutFactoryFallback(bizO, true);
			AssertFactoryHasContext(false);
		}

		void AssertFactoryHasContext(bool hasContext1, bool hasContext2 = false)
		{
			AssertEquals("Factory Has Context1", hasContext1, Factory.HasContext(MyTestContexts.Context1));
			AssertEquals("Factory Has Context2", hasContext2, Factory.HasContext(MyTestContexts.Context2));
		}

		void AssertBusinessObjectHasContext(BusinessObject bizObj, bool hasContext1, bool hasContext2 = false)
		{
			AssertEquals("BusinessObject Has Context1", hasContext1, bizObj.HasContext(MyTestContexts.Context1));
			AssertEquals("BusinessObject Has Context2", hasContext2, bizObj.HasContext(MyTestContexts.Context2));
		}

		void AssertBusinessObjectHasContextWithoutFactoryFallback(BusinessObject bizObj, bool hasContext1, bool hasContext2 = false)
		{
			AssertEquals("BusinessObject Has Context1", hasContext1, bizObj.HasContextWithoutFactoryFallback(MyTestContexts.Context1));
			AssertEquals("BusinessObject Has Context2", hasContext2, bizObj.HasContextWithoutFactoryFallback(MyTestContexts.Context2));
		}

		internal enum MyTestContexts
		{
			Context1,
			Context2
		}
	}
}
