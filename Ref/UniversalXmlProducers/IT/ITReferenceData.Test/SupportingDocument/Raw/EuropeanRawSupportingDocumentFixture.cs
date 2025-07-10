using System;
using System.IO;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class EuropeanRawSupportingDocumentFixture : RawSupportingDocumentFixture<EuropeanRawSupportingDocument>
	{
		[Test]
		public void ParseSupportingDocumentWithEmptyAttributes()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\EuropeanEmptyAttributes_Y001.html");
			var rawSupportingDocument = GetNewRawSupportingDocument(File.ReadAllText(htmlTestFile));
			RawSupportingDocumentAssertionFixture.AssertRawSupportingDocument(rawSupportingDocument
							, expectedIsCertificateIdRequired: false
							, expectedCode: "Y001"
							, expectedIsCountryRequired: false
							, expectedDescription: "Interamente ottenuto in Libano e trasportato direttamente da tale paese nella Comunità."
							, expectedElectronicFolderNote: ""
							, expectedIsElectronicFolderRequired: false
							, expectedEndDate: new DateTime(9999, 12, 31)
							, expectedPaperFolderNote: ""
							, expectedIsPaperFolderRequired: false
							, expectedIsQuantityRequired: false
							, expectedIsRetroActiveRequired: false
							, expectedStartDate: new DateTime(1998, 01, 24)
							, expectedIsUnitOfQuantityRequired: false
							, expectedIsYearRequired: false
							, expectedSupportingDocumentType: SupportingDocumentType.European);
		}

		[Test]
		public void ParseSupportingDocumentWithFilledAttributes()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\EuropeanFilledAttributes_N002.html");
			var rawSupportingDocument = GetNewRawSupportingDocument(File.ReadAllText(htmlTestFile));
			RawSupportingDocumentAssertionFixture.AssertRawSupportingDocument(rawSupportingDocument
								, expectedIsCertificateIdRequired: true
								, expectedCode: "N002"
								, expectedIsCountryRequired: true
								, expectedDescription: "Certificato di conformità alle norme di commercializzazione dell'Unione europea applicabili agli ortofrutticoli freschi"
								, expectedElectronicFolderNote: "FASCICOLO ELETTRONICO NOTE"
								, expectedIsElectronicFolderRequired: true
								, expectedEndDate: new DateTime(9999, 12, 31)
								, expectedPaperFolderNote: "FASCICOLO CARTACEO NOTE"
								, expectedIsPaperFolderRequired: true
								, expectedIsQuantityRequired: true
								, expectedIsRetroActiveRequired: true
								, expectedStartDate: new DateTime(2004, 07, 01)
								, expectedIsUnitOfQuantityRequired: true
								, expectedIsYearRequired: true
								, expectedSupportingDocumentType: SupportingDocumentType.European);
		}

		[Test]
		public void DescriptionWithSUBCharIsSanitized()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\EuropeanBadCharsInDescription_C072.html");
			var rawSupportingDocument = GetNewRawSupportingDocument(File.ReadAllText(htmlTestFile));
			Assert.AreEqual("Certificato di immatricolazione dell aeromobile in conformita' con la Convenzione sull aviazione civile internazionale del 7 dicembre 1944", rawSupportingDocument.Description, "Description has no SUB() char");
		}

		protected override IRawSupportingDocument GetNewRawSupportingDocument(string rawHtml) => new EuropeanRawSupportingDocument(rawHtml);
	}
}
