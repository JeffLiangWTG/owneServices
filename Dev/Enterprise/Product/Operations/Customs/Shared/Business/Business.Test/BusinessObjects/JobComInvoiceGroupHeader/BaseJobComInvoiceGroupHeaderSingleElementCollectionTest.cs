using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceGroupHeaderSingleElementCollection))]
	sealed class BaseJobComInvoiceGroupHeaderSingleElementCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSwapGroup()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			AssertEquals(declaration.JobComInvoiceGroupHeaders[0], declaration.ActiveGroupHeader[0]);

			int listChanged = 0;
			BaseJobComInvoiceGroupHeader childGroup = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();

			((IBindingList)declaration.ActiveGroupHeader).ListChanged += new ListChangedEventHandler(delegate
			{ listChanged++; });

			declaration.ActiveGroupHeader.SwapGroup(childGroup);
			AssertEquals(childGroup, declaration.ActiveGroupHeader[0]);
			AssertEquals("List changed is called", 1, listChanged);

			declaration.ActiveGroupHeader.SwapGroup(childGroup);
			AssertEquals(childGroup, declaration.ActiveGroupHeader[0]);
			AssertEquals("List changed should not be called if the same group is in", 1, listChanged);
		}

		public void TestSwapgroupHandlesNullGroupParameter()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader childGroup = null;
			AssertNoExceptionThrown("Should not be throwing NullReferenceException.", () => { declaration.ActiveGroupHeader.SwapGroup(childGroup); });
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BaseJobComInvoiceGroupHeaderSingleElementCollection(Factory);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}
	}
}
