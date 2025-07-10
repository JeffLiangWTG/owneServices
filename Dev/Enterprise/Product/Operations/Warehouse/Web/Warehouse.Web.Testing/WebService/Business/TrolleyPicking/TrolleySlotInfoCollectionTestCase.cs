using System;
using System.Linq;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class TrolleySlotInfoCollectionTestCase : DataObjectInfoCollectionTestCase<TrolleySlotInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var pkgPackageJob = Factory.New<PkgPackageJob>();
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			var trolleySlot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			var trolleySlot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);

			var slotInfoCollection = new TrolleySlotInfoCollection(trolleyJob.Slots);
			AssertEquals("Should have 2 slots added to collection", 2, slotInfoCollection.Count);
			slotInfoCollection.Single(s => s.PackageID == pkgPackage1.KP_PackageID && s.SlotNumber == 2);
			slotInfoCollection.Single(s => s.PackageID == pkgPackage2.KP_PackageID && s.SlotNumber == 5);
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(TrolleySlotInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(TrolleySlotInfo);
		}

		protected override TrolleySlotInfo GetNewObjectInfo()
		{
			return new TrolleySlotInfo(null);
		}

		protected override DataObjectInfoCollection<TrolleySlotInfo> GetNewObjectInfoCollection()
		{
			return new TrolleySlotInfoCollection();
		}

		#endregion
	}
}
