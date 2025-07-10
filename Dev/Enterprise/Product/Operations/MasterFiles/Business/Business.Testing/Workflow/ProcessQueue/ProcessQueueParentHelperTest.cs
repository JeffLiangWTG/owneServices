using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessQueueParentHelperTest : TestCaseWithFactory
	{
		public void TestCurrentQueueCreatedByLazyLoader()
		{
			ZQuery queueFilter = new ZQuery(ProcessQueueSchema.P4_ParentID, Parent.PK);
			AssertNull("Should not exist in DB", Factory.LoadTop1(typeof(ProcessQueue), queueFilter));
			AssertNotNull("Should be created", Helper.CurrentQueue);
			AssertNotNull("Should exist now", Factory.Load(typeof(ProcessQueue), queueFilter));
			AssertEquals("Should be assigned when queue is created", Parent, Helper.CurrentQueue.Parent);
		}

		public void TestCurrentQueue()
		{
			AssertNotNull("A new ProcessQueue record should be created in the lazy loader when there are no records in the DB", Helper.CurrentQueue);

			ProcessQueue queue = Factory.New<ProcessQueue>();
			queue.P4_ParentID = ZGuid.NewZGuid();
			queue.P4_ParentTableCode = "__";
			Helper.ResetCurrentQueue();
			Assert("ProcessQueue record should not be picked up by the filter. Should still be creating a new one", queue.PK != Helper.CurrentQueue.PK);

			queue.P4_ParentID = Parent.PK;
			Helper.ResetCurrentQueue();
			Assert("ParentTableCode is incorrect. ProcessQueue record should not be picked up by the filter. Should still be creating a new one", queue.PK != Helper.CurrentQueue.PK);

			queue.P4_ParentTableCode = "_1";
			Helper.ResetCurrentQueue();
			AssertEquals("Should be picked up by the filter", queue.PK, Helper.CurrentQueue.PK);
			Assert("CurrentQueue has to be registered as an editable child object", Parent.IsRegisteredEditableChildObject(Helper.CurrentQueue));
		}

		public void TestProcessQueueForBinding()
		{
			AssertEquals("The collection should have 1 child", 1, Helper.ActiveProcessQueueForBinding.Count);
			AssertEquals("Child should be the ActiveProcessQueue for the CurrentQueue", Helper.CurrentQueue, Helper.ActiveProcessQueueForBinding[0].ProcessQueue);
		}

		public void TestDeleteProcessQueue_NotInDatabase()
		{
			AssertEquals("Pre-condition", 1, Helper.ActiveProcessQueueForBinding.Count);
			Helper.DeleteProcessQueue();
			AssertEquals("Should be removed from the collection", 0, Helper.ActiveProcessQueueForBinding.Count);
		}

		public void TestDeleteProcessQueue_ProcessQueueInDatabase()
		{
			ZGuid currentQueuePK = Helper.CurrentQueue.PK;
			Factory.Save();

			AssertNotNull("Pre-condition", new BusinessObjectFactory().Load(typeof(ProcessQueue), currentQueuePK));
			AssertEquals("Pre-condition", 1, Helper.ActiveProcessQueueForBinding.Count);
			Helper.DeleteProcessQueue();
			Factory.Save();

			AssertNull("should be deleted now", new BusinessObjectFactory().Load(typeof(ProcessQueue), currentQueuePK));
			AssertEquals("Should be removed from the collection", 0, Helper.ActiveProcessQueueForBinding.Count);
		}

		public void TestDeleteProcessQueue_ProcessQueueNotLoadedAndInDatabase()
		{
			AssertNotNull("Pre-condition", Helper.CurrentQueue);
			Factory.Save();

			ProcessQueueParentHelper newHelper = new ProcessQueueParentHelper(Parent);
			newHelper.DeleteProcessQueue();
			AssertEquals("Should be deleted", true, Helper.CurrentQueue.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestDeleteProcessQueue_ShouldNotCreateANewQueueToBeDeletedIfNotYetCreated()
		{
			Helper.DeleteProcessQueue();
			foreach (BusinessObject bizO in (((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects))
			{
				if (bizO is ProcessQueue)
				{
					Fail("There should be no ProcessQueue object created");
				}
			}
		}

		#region ProcessQueueParentHelper with TypeOfProcessQueue parameter 

		public void TestProcessQueueParentHelperWithInvalidTypeOfProcessQueue()
		{
			try
			{
				ProcessQueueParentHelper helper = new ProcessQueueParentHelper(Parent, typeof(string));
				Fail("Exception should be thrown");
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Type has to be (or a subclass of) " + typeof(ProcessQueue).FullName, ex.Message);
			}
		}

		public void TestProcessQueueParentHelperWithSpecificTypeOfProcessQueue()
		{
			ProcessQueueParentHelper helper = new ProcessQueueParentHelper(Parent, typeof(ProcessQueueForTest));
			AssertEquals("Newly created queue has to be of ProcessQueueForTest type", typeof(ProcessQueueForTest), helper.CurrentQueue.GetType());
			Factory.Save();

			ProcessQueueParentHelper newHelper = new ProcessQueueParentHelper(Parent, typeof(ProcessQueueForTest));
			AssertEquals("Has to be loading the same record", helper.CurrentQueue.PK, newHelper.CurrentQueue.PK);
			AssertEquals("Loaded queue has to be of ProcessQueueForTest type", typeof(ProcessQueueForTest), newHelper.CurrentQueue.GetType());

			ProcessQueueParentHelper anotherHelper = new ProcessQueueParentHelper(Parent);
			AssertEquals("Has to be loading the same record", helper.CurrentQueue.PK, anotherHelper.CurrentQueue.PK);
			AssertEquals("No specific type specified in the constructor, has to be using the base type", typeof(ProcessQueue), anotherHelper.CurrentQueue.GetType());
		}

		#region ProcessQueueForTest

		class ProcessQueueForTest : ProcessQueue
		{
			public ProcessQueueForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		#endregion

		#endregion

		#region Implementation

		ProcessQueueParentHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new ProcessQueueParentHelper(Parent);
				}
				return fHelper;
			}
		}

		ProcessQueueParentForTest Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = Factory.New<ProcessQueueParentForTest>();
				}
				return fParent;
			}
		}

		ProcessQueueParentHelper fHelper;
		ProcessQueueParentForTest fParent;

		#endregion
	}
}
