using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class AsycudaBillLinkAsycudaContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainerTypes()
		{
			var containerTypes = lookups.ContainerTypes;
			AssertEquals(RefContainerLookups.ShippingModes.Sea, containerTypes.FilterBusinessObjectDefaults["Transport Mode" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			var newContainer = containerTypes.AddNew();
			AssertEquals(RefContainerLookups.ShippingModes.Sea, newContainer.RC_ShippingMode);
		}

		public void TestEmptyFullList()
		{
			var emptyFullList = lookups.EmptyFullList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "0, 1, 2, 3, 4, 5, 6", emptyFullList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<EmptyFullList>(), emptyFullList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var asycudaBillLinkAsycudaContainer = bill.AsycudaBillLinkAsycudaContainers.Cast<AsycudaBillLinkAsycudaContainer>().First();
			lookups = asycudaBillLinkAsycudaContainer.Lookups;
		}

		AsycudaBillLinkAsycudaContainerLookups lookups;
	}
}
