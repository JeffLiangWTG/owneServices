using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.CountryCodesCTC.DocumentTypeExcise
{
	[TestFixture]
	sealed class NctsDocumentTypeExciseTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsDocumentTypeExcise));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.DocumentTypeExcise));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.DocumentTypeExcise));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsDocumentTypeExcise;
		}

		NctsDocumentTypeExcise declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsDocumentTypeExcise();
	}
}
