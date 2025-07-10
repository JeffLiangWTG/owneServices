using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	public static class HelperTrolleyExtensions
	{
		#region CreateWhsPickTrolleyJob

		public static WhsPickTrolleyJob CreateWhsPickTrolleyJob(this WhsTestHelperFunctions helper, RefEquipment trolley, string jobStatus = PickTrolleyStatus.Codes.Building)
		{
			var pickTrolley = helper.Factory.New<WhsPickTrolleyJob>();
			pickTrolley.WTJ_RQ_Equipment = trolley.PK;
			pickTrolley.WTJ_Status = jobStatus;

			return pickTrolley;
		}

		#endregion

		#region CreateWhsPickTrolleySlot

		public static WhsPickTrolleySlot CreateWhsPickTrolleySlot(this WhsTestHelperFunctions helper, WhsPickTrolleyJob trolleyJob, PkgPackage package, ZShort slot)
		{
			var trolleySlot = trolleyJob.Slots.AddNew();
			trolleySlot.WTS_KP_Package = package.PK;
			trolleySlot.WTS_SlotNumber = slot;

			return trolleySlot;
		}

		#endregion
	}
}
