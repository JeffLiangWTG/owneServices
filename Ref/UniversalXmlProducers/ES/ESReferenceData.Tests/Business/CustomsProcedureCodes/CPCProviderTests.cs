using System.Text;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests
{
	[TestFixture]
	public class CPCProviderTests
	{
		class CPCProviderTester : CPCProvider
		{
			protected override IWebRequestWrapper GetWebRequestWrapper() => TestWebRequestWrapper;
			public IWebRequestWrapper TestWebRequestWrapper { get; set; }
		}

		[Test]
		public void TestRequestCodes()
		{
			var provider = new CPCProviderTester();
			var webRequestWrapper = new Mock<IWebRequestWrapper>();
			var twentySixNovember2020 = "26-11-2020";

			byte[] bytesExport = Encoding.Default.GetBytes(TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Input.Export.web_mock_cpc_EXPORT.html"));
			var getHTMLForExport = Encoding.UTF8.GetString(bytesExport);

			byte[] bytesImport = Encoding.Default.GetBytes(TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Input.Import.web_mock_cpc_IMPORT.html"));
			var getHTMLForImport = Encoding.UTF8.GetString(bytesImport);
			webRequestWrapper.Setup(x => x.GetContent("url?VEZ=BUSCAR&FECCON=26-11-2020&IDETAB=EXREG371")).Returns(getHTMLForExport);
			webRequestWrapper.Setup(x => x.GetContent("url?VEZ=BUSCAR&FECCON=26-11-2020&IDETAB=IMREG371")).Returns(getHTMLForImport);
			webRequestWrapper.Setup(x => x.GetContentFromPost("url", "IDETAB=EXREG371&POSINI=0&POSLON=0&AYUCMP=&CLASGTE=20%2f25%2f20201126110243224716&PAGINA=&CON_TOTALES=false&QUE_MODO=NORMAL&ESTADO_COLS=1&NUM_RESULTADOS_RS=25&VEZ=EXPOREXCEL&NOM_QUERY=es.aeat.adtb.jdit.web.tablas.ElementoQuery&COLUMNA_ORDEN=&MODO_ORDEN=&", false)).Returns("csvPlainTextForExport");
			webRequestWrapper.Setup(x => x.GetContentFromPost("url", "IDETAB=IMREG371&POSINI=0&POSLON=0&AYUCMP=&CLASGTE=20%2f71%2f20201126145556358940&PAGINA=&CON_TOTALES=false&QUE_MODO=NORMAL&ESTADO_COLS=1&NUM_RESULTADOS_RS=71&VEZ=EXPOREXCEL&NOM_QUERY=es.aeat.adip.jdit.web.qtbespe.QImreg371&COLUMNA_ORDEN=&MODO_ORDEN=&", false)).Returns("csvPlainTextForImport");

			provider.TestWebRequestWrapper = webRequestWrapper.Object;
			var csvExport = provider.RequestCodes("url", twentySixNovember2020, CPCProvider.CPCType.EXREG371);
			var csvImport = provider.RequestCodes("url", twentySixNovember2020, CPCProvider.CPCType.IMREG371);

			Assert.That(csvExport, Is.EqualTo("csvPlainTextForExport"));
			Assert.That(csvImport, Is.EqualTo("csvPlainTextForImport"));
		}

		[Test]
		public void TestExceptions()
		{
			var provider = new CPCProviderTester();
			var webRequestWrapper = new Mock<IWebRequestWrapper>();
			byte[] bytes = Encoding.Default.GetBytes(TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Input.web_mock_cpc_NULL.html"));
			var getHTML = Encoding.UTF8.GetString(bytes);
			var twentySixFebruary2020 = "26-02-2020";

			webRequestWrapper.Setup(x => x.GetContent("url?VEZ=BUSCAR&FECCON=26-02-2020&IDETAB=EXREG371")).Returns(getHTML);
			provider.TestWebRequestWrapper = webRequestWrapper.Object;
			var exception = Assert.Throws<CPCException>(() => provider.RequestCodes("url", twentySixFebruary2020, CPCProvider.CPCType.EXREG371));
			Assert.That(exception.Message, Is.EqualTo("Empty key or value. Or key can't have a value for 'DEF_MASIVA', 'AYUCMP', 'PAGINA', 'VEZ', 'COLUMNA_ORDEN', 'MODO_ORDEN'."));

			webRequestWrapper.Setup(x => x.GetContent("invalidHtml?VEZ=BUSCAR&FECCON=26-02-2020&IDETAB=EXREG371")).Returns("<html></html>");
			exception = Assert.Throws<CPCException>(() => provider.RequestCodes("invalidHtml", twentySixFebruary2020, CPCProvider.CPCType.EXREG371));
			Assert.That(exception.Message, Is.EqualTo("Cannot process the requested page. Invalid nodes. Type: EXREG371."));
		}

		[Test]
		public void TestSanitizeLastEmptyLine()
		{
			var provider = new CPCProviderTester();
			var webRequestWrapper = new Mock<IWebRequestWrapper>();
			var twentySixNovember2020 = "26-11-2020";

			byte[] bytesExport = Encoding.Default.GetBytes(TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Input.Export.web_mock_cpc_EXPORT.html"));
			var getHTMLForExport = Encoding.UTF8.GetString(bytesExport);

			byte[] bytesImport = Encoding.Default.GetBytes(TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Input.Import.web_mock_cpc_IMPORT.html"));
			var getHTMLForImport = Encoding.UTF8.GetString(bytesImport);
			webRequestWrapper.Setup(x => x.GetContent("url?VEZ=BUSCAR&FECCON=26-11-2020&IDETAB=EXREG371")).Returns(getHTMLForExport);
			webRequestWrapper.Setup(x => x.GetContent("url?VEZ=BUSCAR&FECCON=26-11-2020&IDETAB=IMREG371")).Returns(getHTMLForImport);
			var csvPlainTextForExport = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Input.Export.EXPORT_CPC_LAST_LINE_EMPTY.csv");
			var csvPlainTextForImport = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Input.Import.IMPORT_CPC_LAST_LINE_EMPTY.csv");

			webRequestWrapper.Setup(x => x.GetContentFromPost("url", "IDETAB=EXREG371&POSINI=0&POSLON=0&AYUCMP=&CLASGTE=20%2f25%2f20201126110243224716&PAGINA=&CON_TOTALES=false&QUE_MODO=NORMAL&ESTADO_COLS=1&NUM_RESULTADOS_RS=25&VEZ=EXPOREXCEL&NOM_QUERY=es.aeat.adtb.jdit.web.tablas.ElementoQuery&COLUMNA_ORDEN=&MODO_ORDEN=&", false)).Returns(csvPlainTextForExport);
			webRequestWrapper.Setup(x => x.GetContentFromPost("url", "IDETAB=IMREG371&POSINI=0&POSLON=0&AYUCMP=&CLASGTE=20%2f71%2f20201126145556358940&PAGINA=&CON_TOTALES=false&QUE_MODO=NORMAL&ESTADO_COLS=1&NUM_RESULTADOS_RS=71&VEZ=EXPOREXCEL&NOM_QUERY=es.aeat.adip.jdit.web.qtbespe.QImreg371&COLUMNA_ORDEN=&MODO_ORDEN=&", false)).Returns(csvPlainTextForImport);

			provider.TestWebRequestWrapper = webRequestWrapper.Object;
			var csvExport = provider.RequestCodes("url", twentySixNovember2020, CPCProvider.CPCType.EXREG371);
			var csvImport = provider.RequestCodes("url", twentySixNovember2020, CPCProvider.CPCType.IMREG371);

			var expectedCsvPlainTextForExport = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Output.EXPORT_CPC_LAST_LINE_EMPTY.csv");
			var expectedCsvPlainTextForImport = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Output.IMPORT_CPC_LAST_LINE_EMPTY.csv");

			Assert.That(csvExport, Is.EqualTo(expectedCsvPlainTextForExport));
			Assert.That(csvImport, Is.EqualTo(expectedCsvPlainTextForImport));
		}
	}
}
