using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var cont = header.Containers.AddNew();
			AssertEquals(header, cont.Header);
		}

		public void TestACN_ContainerNumber_MaxLength()
		{
			var cont = Factory.New<AsycudaContainer>();
			AssertHasCustomAttribute<MaxLengthAttribute>(cont.GetType(), nameof(cont.ACN_ContainerNumber), false, attr => attr.MaxLength == 17);
		}

		public void TestACN_Seal1_MaxLength()
		{
			var cont = Factory.New<AsycudaContainer>();
			AssertHasCustomAttribute<MaxLengthAttribute>(cont.GetType(), nameof(cont.ACN_Seal1), false, attr => attr.MaxLength == 17);
		}

		public void TestACN_Seal2_MaxLength()
		{
			var cont = Factory.New<AsycudaContainer>();
			AssertHasCustomAttribute<MaxLengthAttribute>(cont.GetType(), nameof(cont.ACN_Seal2), false, attr => attr.MaxLength == 17);
		}

		public void TestACN_Seal3_MaxLength()
		{
			var cont = Factory.New<AsycudaContainer>();
			AssertHasCustomAttribute<MaxLengthAttribute>(cont.GetType(), nameof(cont.ACN_Seal3), false, attr => attr.MaxLength == 17);
		}

		public void TestACN_Seal1_Caption()
		{
			var cont = Factory.New<AsycudaContainer>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cont.ACN_Seal1Info);
			AssertEquals("Caption", "Seal 1", resourceStringData.Caption);
		}

		public void TestACN_Seal2_Caption()
		{
			var cont = Factory.New<AsycudaContainer>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cont.ACN_Seal2Info);
			AssertEquals("Caption", "Seal 2", resourceStringData.Caption);
		}

		public void TestACN_Seal3_Caption()
		{
			var cont = Factory.New<AsycudaContainer>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cont.ACN_Seal3Info);
			AssertEquals("Caption", "Seal 3", resourceStringData.Caption);
		}

		public void TestACN_EmptyFullIndicator_Caption()
		{
			var cont = Factory.New<AsycudaContainer>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cont.ACN_EmptyFullIndicatorInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Container Mode", resourceStringData.Caption);
				AssertEquals("Short Caption", "Cont. Mode", resourceStringData.ShortCaption);
			});
		}

		public void TestValidationType()
		{
			var cont = Factory.New<AsycudaContainer>();
			AssertType<AsycudaContainerValidation>(cont.Validation);
		}

		public void TestLookupsType()
		{
			var cont = Factory.New<AsycudaContainer>();
			AssertType<AsycudaContainerLookups>(cont.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var constainer = header.Containers.AddNew();
			constainer.ACN_ContainerNumber = "Test";
			return constainer;
		}
	}
}
