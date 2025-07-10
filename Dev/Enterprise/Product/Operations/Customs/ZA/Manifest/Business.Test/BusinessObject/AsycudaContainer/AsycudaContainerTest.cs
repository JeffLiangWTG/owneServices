using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaContainer()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaContainer>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaContainer>(bizObj.PK).GetType());
		}

		public void TestLandedPurpose()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			container.LandedPurpose = "AR1";
			AssertEquals("AR1", container.GetSystemDefinedValue<ZString>(AsycudaContainer.Schema.LandedPurpose));
		}

		public void TestLandedPurposeOnAddNewContainer()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = NatureList.Codes.Transit24;
			var container = header.Containers.AddNew();
			AssertEquals(LandedPurposeList.Codes.ContinentalTransit, container.GetSystemDefinedValue<ZString>(AsycudaContainer.Schema.LandedPurpose));

			header.AMA_Nature = ZaShipmentTypeList.Codes.MutualMultipleZzz;
			var container1 = header.Containers.AddNew();
			AssertEquals(ZString.Empty, container1.GetSystemDefinedValue<ZString>(AsycudaContainer.Schema.LandedPurpose));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Containers.AddNew();
		}
	}
}
