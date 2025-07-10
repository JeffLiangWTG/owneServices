using System;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(TrolleySlotInfo))]
	public class TrolleySlotInfoTestCase : DataObjectInfoTestCase<TrolleySlotInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_PackageID = "PKG1";
			package.CartonGroupAndSize = "GRP - SIZ";

			var trolleySlot = Factory.New<WhsPickTrolleySlot>();
			trolleySlot.WTS_KP_Package = package.PK;
			trolleySlot.WTS_SlotNumber = 3;

			var slotInfo1 = new TrolleySlotInfo();
			AssertNull("PackageID", slotInfo1.PackageID);
			AssertEquals("PackagePK", Guid.Empty, slotInfo1.PackagePK);
			AssertEquals("SlotNumber", (short)0, slotInfo1.SlotNumber);
			AssertNull("CartonGroupAndSize", slotInfo1.CartonGroupAndSize);

			var slotInfo2 = new TrolleySlotInfo(null);
			AssertNull("PackageID", slotInfo2.PackageID);
			AssertEquals("PackagePK", Guid.Empty, slotInfo2.PackagePK);
			AssertEquals("SlotNumber", (short)0, slotInfo2.SlotNumber);
			AssertNull("CartonGroupAndSize", slotInfo2.CartonGroupAndSize);

			var slotInfo3 = new TrolleySlotInfo(trolleySlot);
			AssertEquals("PackageID", "PKG1", slotInfo3.PackageID);
			AssertEquals("PackagePK", package.PK, slotInfo3.PackagePK);
			AssertEquals("SlotNumber", (short)3, slotInfo3.SlotNumber);
			AssertEquals("CartonGroupAndSize", "GRP - SIZ", slotInfo3.CartonGroupAndSize);

			var emptyPackageTrolleySlot = Factory.New<WhsPickTrolleySlot>();
			emptyPackageTrolleySlot.WTS_SlotNumber = 4;

			var slotInfo4 = new TrolleySlotInfo(emptyPackageTrolleySlot);
			AssertEquals("PackageID", "", slotInfo4.PackageID);
			AssertEquals("PackagePK", Guid.Empty, slotInfo4.PackagePK);
			AssertEquals("SlotNumber", (short)4, slotInfo4.SlotNumber);
			AssertEquals("CartonGroupAndSize", "", slotInfo4.CartonGroupAndSize);
		}

		#endregion

		#region TestProperties

		public void TestProperties()
		{
			var slotInfo = new TrolleySlotInfo(null);
			AssertNull("PackageID", slotInfo.PackageID);
			AssertEquals("SlotNumber", (short)0, slotInfo.SlotNumber);
			AssertNull("CartonGroupAndSize", slotInfo.CartonGroupAndSize);

			slotInfo.PackageID = "PKG1";
			slotInfo.SlotNumber = 2;
			slotInfo.CartonGroupAndSize = "GRP - SIZ";
			AssertEquals("PackageID", "PKG1", slotInfo.PackageID);
			AssertEquals("SlotNumber", (short)2, slotInfo.SlotNumber);
			AssertEquals("CartonGroupAndSize", "GRP - SIZ", slotInfo.CartonGroupAndSize);
		}

		#endregion

		#region Implementation

		protected new TrolleySlotInfo Parent
		{
			get { return (TrolleySlotInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new TrolleySlotInfo(null);
		}

		#endregion
	}
}
