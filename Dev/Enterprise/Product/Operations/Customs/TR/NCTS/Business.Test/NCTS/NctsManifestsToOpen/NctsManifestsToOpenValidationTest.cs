using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsManifestsToOpenValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_LineNo()
		{
			CombineAssertions(() =>
			{
				nctsManifestsToOpen.IsPartial = true;
				nctsManifestsToOpen.CSI_LineNo = ZShort.Zero;
				AssertHasMessageErrorContaining("Partial with Zero", nctsManifestsToOpen.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeZero);

				nctsManifestsToOpen.CSI_LineNo = 1;
				AssertNoMessageErrorContaining("Partial with positive value", nctsManifestsToOpen.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeZero);

				nctsManifestsToOpen.IsPartial = false;
				nctsManifestsToOpen.CSI_LineNo = ZShort.Zero;
				AssertNoMessageErrorContaining("Not Partial with Zero", nctsManifestsToOpen.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsManifestsToOpen.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_CustomsOffice_Mandatory()
		{
			nctsManifestsToOpen.AtWarehouse = true;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsManifestsToOpen.CSI_CustomsOfficeInfo);
			nctsManifestsToOpen.AtWarehouse = false;
			nctsManifestsToOpen.CSI_CustomsOffice = ZString.Empty;
			AssertNoMessageErrorContaining("Not At Warehouse", nctsManifestsToOpen.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_CustomsOffice_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "Turkey Warehouse Codes");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "A0002", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();

			nctsManifestsToOpen.AtWarehouse = true;
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsManifestsToOpen.CSI_CustomsOfficeInfo, "X0001", "A0002");
		}

		public void TestCheckCSI_Quantity3_ValueCannotBeZero()
		{
			nctsManifestsToOpen.IsPartial = true;
			nctsManifestsToOpen.CSI_Quantity3 = ZDecimal.Zero;
			AssertHasMessageErrorContaining("Partial With Zero", nctsManifestsToOpen.CSI_Quantity3Info, MandatoryValidation.ValueCannotBeZero);

			nctsManifestsToOpen.CSI_Quantity3 = 2.34m;
			AssertNoMessageErrorContaining("Partial with positive", nctsManifestsToOpen.CSI_Quantity3Info, MandatoryValidation.ValueCannotBeZero);

			nctsManifestsToOpen.IsPartial = false;
			nctsManifestsToOpen.CSI_Quantity3 = ZDecimal.Zero;
			AssertNoMessageErrorContaining("Not Partial With Zero", nctsManifestsToOpen.CSI_Quantity3Info, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckCSI_Quantity3_ValueCannotBeNegative()
		{
			nctsManifestsToOpen.IsPartial = true;
			nctsManifestsToOpen.CSI_Quantity3 = -1m;
			AssertHasErrorContaining("Negative", nctsManifestsToOpen.CSI_Quantity3Info, MandatoryValidation.ValueCannotBeNegative);

			nctsManifestsToOpen.CSI_Quantity3 = 1.25m;
			AssertNoErrorContaining("Positive", nctsManifestsToOpen.CSI_Quantity3Info, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckCSI_Quantity3_ValueMustBeInteger()
		{
			nctsManifestsToOpen.IsPartial = true;
			nctsManifestsToOpen.CSI_Quantity3 = 2147483648m;
			AssertHasErrorContaining(nctsManifestsToOpen.CSI_Quantity3Info, "The maximum quantity should be 2147483647.");

			nctsManifestsToOpen.CSI_Quantity3 = 2147483647m;
			AssertNoErrorContaining(nctsManifestsToOpen.CSI_Quantity3Info, "The maximum quantity should be 2147483647.");
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsManifestsToOpen.CSI_ReferenceNumber2Info);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsManifestsToOpen = nctsHeader.ManifestsToOpenList.AddNew();
		}
		NctsHeader nctsHeader;
		NctsManifestsToOpen nctsManifestsToOpen;
	}
}
