using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaPackCollection))]
	sealed class AsycudaPackCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestMaxCount()
		{
			var testCollection = GetCollectionToTest();
			AssertEquals(9999, testCollection.MaxCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs;
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaPackCollection);
	}
}
