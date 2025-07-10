using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentForACASWrapperCollection))]
	public class HVLVConsignmentForACASWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<HVLVConsignmentForACASWrapperCollection>
	{
		public void TestAddNew_ShouldNotBeSupported()
		{
			var collection = GetCollectionToTest();
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}

		protected override HVLVConsignmentForACASWrapperCollection GetCollectionToTest() => new HVLVConsignmentForACASWrapperCollection(Array.Empty<HVLVConsignment>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new HVLVConsignmentForACASWrapper(Factory.NewWithValidTestData<HVLVConsignment>());
	}
}
