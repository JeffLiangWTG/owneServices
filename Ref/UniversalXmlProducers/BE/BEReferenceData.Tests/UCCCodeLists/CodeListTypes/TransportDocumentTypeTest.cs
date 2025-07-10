using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	sealed class TransportDocumentTypeTest
	{
		[Test]
		public void TestDomain()
		{
			Assert.That(codeListDetail.Domain, Is.EqualTo(Constants.UccConstants.ExportDomain));
		}

		[Test]
		public void TestCodeType()
		{
			Assert.That(codeListDetail.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.UccTransportDocumentType));
		}

		[Test]
		public void TestCodeListType()
		{
			Assert.That(codeListDetail.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.TransportDocumentType));
		}

		[Test]
		public void TestDataSource()
		{
			Assert.That(codeListDetail.DataSource, Is.EqualTo(Constants.DataSources.BeTransportDocuments));
		}

		IUCCCodeListDetails codeListDetail => new ExportTransportDocumentType();
	}
}
