using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSubTypeEmpty()
		{
			nctsPreviousDocument.CSI_SubType = ZString.Empty;
			AssertHasMessageError(nctsPreviousDocument.CSI_SubTypeInfo, "You have not entered a Payment Type.");
		}

		public void TestCheckIncoterm_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPreviousDocument.IncotermInfo);
		}

		public void TestCheckIncoterm_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsPreviousDocument.IncotermInfo, "XXX", "CPT");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPreviousDocument.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_UnitOfQuantity2_Mandatory()
		{
			nctsPreviousDocument.CSI_Quantity2 = 1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPreviousDocument.CSI_UnitOfQuantity2Info);
			nctsPreviousDocument.CSI_Quantity2 = ZDecimal.Zero;
			nctsPreviousDocument.CSI_UnitOfQuantity2 = ZString.Empty;
			AssertNoMessageErrorContaining("Quantity is Zero", nctsPreviousDocument.CSI_UnitOfQuantity2Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_UnitOfQuantity2_List()
		{
			nctsPreviousDocument.CSI_Quantity2 = 55;
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsPreviousDocument.CSI_UnitOfQuantity2Info, "XX", "KG");
		}

		public void TestCheckCSI_UnitOfQuantity3_Mandatory()
		{
			nctsPreviousDocument.CSI_Quantity3 = 1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPreviousDocument.CSI_UnitOfQuantity3Info);
			nctsPreviousDocument.CSI_Quantity3 = ZDecimal.Zero;
			nctsPreviousDocument.CSI_UnitOfQuantity3 = ZString.Empty;
			AssertNoMessageErrorContaining("Quantity is Zero", nctsPreviousDocument.CSI_UnitOfQuantity3Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_UnitOfQuantity3_List()
		{
			nctsPreviousDocument.CSI_Quantity3 = 42;
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsPreviousDocument.CSI_UnitOfQuantity3Info, "XX", "KG");
		}

		public void TestCheckCSI_Procedure_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPreviousDocument.CSI_ProcedureInfo);
		}

		public void TestCheckCSI_Procedure_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "Nature Of Business");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "11", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsPreviousDocument.CSI_ProcedureInfo, "21", "11");
		}

		public void TestCheckCSI_RN_NKCountryCode_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPreviousDocument.CSI_RN_NKCountryCodeInfo);
		}

		public void TestCheckCSI_RN_NKCountryCode_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsPreviousDocument.CSI_RN_NKCountryCodeInfo, "XX", Core.Constants.CountryCodes.Turkey);
		}

		public void TestCheckCSI_RX_NKCurrency_Mandatory()
		{
			nctsPreviousDocument.CSI_Value = 55;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsPreviousDocument.CSI_RX_NKCurrencyInfo);
			nctsPreviousDocument.CSI_Value = ZDecimal.Zero;
			nctsPreviousDocument.CSI_RX_NKCurrency = ZString.Empty;
			AssertNoMessageErrorContaining("Value is Zero", nctsPreviousDocument.CSI_RX_NKCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_RX_NKCurrency_List()
		{
			nctsPreviousDocument.CSI_Value = 55;
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsPreviousDocument.CSI_RX_NKCurrencyInfo, "XXX", Core.Constants.CurrencyCodes.Turkey);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var detail = nctsHeader.MovementHeader.GoodsItems.AddNew();
			nctsPreviousDocument = detail.PreviousDocuments.AddNew();
		}
		NctsPreviousDocument nctsPreviousDocument;
	}
}
