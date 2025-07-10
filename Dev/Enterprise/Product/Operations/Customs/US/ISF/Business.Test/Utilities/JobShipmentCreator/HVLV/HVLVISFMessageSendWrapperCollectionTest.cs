using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(HVLVISFMessageSendWrapperCollection))]
	public class HVLVISFMessageSendWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<HVLVISFMessageSendWrapperCollection>
	{
		public void TestAddNew_ShouldNotBeSupported()
		{
			var collection = GetCollectionToTest();
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}

		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		protected override HVLVISFMessageSendWrapperCollection GetCollectionToTest() => new HVLVISFMessageSendWrapperCollection(new RelatedJobCollection(Factory));

		protected override BusinessObject GetNewElementToAddToTheCollection() => new HVLVISFMessagesSendWrapper(Factory.NewWithValidTestData<CusISFHeader>());
	}
}
