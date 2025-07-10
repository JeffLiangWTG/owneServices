using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.VN.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestCorrectTypeDeciderForAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertType<AsycudaBill>("Load using Integration.Customs.ASYCUDA.VNManifest.IAsycudaBill", new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.VNManifest.IAsycudaBill>(bizObj.PK));
			AssertType<AsycudaBill>("Load using ASYCUDA.Business.AsycudaBill", new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK));
			AssertType<AsycudaBill>("Load using ManifestBase.AsycudaBill", new BusinessObjectFactory().Load<ManifestBase.AsycudaBill>(bizObj.PK));
		}

		public void TestCountryCode()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals("CountryCode", Core.Constants.CountryCodes.VietNam, bill.CountryCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}
	}
}
