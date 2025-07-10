using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveProcessQueueCollection))]
	sealed class ActiveProcessQueueCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ActiveProcessQueueCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals("Should not allow new", false, Collection.AllowNew);
		}

		public new void TestAddNew()
		{
			try
			{
				Collection.AddNew();
				Fail("Exception should be thrown");
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Should not be creating new ActiveProcessQueue. This class is only used for binding.", ex.Message);
			}
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			// This collection can never be added to via Binding.
			// The AddNew() method throws a NotSupportedException.
			Assert(true);
		}

		protected override ActiveProcessQueueCollection GetCollectionToTest()
		{
			return new ActiveProcessQueueCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProcessQueue processQueue = Factory.New<ProcessQueue>();
			return ActiveProcessQueue.New(processQueue);
		}

		new ActiveProcessQueueCollection Collection
		{
			get { return base.Collection; }
		}
	}
}
