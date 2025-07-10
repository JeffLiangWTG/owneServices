using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	sealed class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACN_GoodsWeight()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			container.ACN_GoodsWeight = 100;
			AssertNoMessageErrorContaining(container.ACN_GoodsWeightInfo, "cannot be zero");
			container.ACN_GoodsWeight = 0;
			AssertHasMessageErrorContaining(container.ACN_GoodsWeightInfo, "cannot be zero");
		}

		public void TestValidateSealType1()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			container.ACN_Seal1 = ZString.Empty;
			container.Validation.ValidateACN_SealType1();
			AssertNoMessageErrors(container.ACN_SealType1Info);
			container.ACN_Seal1 = "1";
			container.Validation.ValidateACN_SealType1();
			AssertHasMessageErrorContaining(container.ACN_SealType1Info, "");
			container.ACN_Seal1 = ZA.Business.MessageBuilders.COSTCO.Contants.SealNumber.NoSealNo;
			container.Validation.ValidateACN_SealType1();
			AssertNoMessageErrors(container.ACN_SealType1Info);
		}

		public void TestValidateSealType2()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			container.ACN_Seal2 = ZString.Empty;
			container.Validation.ValidateACN_SealType2();
			AssertNoMessageErrors(container.ACN_SealType2Info);
			container.ACN_Seal2 = "2";
			container.Validation.ValidateACN_SealType2();
			AssertHasMessageErrorContaining(container.ACN_SealType2Info, "");
			container.ACN_Seal2 = ZA.Business.MessageBuilders.COSTCO.Contants.SealNumber.NoSealNo;
			container.Validation.ValidateACN_SealType2();
			AssertNoMessageErrors(container.ACN_SealType2Info);
		}

		public void TestValidateSealType3()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			container.ACN_Seal3 = ZString.Empty;
			container.Validation.ValidateACN_SealType3();
			AssertNoMessageErrors(container.ACN_SealType3Info);
			container.ACN_Seal3 = "3";
			container.Validation.ValidateACN_SealType3();
			AssertHasMessageErrorContaining(container.ACN_SealType3Info, "");
			container.ACN_Seal3 = ZA.Business.MessageBuilders.COSTCO.Contants.SealNumber.NoSealNo;
			container.Validation.ValidateACN_SealType3();
			AssertNoMessageErrors(container.ACN_SealType3Info);
		}

		public void TestLandedPurpose()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			container.LandedPurpose = "ABC";
			AssertHasMessageErrorContaining(container.LandedPurposeInfo, ListValidation.InvalidCodeMessageError);

			container.LandedPurpose = "2";
			AssertNoNotifications(container.LandedPurposeInfo);

			container.LandedPurpose = ZString.Empty;
			AssertHasMessageErrorContaining(container.LandedPurposeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
