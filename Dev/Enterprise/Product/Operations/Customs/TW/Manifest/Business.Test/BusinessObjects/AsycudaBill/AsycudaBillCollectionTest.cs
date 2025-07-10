using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaBillCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.Bills;
		}

		public void TestDefaultMessageStatus()
		{
			var bill = (AsycudaBill)GetCollectionToTest().AddNew();
			AssertEquals(TWMessageStatusCodeList.Codes.NotSent, bill.ABL_MessageStatus);
		}

		public void TestDefaultGoodsLocation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_GoodsLocationFromMasterBill = "ANP0060D";

			var bill = header.Bills.AddNew();
			AssertEquals("For SetDefaultsForNewChild", "ANP0060D", bill.ABL_GoodsLocation);

			var bill2 = Factory.New<AsycudaBill>();
			header.Bills.Add(bill2);
			AssertEquals("For Add", "ANP0060D", bill2.ABL_GoodsLocation);
		}
	}
}
