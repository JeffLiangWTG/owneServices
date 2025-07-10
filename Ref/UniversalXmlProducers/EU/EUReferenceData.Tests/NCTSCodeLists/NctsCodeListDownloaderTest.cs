using System;
using System.Text;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	class NctsCodeListDownloaderTest
	{
		[Test]
		public void GuardClause()
		{
			var httpClientHelperMock = new Mock<IHttpClientHelper>();
			var codeListDetailMock = new Mock<IUCCExportCodeListDetail>();

			Assert.Throws<ArgumentNullException>(() => new NctsCodeListDownloader(errorBuilder: null, httpClientHelperMock.Object, codeListDetailMock.Object));
			Assert.Throws<ArgumentNullException>(() => new NctsCodeListDownloader(new StringBuilder(), httpClientHelper: null, codeListDetailMock.Object));
			Assert.Throws<ArgumentNullException>(() => new NctsCodeListDownloader(new StringBuilder(), httpClientHelperMock.Object, codeListDetail: null));
		}
	}
}
