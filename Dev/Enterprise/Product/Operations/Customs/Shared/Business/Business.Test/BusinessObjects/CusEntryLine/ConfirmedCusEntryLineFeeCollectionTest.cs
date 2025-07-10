using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ConfirmedCusEntryLineFeeCollection))]
	public class ConfirmedCusEntryLineFeeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestConstructor()
		{
			AssertNotNull(Collection);
		}

		public void TestOverriddenAddNew()
		{
			Assert(GetTypeOfElement().IsAssignableFrom(Collection.AddNew().GetType()));
		}

		protected virtual Type GetTypeOfElement()
		{
			return typeof(CusEntryLineFee);
		}

		public void TestTypedIndexer()
		{
			CusEntryLineFee fee = Collection.AddNew();
			AssertEquals(fee, Collection[0]);
		}

		public void TestFeeIsConfirmed()
		{
			CusEntryLineFee fee = Collection.AddNew();
			AssertEquals(CusEntryLineFeeSourceCodeList.Codes.CUS, fee.CF_Source);
			Assert(fee.IsConfirmed);
		}

		#region Implementation
		protected new ConfirmedCusEntryLineFeeCollection Collection => (ConfirmedCusEntryLineFeeCollection)base.Collection;

		#endregion

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ConfirmedCusEntryLineFeeCollection(Factory.New<CusEntryLine>());
		}
	}
}
