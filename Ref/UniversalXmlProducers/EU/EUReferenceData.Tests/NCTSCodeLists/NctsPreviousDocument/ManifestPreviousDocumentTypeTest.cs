using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class ManifestPreviousDocumentTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(previousDocument.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsP5PreviousDocumentCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(previousDocument.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.PreviousDocumentType));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(previousDocument.DataSource, Is.EqualTo("EUN DC40M Previous Document"));
		}

		[SetUp]
		public void Setup()
		{
			previousDocument = GetNctsCodeListDetails() as ManifestPreviousDocumentType;
		}
		ManifestPreviousDocumentType previousDocument;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new ManifestPreviousDocumentType();
	}
}
