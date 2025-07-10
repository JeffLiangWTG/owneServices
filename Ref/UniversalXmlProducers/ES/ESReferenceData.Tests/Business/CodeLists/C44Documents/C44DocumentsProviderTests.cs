using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

[TestFixture]
public class C44DocumentsProviderTests
{
	class C44DocumentsProviderTester : C44DocumentsProvider
	{
		protected override IWebRequestWrapper GetWebRequestWrapper() => TestWebRequestWrapper;
		public IWebRequestWrapper TestWebRequestWrapper { get; set; }
	}

	[Test]
	public void TestRequestCodes()
	{
		var provider = new C44DocumentsProviderTester();
		var webRequestWrapper = new Mock<IWebRequestWrapper>();

		byte[] bytes = Encoding.Default.GetBytes(TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.C44Documents.TestFiles.Input.web-mock-c44documents.html"));
		var getHTML = Encoding.UTF8.GetString(bytes);

		webRequestWrapper.Setup(x => x.GetContent("url?VEZ=BUSCAR&FEC_CONSULTA=20-05-2019")).Returns(getHTML);
		webRequestWrapper.Setup(x => x.GetContentFromPost("url", "CLASGTE=20%2f537%2f20201008115515976074&TOTALES=Tipo%2bCertificado%253D20%2523%253A%2523&QUE-MODO=NORMAL&QUE_MODO=NORMAL&ESTADO_COLS=&NUM_RESULTADOS_RS=60&VEZ=AVANZAR&", true)).Returns("htmlPost1");
		webRequestWrapper.Setup(x => x.GetContentFromPost("url", "CLASGTE=40%2f537%2f20201008115515976074&TOTALES=Tipo%2bCertificado%253D40%2523%253A%2523&QUE-MODO=NORMAL&QUE_MODO=NORMAL&ESTADO_COLS=&NUM_RESULTADOS_RS=60&VEZ=AVANZAR&", true)).Returns("htmlPost2");

		provider.TestWebRequestWrapper = webRequestWrapper.Object;
		var documents = provider.RequestCodes("url", "20-05-2019");
		var exepectedDocuments = ExpectedDocuments;

		CollectionAssert.AreEquivalent(documents.Select(x => x.Code), exepectedDocuments.Select(x => x.Code));
		CollectionAssert.AreEquivalent(documents.Select(x => x.Description), exepectedDocuments.Select(x => x.Description));
	}

	[Test]
	public void TestRequestCodesNCTS()
	{
		var provider = new C44DocumentsProviderTester();
		var webRequestWrapper = new Mock<IWebRequestWrapper>();
		var twentySixNovember2020 = "26-11-2020";

		byte[] bytes = Encoding.Default.GetBytes(TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.C44Documents.TestFiles.Input.web-mock-c44documents-ncts.html"));
		var getHTML = Encoding.UTF8.GetString(bytes);

		webRequestWrapper.Setup(x => x.GetContent("url?VEZ=BUSCAR&FECCON=26-11-2020&IDETAB=CSRDT213")).Returns(getHTML);
		var postArgs = "DEF_MASIVA=&IDETAB=CSRDT213&POSINI=0&POSLON=0&AYUCMP=&CLASGTE=20%2f39%2f20240115092526433046&PAGINA=&CON_TOTALES=false&VERIFICAR_CAMPOS_REQUERIDOS=false&QUE_MODO=NORMAL&ESTADO_COLS=1&NUM_RESULTADOS_RS=39&VEZ=EXPOREXCEL&NOM_QUERY=es.aeat.adtb.jdit.web.tablas.ElementoQuery&COLUMNA_ORDEN=&MODO_ORDEN=&";
		webRequestWrapper.Setup(x => x.GetContentFromPost("url", postArgs, false)).Returns("csvPlainTextForNCTS");

		provider.TestWebRequestWrapper = webRequestWrapper.Object;
		var csvExport = provider.RequestCodesNCTS("url", twentySixNovember2020, C44DocumentsProvider.NctsC44Type.CSRDT213);

		Assert.That(csvExport, Is.EqualTo("csvPlainTextForNCTS"));
	}

	public static IEnumerable<(string Code, string Description)> ExpectedDocuments =>
	[
		("A010", "Certificado de autenticidad zumo de naranja concentrado"),
		("A009", "Certificado de autenticidad frescas minneolas"),
		("A008", "Certificado de autenticidad frescas naranjas dulces \"de calidad superior\""),
		("A004", "Certificado de autenticidad del tabaco"),
		("A001", "Certificado de autenticidad uvas frescas de mesa 'EMPERADOR'"),
		("A014", "Certificado de autenticidad HANDI"),
		("A015", "Certificado de autenticidad (Productos de seda o de algodón tejidos en telares a mano)"),
		("A017", "Certificado de autenticidad conforme a las disposiciones del Reglamento (UE) No 593/2013 (DO L 170)"),
		("A019", "Certificado de calidad:Nitrato de Chile"),
		("A022", "Certificado de autenticidad: \"Certificate of authenticity B \"Basmati Rice\" for export to the European Community\""),
		("A023", "Certificado de autenticidad conforme a las disposiciones del Reglamento (UE) No 481/2012 (DO L 148)"),
		("A007", "Certificado de autenticidad - Carne de vaca y ternera congelada (cortes de cuartos delanteros de pecho llamados australianos )"),
		("C406", "Certificado de instrumento musical CITES"),
		("C043", "Certificado de reexportatión CICAA para el patudo o Certificado de reexportatión CAOI-patudo"),
		("C400", "Presentación del certificado \"CITES\" requerida"),
		("C019", "OPO - Autorización de utilización del régimen de perfeccionamiento pasivo (columna 8b del Anexo A del Reglamento Delegado (UE) 2015/2446)"),
		("C018", "Extracto V I 2 anotado de conformidad con el artículo2 0, apartado 2, del Reglamento (UE) 2018/273"),
		("C017", "Documento V I 1 anotado de conformidad con el artículo 25, apartado 2, del Reglamento (UE) 2018/273"),
		("C013", "Certificado IMA 1"),
		("C012", "Certificate for the export of pasta to the USA (P 2 certificate).")
	];

	[Test]
	public void TestRequestCodesExceptions()
	{
		const string c44URL = "url";
		const string validC44Path = "url?VEZ=BUSCAR&FEC_CONSULTA=26-02-2021";

		static IWebRequestWrapper GetAndConfigureMockWebRequest(string file)
		{
			var mockc44WebPageContent = TestHelper.ReadManifestResourceContentUTF8($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.C44Documents.TestFiles.Input.{file}");
			var mockWebRequestWrapper = new Mock<IWebRequestWrapper>();
			mockWebRequestWrapper.Setup(x => x.GetContent(validC44Path)).Returns(mockc44WebPageContent);
			return mockWebRequestWrapper.Object;
		}

		var provider = new C44DocumentsProviderTester();
		var webRequestWrapper = new Mock<IWebRequestWrapper>();

		provider.TestWebRequestWrapper = GetAndConfigureMockWebRequest("web-mock-c44documents-invalid-value.html");
		var exception = Assert.Throws<C44DocumentsException>(() => provider.RequestCodes(c44URL, "26-02-2021"));
		Assert.That(exception.Message, Is.EqualTo("Empty key or value. Or key can't have a value for 'ESTADO_COLS', 'VEZ'."));

		provider.TestWebRequestWrapper = GetAndConfigureMockWebRequest("web-mock-c44documents-no-key.html");
		exception = Assert.Throws<C44DocumentsException>(() => provider.RequestCodes(c44URL, "26-02-2021"));
		Assert.That(exception.Message, Is.EqualTo("Empty key or value. Or key can't have a value for 'ESTADO_COLS', 'VEZ'."));

		provider.TestWebRequestWrapper = GetAndConfigureMockWebRequest("web-mock-c44documents-no-form.html");
		exception = Assert.Throws<C44DocumentsException>(() => provider.RequestCodes(c44URL, "26-02-2021"));
		Assert.That(exception.Message, Is.EqualTo("Cannot process the requested page. Invalid nodes."));
	}
}
