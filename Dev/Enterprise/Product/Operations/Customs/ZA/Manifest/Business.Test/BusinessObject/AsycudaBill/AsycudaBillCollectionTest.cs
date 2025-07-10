using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var collection = header.Bills;
			header.AMA_ManifestType = "RFM";
			var child1 = collection.AddNew();
			AssertEquals(ZString.Empty, child1.CustomsEntryNumberType);
			header.AMA_ManifestType = "AQM";
			var child2 = collection.AddNew();
			AssertEquals(ZString.Empty, child2.CustomsEntryNumberType);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaBillCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.Bills;
		}
	}
}
