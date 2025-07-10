using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USHFCHeaderAddInfoValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckUS_ASHRAENumber()
		{
			hfcHeader.AddInfoValidation.ValidateUS_ASHRAENumber();
			AssertHasMessageErrorContaining(hfcHeader.US_ASHRAENumberInfo, MandatoryValidation.YouHaveNotEntered);
			hfcHeader.USHFCDetails.AddNew();
			hfcHeader.AddInfoValidation.ValidateUS_ASHRAENumber();
			AssertNoMessageErrorContaining(hfcHeader.US_ASHRAENumberInfo, MandatoryValidation.YouHaveNotEntered);
			hfcHeader.US_ASHRAENumber = "1";
			AssertHasWarningContaining(hfcHeader.US_ASHRAENumberInfo, USHFCHeaderAddInfoValidation.ASHRAENumberFormat);
			hfcHeader.US_ASHRAENumber = "R-1A";
			AssertNoWarningContaining(hfcHeader.US_ASHRAENumberInfo, USHFCHeaderAddInfoValidation.ASHRAENumberFormat);
		}

		public void TestCheckUS_NetWeight()
		{
			hfcHeader.AddInfoValidation.ValidateUS_NetWeight();
			AssertHasMessageErrorContaining(hfcHeader.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
			hfcHeader.US_NetWeight = -1;
			AssertHasErrorContaining(hfcHeader.US_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);
			hfcHeader.US_NetWeight = 1;
			AssertNoMessageErrorContaining(hfcHeader.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CertifyingIndividual()
		{
			hfcHeader.US_CertifyingIndividual = "~";
			AssertHasMessageError(hfcHeader.US_CertifyingIndividualInfo, ListValidation.InvalidCodeMessageError);
			hfcHeader.US_CertifyingIndividual = EntityRoleCodeList.Codes.Importer;
			AssertNoMessageError(hfcHeader.US_CertifyingIndividualInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_HFCImageSent()
		{
			hfcHeader.AddInfoValidation.ValidateUS_HFCImageSent();
			AssertHasMessageError(hfcHeader.US_HFCImageSentInfo, USHFCHeaderAddInfoValidation.LabelRequired);
			hfcHeader.US_HFCImageSent = true;
			AssertNoMessageError(hfcHeader.US_HFCImageSentInfo, USHFCHeaderAddInfoValidation.LabelRequired);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			hfcHeader = invoiceLine.USHFCHeaders.AddNew();
		}
		JobComInvoiceLine invoiceLine;
		USHFCHeader hfcHeader;
	}
}
