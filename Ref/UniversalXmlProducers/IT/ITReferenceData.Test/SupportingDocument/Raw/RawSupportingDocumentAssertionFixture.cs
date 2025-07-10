using System;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	static class RawSupportingDocumentAssertionFixture
	{
		public static void AssertRawSupportingDocument(IRawSupportingDocument rawSupportingDocument, bool expectedIsCertificateIdRequired, string expectedCode, bool expectedIsCountryRequired, string expectedDescription, string expectedElectronicFolderNote
			, bool expectedIsElectronicFolderRequired, DateTime expectedEndDate, string expectedPaperFolderNote, bool expectedIsPaperFolderRequired, bool expectedIsQuantityRequired, bool expectedIsRetroActiveRequired, DateTime expectedStartDate
			, bool expectedIsUnitOfQuantityRequired, bool expectedIsYearRequired, SupportingDocumentType expectedSupportingDocumentType)
		{
			Assert.Multiple(() =>
			{
				Assert.AreEqual(expectedIsCertificateIdRequired, rawSupportingDocument.IsCertificateIdRequired, nameof(rawSupportingDocument.IsCertificateIdRequired));
				Assert.AreEqual(expectedCode, rawSupportingDocument.Code, nameof(rawSupportingDocument.Code));
				Assert.AreEqual(expectedIsCountryRequired, rawSupportingDocument.IsCountryRequired, nameof(rawSupportingDocument.IsCountryRequired));
				Assert.AreEqual(expectedDescription, rawSupportingDocument.Description, nameof(rawSupportingDocument.Description));
				Assert.AreEqual(expectedElectronicFolderNote, rawSupportingDocument.ElectronicFolderNote, nameof(rawSupportingDocument.ElectronicFolderNote));
				Assert.AreEqual(expectedIsElectronicFolderRequired, rawSupportingDocument.IsElectronicFolderRequired, nameof(rawSupportingDocument.IsElectronicFolderRequired));
				Assert.AreEqual(expectedEndDate, rawSupportingDocument.EndDate, nameof(rawSupportingDocument.EndDate));
				Assert.AreEqual(expectedPaperFolderNote, rawSupportingDocument.PaperFolderNote, nameof(rawSupportingDocument.PaperFolderNote));
				Assert.AreEqual(expectedIsPaperFolderRequired, rawSupportingDocument.IsPaperFolderRequired, nameof(rawSupportingDocument.IsPaperFolderRequired));
				Assert.AreEqual(expectedIsQuantityRequired, rawSupportingDocument.IsQuantityRequired, nameof(rawSupportingDocument.IsQuantityRequired));
				Assert.AreEqual(expectedIsRetroActiveRequired, rawSupportingDocument.IsRetroActiveRequired, nameof(rawSupportingDocument.IsRetroActiveRequired));
				Assert.AreEqual(expectedStartDate, rawSupportingDocument.StartDate, nameof(rawSupportingDocument.StartDate));
				Assert.AreEqual(expectedIsUnitOfQuantityRequired, rawSupportingDocument.IsUnitOfQuantityRequired, nameof(rawSupportingDocument.IsUnitOfQuantityRequired));
				Assert.AreEqual(expectedIsYearRequired, rawSupportingDocument.IsYearRequired, nameof(rawSupportingDocument.IsYearRequired));
				Assert.AreEqual(expectedSupportingDocumentType, rawSupportingDocument.Type, nameof(rawSupportingDocument.Type));
			});
		}
	}
}
