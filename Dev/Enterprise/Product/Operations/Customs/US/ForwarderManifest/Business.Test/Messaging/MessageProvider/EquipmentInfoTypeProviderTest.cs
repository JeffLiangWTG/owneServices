using System.Linq;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class EquipmentInfoTypeProviderTest : TestCaseWithFactory
	{
		public void TestEquipmentType()
		{
			AssertEquals("FR", provider.EquipmentTypeCode.Value);
		}

		public void TestEquipmentNumber()
		{
			container.ACN_ContainerNumber = "12";

			AssertEquals("12", provider.EquipmentNumber.Value);
		}

		public void TestEquipmentLength()
		{
			AssertEquals("20", provider.EquipmentLength.Value);
		}

		public void TestEquipmentHeight()
		{
			AssertEquals("8", provider.EquipmentHeight.Value);
		}

		public void TestEquipmentWidth()
		{
			AssertEquals("8", provider.EquipmentWidth.Value);
		}

		public void TestEquipmentSizeTypeCode()
		{
			AssertEquals("22P1", provider.EquipmentSizeTypeCode.Value);
		}

		public void TestLoadedEmptyStatus()
		{
			container.ACN_EmptyFullIndicator = "";

			AssertEquals("E", provider.LoadedEmptyStatus.Value);

			container.ACN_EmptyFullIndicator = "LCL";

			AssertEquals("L", provider.LoadedEmptyStatus.Value);

			container.ACN_EmptyFullIndicator = "MT";

			AssertEquals("E", provider.LoadedEmptyStatus.Value);
		}

		public void TestSeals()
		{
			container.ACN_Seal1 = "1";
			container.ACN_Seal2 = "2";
			container.ACN_Seal3 = "3";

			AssertEquals(3, provider.SealInfoList.Count);
			Assert(provider.SealInfoList.Any(s => s.SealNumber.Value == "1"));
			Assert(provider.SealInfoList.Any(s => s.SealNumber.Value == "2"));
			Assert(provider.SealInfoList.Any(s => s.SealNumber.Value == "3"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();

			container = manifestHeader.Containers.AddNew();

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			container.ACN_RC_ContainerType = refContainer.PK;

			provider = new EquipmentInfoTypeProvider(container, null);
		}
		IEquipmentInfoType provider;
		AsycudaContainer container;
	}
}
