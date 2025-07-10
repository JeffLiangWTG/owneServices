using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USNHTSADocumentAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_NHTDocumentType()
		{
			Document.AddInfo.Validation.ValidateUS_NHTDocumentType();
			AssertHasMessageErrorContaining(Document.US_NHTDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			Document.US_NHTDocumentType = "XXX";
			AssertHasMessageErrorContaining(Document.US_NHTDocumentTypeInfo, ListValidation.InvalidCodeMessageError);

			Document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;
			AssertHasMessageErrorContaining(Document.US_NHTDocumentTypeInfo, ValidationConstants.NHTSA.DuplicateDocumentWithSameType(NHTSADocumentTypeList.Codes._946));
		}

		public void TestCheckUS_NHTDocumentOwner()
		{
			Document.AddInfo.Validation.ValidateUS_NHTDocumentOwner();
			AssertHasMessageErrorContaining(Document.US_NHTDocumentOwnerInfo, MandatoryValidation.YouHaveNotEntered);

			Document.US_NHTDocumentOwner = "XX";
			AssertHasMessageErrorContaining(Document.US_NHTDocumentOwnerInfo, ListValidation.InvalidCodeMessageError);

			Document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.CertifyingIndividual;
			AssertNoMessageErrors(Document.US_NHTDocumentOwnerInfo);
		}

		#region Implementation

		NHTSAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					header = invoiceLine.NHTSALines.AddNew();
				}

				return header;
			}
		}
		NHTSAHeader header;

		NHTSADocument Document
		{
			get { return document ?? (document = Header.NHTSADocuments.AddNew()); }
		}
		NHTSADocument document;

		#endregion
	}
}
