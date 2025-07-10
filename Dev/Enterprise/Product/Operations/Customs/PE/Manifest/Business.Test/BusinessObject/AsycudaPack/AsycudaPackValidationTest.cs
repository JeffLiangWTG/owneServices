using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	sealed class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPA_PackUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Peru);
			var bbk = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Peru, RefCusCodeListTypes.Codes.PackageTypes, Core.Constants.PkgUnit.BreakBulk, Core.Constants.PkgUnit.BreakBulk, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(bbk.PK, "123", "123");
			Factory.Save();

			AssertNoNotifications(pack.APA_PackUQInfo);

			pack.APA_PackUQ = "BAG";
			AssertHasMessageErrorContaining(pack.APA_PackUQInfo, "Package type BAG does not map to a Customs package type for country PE. Please add a mapping via Maintain > Customs > Customs Files > Packs Conversion.");

			pack.APA_PackQty = 5;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(pack.APA_PackUQInfo, "XX", "BBK");

			pack.APA_PackQty = 0;
			pack.APA_PackUQ = string.Empty;
			AssertNoMessageErrorContaining(pack.APA_PackUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestAPA_WeightUQ()
		{
			pack.APA_Weight = 10;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(pack.APA_WeightUQInfo, "XX", Core.Constants.Weight.Pounds);

			pack.APA_Weight = 0;
			pack.APA_WeightUQ = string.Empty;
			AssertNoMessageError(pack.APA_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestAPA_VolumeUQ()
		{
			pack.APA_Volume = 10;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(pack.APA_VolumeUQInfo, "XX", Core.Constants.Volume.CubicFeet);

			pack.APA_Volume = 0;
			pack.APA_VolumeUQ = string.Empty;
			AssertNoMessageError(pack.APA_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPA_GoodsDescription()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(pack.APA_GoodsDescriptionInfo);
		}

		public void TestCheckAPA_Volume()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(pack.APA_VolumeInfo);
		}

		public void TestCheckAPA_Weight()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(pack.APA_WeightInfo);
		}

		public void TestCheckAPA_PackQty()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(pack.APA_PackQtyInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			pack = bill.Packs.AddNew();
		}

		public void TestCheckLinePrice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.LinePrice = ZDecimal.Zero;
			AssertHasMessageErrorContaining(pack.LinePriceInfo, MandatoryValidation.YouHaveNotEntered);

			pack.LinePrice = 10m;
			AssertNoMessageErrorContaining(pack.LinePriceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		AsycudaPack pack;
	}
}
