using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USFSISLineAddInfoValidationHelperTest : TestCaseWithFactory
	{
		public void TestCheckEstablishNumbers()
		{
			fSISLine.US_ImportingEstNo = "AB1-CDE";
			AssertHasMessageError(fSISLine.US_ImportingEstNoInfo, USFSISLineAddInfoValidationHelper.EstablishmentNumbersOnlyContainNumbersAndLetters);

			fSISLine.US_ExportingEstNo = "AB1-CDE";
			AssertHasMessageError(fSISLine.US_ExportingEstNoInfo, USFSISLineAddInfoValidationHelper.EstablishmentNumbersOnlyContainNumbersAndLetters);
			fSISLine.US_ImportingEstNo = "AB1CDE";
			AssertNoMessageError(fSISLine.US_ImportingEstNoInfo, USFSISLineAddInfoValidationHelper.EstablishmentNumbersOnlyContainNumbersAndLetters);

			lot.US_ProducingEstNo = "AB!-CDE";
			AssertHasMessageError(lot.US_ProducingEstNoInfo, USFSISLineAddInfoValidationHelper.EstablishmentNumbersOnlyContainNumbersAndLetters);
			lot.US_SourceEstNo = "AB!-CDE";
			AssertHasMessageError(lot.US_SourceEstNoInfo, USFSISLineAddInfoValidationHelper.EstablishmentNumbersOnlyContainNumbersAndLetters);

			lot.US_SourceEstNo = "AB12";
			lot.US_ProducingEstNo = "7788";
			AssertNoMessageError(lot.US_SourceEstNoInfo, USFSISLineAddInfoValidationHelper.EstablishmentNumbersOnlyContainNumbersAndLetters);
			AssertNoMessageError(lot.US_ProducingEstNoInfo, USFSISLineAddInfoValidationHelper.EstablishmentNumbersOnlyContainNumbersAndLetters);
		}

		USFSISLot lot;
		USInvoiceLineFSISLine fSISLine;
		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			fSISLine = invoiceLine.FSISLines.AddNew();

			fSISLine.US_HealthCertificateNumber = "WERWER";
			fSISLine.US_CommercialDescription = "ERWERWER";
			fSISLine.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.France;
			fSISLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.France;
			fSISLine.US_ProductID = "34234";
			fSISLine.US_IntendedUseCode = IntendedUseCodesList.Codes.ForConsumerUseHumanFood;
			fSISLine.US_DateOfInspection = new ZDateTime(2016, 4, 7);

			lot = fSISLine.Lots.AddNew();
			lot.US_LotNumber = "123";
			lot.US_ShippingMarks = "123 123";
			lot.US_NoOfUnit1 = 44;
		}
	}
}
