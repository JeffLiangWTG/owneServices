using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestSupportingDocSendingObjectCollection))]
	class SupportingDocSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ManifestSupportingDocSendingObjectCollection>
	{
		public void TestAllowNew()
		{
			Assert(GetCollectionToTest().AllowNew);
		}

		protected override ManifestSupportingDocSendingObjectCollection GetCollectionToTest()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new ManifestSupportingDocSendingObjectCollection(manifest);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return SupportingDocSendingObject.New(manifest);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}

		protected AsycudaManifestHeader manifest;
	}
}
