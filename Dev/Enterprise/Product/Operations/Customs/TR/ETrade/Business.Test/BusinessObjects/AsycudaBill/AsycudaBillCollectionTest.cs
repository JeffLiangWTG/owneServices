using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	public class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRemoveAndDeleteTotalBoxQtyAndNumberOfBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			bill1.ABL_ManifestQty = 50;
			bill2.ABL_ManifestQty = 50;
			Factory.Save();
			AssertEquals(100, header.TotalBoxQty);
			AssertEquals(2, header.NumberOfBills);

			header.Bills.RemoveAndDelete(bill1);

			Factory.Save();
			AssertEquals(50, header.TotalBoxQty);
			AssertEquals(1, header.NumberOfBills);

			var bill3 = header.Bills.AddNew();
			bill3.ABL_ManifestQty = 10;
			Factory.Save();
			AssertEquals(60, header.TotalBoxQty);
			AssertEquals(2, header.NumberOfBills);
		}

		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("KG", bill.ABL_GrossWeightUQ);
			AssertEquals("BI", bill.ABL_ManifestUQ);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaBillCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaBill.Schema.ABL_SpecialCargoCode };
		}
	}
}
