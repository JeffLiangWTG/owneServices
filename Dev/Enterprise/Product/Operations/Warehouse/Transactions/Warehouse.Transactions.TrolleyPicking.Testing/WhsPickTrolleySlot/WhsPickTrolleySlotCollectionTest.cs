namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	using Business.Testing;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using NUnit.Framework;

	[TestedType(typeof(WhsPickTrolleySlotCollection))]
	class WhsPickTrolleySlotCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsPickTrolleySlotCollection>
	{
		#region TestSetDefaultsForNewElementCore

		public void TestSetDefaultsForNewElementCore()
		{
			// Without Trolley Job
			var collection1 = new WhsPickTrolleySlotCollection(Factory);
			var slot1 = collection1.AddNew();
			AssertEquals("No trolley job should be associated with new slot.", ZGuid.Empty, slot1.WTS_WTJ_TrolleyJob);

			// With Trolley Job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			var collection2 = new WhsPickTrolleySlotCollection(trolleyJob);
			var slot2 = collection2.AddNew();
			AssertEquals("The new slot should be linked to master Trolley Job.", trolleyJob.PK, slot2.WTS_WTJ_TrolleyJob);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
