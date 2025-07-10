using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackCollection))]
	class AsycudaPackCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAutoLinkBetweenPackAndContainerWhenCOMorCOHManifestTypeAndOnlyOneContainer()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "COM";
			var container1 = header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("Pack should auto select container when there is only one and COM manifest type and SouthAfrica country", container1.PK, pack.ContainerPK);
			var container2 = header.Containers.AddNew();
			pack = bill.Packs.AddNew();
			AssertEquals("Pack should not have a container", ZGuid.Empty, pack.ContainerPK);
			header.Containers.Remove(container2);
			header.AMA_ManifestType = "FFM";
			pack = bill.Packs.AddNew();
			AssertEquals("Pack should not have a container", ZGuid.Empty, pack.ContainerPK);
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			pack = bill.Packs.AddNew();
			AssertEquals("Pack should auto select container when there is only one and is containerised", container1.PK, pack.ContainerPK);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaPackCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs;
		}
	}
}
