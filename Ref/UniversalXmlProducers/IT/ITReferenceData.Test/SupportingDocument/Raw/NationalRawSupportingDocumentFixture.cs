using System;
using System.IO;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class NationalRawSupportingDocumentFixture : RawSupportingDocumentFixture<NationalRawSupportingDocument>
	{
		[Test]
		public void ParseSupportingDocumentWithEmptyAttributes()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\NationalEmptyAttributes_03YY.html");
			var rawSupportingDocument = GetNewRawSupportingDocument(File.ReadAllText(htmlTestFile));
			RawSupportingDocumentAssertionFixture.AssertRawSupportingDocument(rawSupportingDocument
							, expectedIsCertificateIdRequired: false
							, expectedCode: "03YY"
							, expectedIsCountryRequired: false
							, expectedDescription: "00100-Menzione speciale."
							, expectedElectronicFolderNote: ""
							, expectedIsElectronicFolderRequired: false
							, expectedEndDate: new DateTime(9999, 12, 31)
							, expectedPaperFolderNote: ""
							, expectedIsPaperFolderRequired: false
							, expectedIsQuantityRequired: false
							, expectedIsRetroActiveRequired: false
							, expectedStartDate: new DateTime(2010, 07, 01)
							, expectedIsUnitOfQuantityRequired: false
							, expectedIsYearRequired: false
							, expectedSupportingDocumentType: SupportingDocumentType.National);
		}

		[Test]
		public void ParseSupportingDocumentWithFilledAttributes()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\NationalFilledAttributes_01AO.html");
			var rawSupportingDocument = GetNewRawSupportingDocument(File.ReadAllText(htmlTestFile));
			RawSupportingDocumentAssertionFixture.AssertRawSupportingDocument(rawSupportingDocument
					, expectedIsCertificateIdRequired: true
					, expectedCode: "01AO"
					, expectedIsCountryRequired: true
					, expectedDescription: "Autorizzazione Ministero dello Sviluppo Economico ai fini del rispetto dell'obbligo di cui all'art. 2-quater del D.L. 10 gennaio 2006, n. 2 e successive modifiche."
					, expectedElectronicFolderNote: "FASCICOLO ELETTRONICO NOTE"
					, expectedIsElectronicFolderRequired: true
					, expectedEndDate: new DateTime(9999, 12, 31)
					, expectedPaperFolderNote: "FASCICOLO CARTACEO NOTE"
					, expectedIsPaperFolderRequired: true
					, expectedIsQuantityRequired: true
					, expectedIsRetroActiveRequired: true
					, expectedStartDate: new DateTime(2013, 05, 31)
					, expectedIsUnitOfQuantityRequired: true
					, expectedIsYearRequired: true
					, expectedSupportingDocumentType: SupportingDocumentType.National);
		}

		[Test]
		public void DescriptionWithNewLineCharsIsSanitized()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\NationalBadCharsInDescription_12YY.html");
			var rawSupportingDocument = GetNewRawSupportingDocument(File.ReadAllText(htmlTestFile));
			var expectedDescriptionWithoutLineBrakes =
				"Esenzione dal pagamento del dazio addizionale in virt? delle condizioni previste dall'art.4 del Reg.UE 2018/724: " +
				"1) per i prodotti elencati nell'allegato I del Reg.UE 2018/886 per i quali sia stata rilasciata, prima del 17/05/2018, una licenza di importazione che comporti un esenzione/riduzione del dazio; " +
				"2) per i prodotti per i quali gli importatori possano dimostrare che l'esportazione dagli USA sia avvenuta prima del 22/06/2018.";
			Assert.AreEqual(expectedDescriptionWithoutLineBrakes, rawSupportingDocument.Description, "Description has no line breaks");
		}

		[Test]
		public void DescriptionWithBlackListCharsIsSanitized()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\NationalBadCharsInDescription_35YY.html");
			var rawSupportingDocument = GetNewRawSupportingDocument(File.ReadAllText(htmlTestFile));
			Assert.AreEqual("Richiesta di utilizzo della procedura facilitata per il rilascio delle certificazioni EUR1/ATR/EURMED. La procedura facilitata è riservata ai soli operatori economici censiti dagli uffici doganali.", rawSupportingDocument.Description, "Description has no line breaks");
		}

		protected override IRawSupportingDocument GetNewRawSupportingDocument(string rawHtml) => new NationalRawSupportingDocument(rawHtml);
	}
}
