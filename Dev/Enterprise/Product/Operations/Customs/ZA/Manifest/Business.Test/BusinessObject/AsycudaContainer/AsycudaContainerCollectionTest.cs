using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerCollection))]
	sealed class AsycudaContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultsForNewObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var collection = header.Containers;
			header.AMA_Nature = NatureList.Codes.Export22;
			var container1 = collection.AddNew();
			AssertEquals(LandedPurposeList.Codes.Export, container1.GetSystemDefinedValue<ZString>(AsycudaContainer.Schema.LandedPurpose));

			header.AMA_Nature = NatureList.Codes.Import23;
			var container2 = collection.AddNew();
			AssertEquals(LandedPurposeList.Codes.Import, container2.GetSystemDefinedValue<ZString>(AsycudaContainer.Schema.LandedPurpose));

			header.AMA_Nature = ZaShipmentTypeList.Codes.MutualMultipleZzz;
			var container3 = collection.AddNew();
			AssertEquals(ZString.Empty, container3.GetSystemDefinedValue<ZString>(AsycudaContainer.Schema.LandedPurpose));

			header.AMA_Nature = NatureList.Codes.Transhipment28;
			var container4 = Factory.New<AsycudaContainer>();
			container4.LandedPurpose = "AR1";
			collection.Add(container4);
			AssertEquals("AR1", container4.GetSystemDefinedValue<ZString>(AsycudaContainer.Schema.LandedPurpose));
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaContainerCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.Containers;
		}
	}
}
