using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USFSISLotAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Species()
		{
			lot.US_Species = "XX";
			lot.US_Species = ZString.Empty;
			AssertHasMessageErrorContaining(lot.US_SpeciesInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_Species = "XX";
			AssertNoMessageErrorContaining(lot.US_SpeciesInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(lot.US_SpeciesInfo, ListValidation.InvalidCodeMessageError);
			lot.US_Species = FSISProductSpeciesNameList.Codes.GoatMeat;
			AssertNoMessageErrorContaining(lot.US_SpeciesInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_ProductQualifierCode()
		{
			lot.US_ProductQualifierCode = "XX";
			lot.US_ProductQualifierCode = ZString.Empty;
			AssertHasMessageErrorContaining(lot.US_ProductQualifierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_ProductQualifierCode = "XX";
			AssertNoMessageErrorContaining(lot.US_ProductQualifierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(lot.US_ProductQualifierCodeInfo, ListValidation.InvalidCodeMessageError);
			lot.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.NFC;
			AssertNoMessageErrorContaining(lot.US_ProductQualifierCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_ProductCharacteristicQualifier()
		{
			lot.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.EEP;
			lot.US_ProductCharacteristicQualifier = "XX";
			lot.US_ProductCharacteristicQualifier = ZString.Empty;
			AssertHasMessageErrorContaining(lot.US_ProductCharacteristicQualifierInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_ProductCharacteristicQualifier = "XX";
			AssertNoMessageErrorContaining(lot.US_ProductCharacteristicQualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(lot.US_ProductCharacteristicQualifierInfo, ListValidation.InvalidCodeMessageError);
			lot.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._2B;
			AssertNoMessageErrorContaining(lot.US_ProductCharacteristicQualifierInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_NoOfUnit2()
		{
			lot.US_UQ2 = ShippingOrPackingingUnitList.Codes.Bag;
			lot.US_NoOfUnit2 = 1;
			lot.US_NoOfUnit2 = ZInt.Zero;
			AssertHasMessageError(lot.US_NoOfUnit2Info, string.Format(USFSISLotAddInfoValidation.InnermostUQIsMissing, "Innermost Package", "UQ"));
			lot.US_NoOfUnit2 = 2;
			AssertNoMessageError(lot.US_NoOfUnit2Info, string.Format(USFSISLotAddInfoValidation.InnermostUQIsMissing, "Innermost Package", "UQ"));
		}

		public void TestCheckUS_LotNumber()
		{
			lot.Parent.Lots.AddNew().US_LotNumber = "2";
			lot.US_LotNumber = "2";
			lot.US_LotNumber = ZString.Empty;
			AssertHasMessageErrorContaining(lot.US_LotNumberInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_LotNumber = "2";
			AssertNoMessageErrorContaining(lot.US_LotNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.RepeatedLotNoMessage);
			lot.US_LotNumber = "1";
			AssertNoMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.RepeatedLotNoMessage);
		}

		public void TestCheckUS_LotNumberOnlyNumberAndLessThenThreeChar()
		{
			lot.US_LotNumber = "1A";
			AssertHasMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = "1";
			AssertNoMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = "12A";
			AssertHasMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = "12";
			AssertNoMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = " 12";
			AssertNoMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = "1 2";
			AssertHasMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = "123";
			AssertNoMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = "ABC";
			AssertHasMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = "001";
			AssertNoMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = "01 ";
			AssertNoMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
			lot.US_LotNumber = " 1 ";
			AssertNoMessageError(lot.US_LotNumberInfo, USFSISLotAddInfoValidation.OnlyNumberAndThreeDigsLotNoMessage);
		}

		public void TestCheckUS_NoOfUnit1()
		{
			lot.US_NoOfUnit1 = 1;
			lot.US_NoOfUnit1 = ZInt.Zero;
			AssertHasMessageErrorContaining(lot.US_NoOfUnit1Info, MandatoryValidation.YouHaveNotEntered);
			lot.US_NoOfUnit1 = 1;
			AssertNoMessageErrorContaining(lot.US_NoOfUnit1Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NetWeight()
		{
			lot.US_NetWeight = 1m;
			lot.US_NetWeight = ZDecimal.Zero;
			AssertHasMessageErrorContaining(lot.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_NetWeight = 1m;
			AssertNoMessageErrorContaining(lot.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ShippingMarks()
		{
			lot.US_ShippingMarks = "XX";
			lot.US_ShippingMarks = ZString.Empty;
			AssertHasMessageErrorContaining(lot.US_ShippingMarksInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_ShippingMarks = "XX";
			AssertNoMessageErrorContaining(lot.US_ShippingMarksInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_StartDate()
		{
			lot.US_StartDate = ZDateTime.Today;
			lot.US_StartDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(lot.US_StartDateInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_StartDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(lot.US_StartDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_EndDate()
		{
			lot.US_EndDate = ZDateTime.Today;
			lot.US_EndDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(lot.US_EndDateInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_EndDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(lot.US_EndDateInfo, MandatoryValidation.YouHaveNotEntered);
			lot.US_StartDate = ZDateTime.Today;
			lot.US_EndDate = ZDateTime.Today.AddDays(-1);
			AssertHasMessageError(lot.US_EndDateInfo, USFSISLotAddInfoValidation.EndDateBeforeStartDate);
			lot.US_EndDate = ZDateTime.Today.AddDays(1);
			AssertNoMessageError(lot.US_EndDateInfo, USFSISLotAddInfoValidation.EndDateBeforeStartDate);
		}

		public void TestCheckUS_UQ1()
		{
			lot.US_UQ1 = "XX";
			lot.US_UQ1 = ZString.Empty;
			AssertHasMessageErrorContaining(lot.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
			lot.US_UQ1 = "XX";
			AssertNoMessageErrorContaining(lot.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(lot.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			lot.US_UQ1 = ShippingOrPackingingUnitList.Codes.Cover;
			AssertNoMessageErrorContaining(lot.US_UQ1Info, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_UQ2()
		{
			lot.US_NoOfUnit2 = 4;
			lot.US_UQ2 = "XX";
			lot.US_UQ2 = ZString.Empty;
			AssertHasMessageError(lot.US_UQ2Info, string.Format(USFSISLotAddInfoValidation.InnermostUQIsMissing, "UQ", "Innermost Package"));
			lot.US_UQ2 = "XX";
			AssertNoMessageError(lot.US_UQ2Info, string.Format(USFSISLotAddInfoValidation.InnermostUQIsMissing, "UQ", "Innermost Package"));
			AssertHasMessageErrorContaining(lot.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			lot.US_UQ2 = ShippingOrPackingingUnitList.Codes.Cover;
			AssertNoMessageError(lot.US_UQ2Info, string.Format(USFSISLotAddInfoValidation.InnermostUQIsMissing, "UQ", "Innermost Package"));
		}

		USFSISLot lot;
		protected override void SetUp()
		{
			base.SetUp();
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew();
			line.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Mexico;
			lot = line.Lots.AddNew();
		}
	}
}
