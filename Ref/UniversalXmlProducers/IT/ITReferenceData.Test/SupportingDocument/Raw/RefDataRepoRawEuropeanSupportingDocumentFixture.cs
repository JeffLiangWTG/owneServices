using System;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	sealed class RefDataRepoRawEuropeanSupportingDocumentFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentException>(() => new RefDataRepoRawEuropeanSupportingDocument(string.Empty, null, null, null));
		}

		[Test]
		public void TestProperties()
		{
			IRawSupportingDocument sut = new RefDataRepoRawEuropeanSupportingDocument(
				code: "code",
				startDate: new DateTime(2021, 1, 1),
				endDate: new DateTime(2021, 1, 1),
				description: "description");

			Assert.Multiple(() =>
			{
				Assert.That(sut.Code, Is.EqualTo("code"), "Code");
				Assert.That(sut.StartDate, Is.EqualTo(new DateTime(2021, 1, 1)), "StartDate");
				Assert.That(sut.EndDate, Is.EqualTo(new DateTime(2021, 1, 1)), "EndDate");
				Assert.That(sut.Description, Is.EqualTo("description"), "Description");
				Assert.That(sut.Type, Is.EqualTo(SupportingDocumentType.European), "Type");
				Assert.That(sut.IsRetroActiveRequired, Is.False, "IsRetroActiveRequired");
				Assert.That(sut.IsYearRequired, Is.False, "IsYearRequired");
				Assert.That(sut.IsCountryRequired, Is.False, "IsCountryRequired");
				Assert.That(sut.IsCertificateIdRequired, Is.False, "IsCertificateIdRequired");
				Assert.That(sut.IsQuantityRequired, Is.False, "IsQuantityRequired");
				Assert.That(sut.IsUnitOfQuantityRequired, Is.False, "IsUnitOfQuantityRequired");
				Assert.That(sut.IsElectronicFolderRequired, Is.False, "IsElectronicFolderRequired");
				Assert.That(sut.IsElectronicFolderRequired, Is.False, "IsElectronicFolderRequired");
				Assert.That(sut.IsPaperFolderRequired, Is.False, "IsPaperFolderRequired");
				Assert.That(sut.ElectronicFolderNote, Is.Empty, "ElectronicFolderNote");
				Assert.That(sut.PaperFolderNote, Is.Empty, "PaperFolderNote");
			});
		}
	}
}
