using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLHeaderCollection))]
	sealed class AccGLHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccGLHeaderCollectionForTest(Factory);
		}

		public void TestShowGLAccountsForImportAction()
		{
			var collection = (AccGLHeaderCollectionForTest)GetCollectionToTest();
			collection.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			var provider = (AccGLHeaderFindBoxListProvider)collection.FindBoxListProvider_ForTest;
			AssertNotNull(provider.ShowGLAccountsForImportAction);
			AssertEquals(collection.ShowGLAccountsForImportAction, provider.ShowGLAccountsForImportAction);
		}
	}

	class AccGLHeaderCollectionForTest : AccGLHeaderCollection
	{
		public AccGLHeaderCollectionForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccGLHeaderCollectionForTest(BusinessObjectFactory factory, AccTransactionLines transactionLines, Action<AccGLHeaderCollection, List<AccGLHeader>> glAccountAction) : base(factory, transactionLines, glAccountAction)
		{
		}

		public IFindBoxListProvider FindBoxListProvider_ForTest => FindBoxListProvider;
	}
}
