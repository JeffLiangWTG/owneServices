using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class AsycudaBillLinkAsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var asycudaBillLinkAsycudaContainer = bill.AsycudaBillLinkAsycudaContainers.Cast<AsycudaBillLinkAsycudaContainer>().First();
			AssertType<AsycudaBillLinkAsycudaContainerValidation>(asycudaBillLinkAsycudaContainer.Validation);
		}
	}
}
