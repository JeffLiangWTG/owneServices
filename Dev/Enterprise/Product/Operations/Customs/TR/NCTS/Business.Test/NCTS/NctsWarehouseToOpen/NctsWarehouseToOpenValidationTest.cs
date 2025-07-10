using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsWarehouseToOpenValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ItemNumber()
		{
			nctsWarehouseToOpen.CSI_ItemNumber = ZInt.Zero;
			CombineAssertions(() =>
			{
				TestCaseWithFactory.AssertHasMessageErrors(nctsWarehouseToOpen.CSI_ItemNumberInfo);
				nctsWarehouseToOpen.CSI_ItemNumber = 532323;
				AssertNoMessageErrors("Value is Valid", nctsWarehouseToOpen.CSI_ItemNumberInfo);
			});
		}

		public void TestCSI_SubType_Mandatory()
		{
			nctsWarehouseToOpen.CSI_SubType = ZString.Empty;
			AssertHasMessageError(nctsWarehouseToOpen.CSI_SubTypeInfo, "You have not entered a Payment Type.");
		}

		public void TestCheckCSI_SubType_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsWarehouseToOpen.CSI_SubTypeInfo, "00", "14");
		}

		public void TestCheckIncoterm_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsWarehouseToOpen.IncotermInfo);
		}

		public void TestCheckIncoterm_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsWarehouseToOpen.IncotermInfo, "XXX", "CPT");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsWarehouseToOpen.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_UnitOfQuantity_Mandatory()
		{
			nctsWarehouseToOpen.CSI_Quantity = 1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsWarehouseToOpen.CSI_UnitOfQuantityInfo);
			nctsWarehouseToOpen.CSI_Quantity2 = ZDecimal.Zero;
			nctsWarehouseToOpen.CSI_UnitOfQuantity2 = ZString.Empty;
			AssertNoMessageErrorContaining("Quantity is Zero", nctsWarehouseToOpen.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_UnitOfQuantity_List()
		{
			nctsWarehouseToOpen.CSI_Quantity = 55;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Unit of Quantity");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "AYR", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsWarehouseToOpen.CSI_UnitOfQuantityInfo, "XXX", "AYR");
		}

		public void TestCheckCSI_Procedure_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsWarehouseToOpen.CSI_ProcedureInfo);
		}

		public void TestCheckCSI_Procedure_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "Nature Of Business");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "11", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsWarehouseToOpen.CSI_ProcedureInfo, "20", "11");
		}

		public void TestCheckCSI_RN_NKCountryCode_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsWarehouseToOpen.CSI_RN_NKCountryCodeInfo);
		}

		public void TestCheckCSI_RN_NKCountryCode_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsWarehouseToOpen.CSI_RN_NKCountryCodeInfo, "XX", Core.Constants.CountryCodes.Turkey);
		}

		public void TestCheckCSI_RX_NKCurrency_Mandatory()
		{
			nctsWarehouseToOpen.CSI_Value = 55;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsWarehouseToOpen.CSI_RX_NKCurrencyInfo);
			nctsWarehouseToOpen.CSI_Value = ZDecimal.Zero;
			nctsWarehouseToOpen.CSI_RX_NKCurrency = ZString.Empty;
			AssertNoMessageErrorContaining("Value is Zero", nctsWarehouseToOpen.CSI_RX_NKCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_RX_NKCurrency_List()
		{
			nctsWarehouseToOpen.CSI_Value = 55;
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsWarehouseToOpen.CSI_RX_NKCurrencyInfo, "XXX", Core.Constants.CurrencyCodes.Turkey);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsWarehouseToOpen = nctsHeader.MovementHeader.WarehouseToOpenList.AddNew();
		}
		NctsWarehouseToOpen nctsWarehouseToOpen;
	}
}
