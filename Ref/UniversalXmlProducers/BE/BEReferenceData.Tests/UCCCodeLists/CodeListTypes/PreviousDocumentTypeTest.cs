using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class PreviousDocumentTypeTest
	{
		[Test]
		public void TestDomain()
		{
			Assert.That(codeListDetail.Domain, Is.EqualTo(Constants.UccConstants.ExportDomain));
		}

		[Test]
		public void TestCodeType()
		{
			Assert.That(codeListDetail.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.UccPreviousDocumentType));
		}

		[Test]
		public void TestCodeListType()
		{
			Assert.That(codeListDetail.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.PreviousDocumentType));
		}

		[Test]
		public void TestDataSource()
		{
			Assert.That(codeListDetail.DataSource, Is.EqualTo(Constants.DataSources.BePreviousDocuments));
		}

		IUCCCodeListDetails codeListDetail => new ExportPreviousDocumentType();
	}
}
