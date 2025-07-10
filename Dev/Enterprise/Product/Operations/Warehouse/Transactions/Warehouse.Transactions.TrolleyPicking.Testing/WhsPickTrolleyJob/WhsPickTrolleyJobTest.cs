using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	[TestedType(typeof(WhsPickTrolleyJob))]
	internal class WhsPickTrolleyJobTest : WhsBusinessObjectTestCase
	{
		#region TestSlots

		public void TestSlots()
		{
			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			AssertEquals("Precondition", 0, trolleyJob.Slots.Count);

			var trolleySlot = trolleyJob.Slots.AddNew();
			AssertEquals(trolleySlot, trolleyJob.Slots.Single());
			AssertEquals("Slots should be editable children for trolley job.", true, trolleyJob.IsRegisteredEditableChildObject(trolleyJob.Slots));
		}

		#endregion

		#region Properties

		#region TestWTJ_CriticalVersionIDInfo_ConcurrencyPolicy

		public void TestWTJ_CriticalVersionIDInfo_ConcurrencyPolicy()
		{
			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			AssertEquals(ConcurrencyPolicy.Strict, trolleyJob.WTJ_CriticalChangesVersionIDInfo.ConcurrencyPolicy);
		}

		#endregion

		[TestDate(2016, 2, 29, 4, 45, 0)]
		public void TestWTJ_Status_SetsWTJ_FinalisedDateUtc()
		{
			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			AssertEquals("Precondition", PickTrolleyStatus.Codes.Building, trolleyJob.WTJ_Status);
			AssertEquals("Precondition", ZDateTime.Empty, trolleyJob.WTJ_FinalisedDateUtc);

			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Picking;
			AssertEquals(PickTrolleyStatus.Codes.Picking, trolleyJob.WTJ_Status);
			AssertEquals("Finalised date should not be set when status is Picking.", ZDateTime.Empty, trolleyJob.WTJ_FinalisedDateUtc);

			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Finalised;
			AssertEquals(PickTrolleyStatus.Codes.Finalised, trolleyJob.WTJ_Status);
			AssertEquals("When seting trolley job status to FIN it should test Finalised Date automatically.", ZDateTime.UtcNow, trolleyJob.WTJ_FinalisedDateUtc);

			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Building;
			AssertEquals(PickTrolleyStatus.Codes.Building, trolleyJob.WTJ_Status);
			AssertEquals("Changing status from finalised to any other should clear the finalised date.", ZDateTime.Empty, trolleyJob.WTJ_FinalisedDateUtc);
		}

		public void TestPickingType()
		{
			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			trolleyJob.WTJ_RQ_Equipment = Helper.CreateTrolley("T1").PK;
			AssertEquals("Precondition", false, trolleyJob.Slots.Count > 0);
			AssertEquals("Nothing in the slots, trolley is none type", TrolleyPickingType.None, trolleyJob.PickingType);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var tote = packageJob.Packages.AddNew(Core.Constants.PkgUnit.Tote);

			var newSlot = trolleyJob.Slots.AddNew();
			AssertEquals("Slot doesn't have a Package yet, so should still be none", TrolleyPickingType.None, trolleyJob.PickingType);

			newSlot.WTS_KP_Package = tote.PK;
			newSlot.WTS_SlotNumber = 1;
			AssertEquals("Carton in the slots, trolley is of Carton type", TrolleyPickingType.Carton, trolleyJob.PickingType);

			tote.SetIsTote(true);
			AssertEquals("Tote in the slots, trolley is of Tote type", TrolleyPickingType.Tote, trolleyJob.PickingType);
		}

		#endregion

		#region IWhsPickTrolleyJob Members

		public void TestIWhsPickTrolleyJob_PK()
		{
			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			AssertEquals("Precondition", trolleyJob.PK, ((IWhsPickTrolleyJob)trolleyJob).PK);
		}

		public void TestIWhsPickTrolleyJob_WTJ_RQ_Equipment()
		{
			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			trolleyJob.WTJ_RQ_Equipment = Helper.CreateTrolley("T1").PK;
			AssertEquals("Precondition", trolleyJob.WTJ_RQ_Equipment, ((IWhsPickTrolleyJob)trolleyJob).WTJ_RQ_Equipment);
		}

		public void TestIWhsPickTrolleyJob_WTJ_Status()
		{
			var trolleyJob = Factory.New<WhsPickTrolleyJob>();
			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Picking;
			AssertEquals("Precondition", trolleyJob.WTJ_Status, ((IWhsPickTrolleyJob)trolleyJob).WTJ_Status);
		}

		#endregion
	}
}
