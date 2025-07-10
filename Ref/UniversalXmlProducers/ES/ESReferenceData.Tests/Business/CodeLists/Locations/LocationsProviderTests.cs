using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

[TestFixture]
public class LocationsProviderTests
{
	class LocationsProviderTester : LocationsProvider
	{
		protected override IWebRequestWrapper GetWebRequestWrapper() => TestWebRequestWrapper;
		public IWebRequestWrapper TestWebRequestWrapper { get; set; }

		public List<LocationOfficeCodesItem> GetOfficeCodesFromCsvFileExposed(string url, DateTime date) => GetOfficeCodesFromCsvFile(url, date);
	}

	[Test]
	public void TestRequestCodes()
	{
		var provider = new LocationsProviderTester();
		var webRequestWrapper = new Mock<IWebRequestWrapper>();

		var mockLocationsOfficesHtmlWebPageContent = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.Locations.TestFiles.Input.web_mock_locations_offices.html");
		var mockLocationsHtmlWebPageContent = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.Locations.TestFiles.Input.web-mock-locations.html");

		webRequestWrapper.Setup(x => x.GetContent("urlOffices?IDETAB=TADUANAS&VEZ=BUSCAR&FECCON=26-02-2021")).Returns(mockLocationsOfficesHtmlWebPageContent);
		var csvFileContent = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.Locations.TestFiles.Input.locations_offices.csv");
		var expectedPostData = "IDETAB=TADUANAS&POSINI=0&POSLON=0&AYUCMP=&CLASGTE=20%2f60%2f20201117103341795358&PAGINA=&CON_TOTALES=false&QUE_MODO=NORMAL&ESTADO_COLS=1&NUM_RESULTADOS_RS=305&VEZ=EXPOREXCEL&NOM_QUERY=es.aeat.adtb.jdit.web.tablas.ElementoQuery&COLUMNA_ORDEN=&MODO_ORDEN=&FECCON=26-02-2021&";
		webRequestWrapper.Setup(x => x.GetContentFromPost("urlOffices", expectedPostData, false)).Returns(csvFileContent);

		webRequestWrapper.Setup(x => x.GetContent("url?VEZ=BUSCAR&UBI_IDE_RECINTO=000101")).Returns(mockLocationsHtmlWebPageContent);
		webRequestWrapper.Setup(x => x.GetContentFromPost("url", "CLASGTE=20%2f60%2f20201117103341795358&TOTALES=Ubicaci%25F3n%253D20%2523%253A%2523&QUE-MODO=NORMAL&QUE_MODO=NORMAL&ESTADO_COLS=&NUM_RESULTADOS_RS=60&VEZ=AVANZAR&", true)).Returns("htmlPost1");
		webRequestWrapper.Setup(x => x.GetContentFromPost("url", "CLASGTE=40%2f60%2f20201117103341795358&TOTALES=Ubicaci%25F3n%253D40%2523%253A%2523&QUE-MODO=NORMAL&QUE_MODO=NORMAL&ESTADO_COLS=&NUM_RESULTADOS_RS=60&VEZ=AVANZAR&", true)).Returns("htmlPost2");

		provider.TestWebRequestWrapper = webRequestWrapper.Object;
		var documents = provider.GetLocationItems("url", "urlOffices", twentySixFebruary2021);
		var exepectedDocuments = ExpectedDocuments;

		CollectionAssert.AreEquivalent(documents.Select(x => x.Location), exepectedDocuments.Select(x => x.Location));
		CollectionAssert.AreEquivalent(documents.Select(x => x.Name), exepectedDocuments.Select(x => x.Name));
		CollectionAssert.AreEquivalent(documents.Select(x => x.StartDate), exepectedDocuments.Select(x => x.StartDate));
		CollectionAssert.AreEquivalent(documents.Select(x => x.EndDate), exepectedDocuments.Select(x => x.EndDate));
	}

	IEnumerable<(string Location, string Name, string StartDate, string EndDate)> ExpectedDocuments =>
	[
		("ES00010100DECO", "AEROPUERTO VIT ALMACÉN DECOEXA", "22-02-2019", "31-12-2099"),
		("ES00010101DECO", "AEROPUERTO VIT ALMACÉN DECOEXA", "24-11-2016", "25-04-2019"),
		("ES00010101DECO", "AEROPUERTO VIT ALMACÉN DECOEXA", "25-09-1996", "23-11-2016"),
		("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT ( DHL)", "24-04-2019", "31-12-2099"),
		("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT ( DHL)", "17-11-2016", "23-04-2019"),
		("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT ( DHL)", "15-11-2016", "16-11-2016"),
		("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT ( DHL)", "26-05-2016", "14-11-2016"),
		("ES00010101EAT", "AEROPUERTO VIT ALMACéN DE EAT ( DHL)", "26-11-1998", "25-05-2016"),
		("ES00010101GENE", "AEROPUERTO MERCANCÍAS RAMPA-ZONA RESTRI", "24-11-2016", "31-12-2099"),
		("ES00010101GENE", "AEROPUERTO MERCANCÍAS RAMPA-ZONA RESTRI", "11-10-1996", "23-11-2016"),
		("ES00010101IBER", "AEROPUERTO VIT ALMACEN DE IBERIA", "29-11-2016", "30-11-2016"),
		("ES00010101IBER", "AEROPUERTO VIT ALMACEN DE IBERIA", "24-11-1998", "28-11-2016"),
		("ES00010101TNT", "AEROPUERTO VIT ALMACÉN TNT", "24-04-2019", "31-12-2099"),
		("ES00010101TNT", "AEROPUERTO VIT ALMACÉN TNT", "15-11-2016", "23-04-2019"),
		("ES00010101TNT", "AEROPUERTO VIT ALMACÉN TNT", "04-06-2016", "14-11-2016"),
		("ES00010101TNT", "AEROPUERTO VIT ALMACÉN TNT", "04-02-1997", "03-06-2016"),
		("ES00010101UPS", "UNITED PARCEL SERVICES", "05-06-2017", "31-12-2099"),
		("ES00010101VIAS", "AEROPUERTO VIT ALMACÉN VIAS", "02-10-2020", "31-12-2099"),
		("ES00010101VIAS", "AEROPUERTO VIT ALMACÉN VIAS", "19-06-2020", "01-10-2020"),
		("ES00010101VIAS", "AEROPUERTO VIT ALMACÉN VIAS", "24-11-2016", "18-06-2020")
	];

	[Test]
	public void TestRequestOfficeCodesCsvFile()
	{
		var provider = new LocationsProviderTester();
		var webRequestWrapper = new Mock<IWebRequestWrapper>();

		var mockLocationsOfficesHtmlWebPageContent = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.Locations.TestFiles.Input.web_mock_locations_offices.html");

		webRequestWrapper.Setup(x => x.GetContent("url?IDETAB=TADUANAS&VEZ=BUSCAR&FECCON=26-02-2021")).Returns(mockLocationsOfficesHtmlWebPageContent);
		var postArgs = "IDETAB=TADUANAS&POSINI=0&POSLON=0&AYUCMP=&CLASGTE=20%2f60%2f20201117103341795358&PAGINA=&CON_TOTALES=false&QUE_MODO=NORMAL&ESTADO_COLS=1&NUM_RESULTADOS_RS=305&VEZ=EXPOREXCEL&NOM_QUERY=es.aeat.adtb.jdit.web.tablas.ElementoQuery&COLUMNA_ORDEN=&MODO_ORDEN=&FECCON=26-02-2021&";
		webRequestWrapper.Setup(x => x.GetContentFromPost("url", postArgs, false)).Returns("Código;Descripción;;\n0100;;;;\n0111;;;;");

		provider.TestWebRequestWrapper = webRequestWrapper.Object;
		var documents = provider.GetOfficeCodesFromCsvFileExposed("url", twentySixFebruary2021);

		var expectedDocuments = new LocationOfficeCodesItem [] { new () { Code = "0111" } };
		Assert.That(documents.Count, Is.EqualTo(1));
		Assert.That(documents[0].Code, Is.EqualTo(expectedDocuments[0].Code));
	}

	[Test]
	public void TestRequestOfficeCodesCsvFileExceptions()
	{
		var provider = new LocationsProviderTester();
		var webRequestWrapper = new Mock<IWebRequestWrapper>();
		var mockLocationsHtmlWebPageContent = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CustomsProcedureCodes.TestFiles.Input.web_mock_cpc_NULL.html");

		webRequestWrapper.Setup(x => x.GetContent("url?IDETAB=TADUANAS&VEZ=BUSCAR&FECCON=26-02-2021")).Returns(mockLocationsHtmlWebPageContent);
		provider.TestWebRequestWrapper = webRequestWrapper.Object;
		var exception = Assert.Throws<LocationsProviderException>(() => provider.GetOfficeCodesFromCsvFileExposed("url", twentySixFebruary2021));
		Assert.That(exception.Message, Is.EqualTo("Empty key or value. Or key can't have a value for 'AYUCMP', 'PAGINA', 'VEZ', 'COLUMNA_ORDEN', 'MODO_ORDEN', 'DEF_MASIVA'."));

		webRequestWrapper.Setup(x => x.GetContent("invalidHtml?IDETAB=TADUANAS&VEZ=BUSCAR&FECCON=26-02-2021")).Returns("<html></html>");
		exception = Assert.Throws<LocationsProviderException>(() => provider.GetOfficeCodesFromCsvFileExposed("invalidHtml", twentySixFebruary2021));
		Assert.That(exception.Message, Is.EqualTo("Cannot process the requested page. Invalid nodes."));
	}

	[Test]
	public void TestRequestCodesExceptions()
	{
		const string locationsURL = "url";
		const string validLocationsPath = "url?VEZ=BUSCAR&UBI_IDE_RECINTO=000111";

		const string locationsOfficesURL = "urlOffices";
		const string validLocationsOfficesPath = "urlOffices?IDETAB=TADUANAS&VEZ=BUSCAR&FECCON=26-02-2021";
		const string validCSVOfficesContent = "Código;Descripción;;\n0100;;;;\n0111;;;;";

		const string postArgumentsForLocationsScrapper = "IDETAB=TADUANAS&POSINI=0&POSLON=0&AYUCMP=&CLASGTE=20%2f60%2f20201117103341795358&PAGINA=&CON_TOTALES=false&QUE_MODO=NORMAL&ESTADO_COLS=1&NUM_RESULTADOS_RS=305&VEZ=EXPOREXCEL&NOM_QUERY=es.aeat.adtb.jdit.web.tablas.ElementoQuery&COLUMNA_ORDEN=&MODO_ORDEN=&FECCON=26-02-2021&";

		static IWebRequestWrapper GetAndConfigureMockWebRequest(string file, string mockLocationsOfficesWebPageContent)
		{
			var mockLocationsWebPageContent = TestHelper.ReadManifestResourceContentUTF8($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.Locations.TestFiles.Input.{file}");
			var mockWebRequestWrapper = new Mock<IWebRequestWrapper>();
			mockWebRequestWrapper.Setup(x => x.GetContent(validLocationsPath)).Returns(mockLocationsWebPageContent);
			mockWebRequestWrapper.Setup(x => x.GetContent(validLocationsOfficesPath)).Returns(mockLocationsOfficesWebPageContent);
			mockWebRequestWrapper.Setup(x => x.GetContentFromPost(locationsOfficesURL, postArgumentsForLocationsScrapper, false)).Returns(validCSVOfficesContent);
			return mockWebRequestWrapper.Object;
		}

		var provider = new LocationsProviderTester();
		var webRequestWrapper = new Mock<IWebRequestWrapper>();

		var mockLocationsOfficesHtmlWebPageContent = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.Locations.TestFiles.Input.web_mock_locations_offices.html");
		webRequestWrapper.Setup(x => x.GetContent(validLocationsOfficesPath)).Returns(mockLocationsOfficesHtmlWebPageContent);
		webRequestWrapper.Setup(x => x.GetContentFromPost(locationsOfficesURL, postArgumentsForLocationsScrapper, false)).Returns(validCSVOfficesContent);

		var mockLocationsHtmlWebPageContent = TestHelper.ReadManifestResourceContentUTF8("CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.Locations.TestFiles.Input.web-mock-locations-exceptions-null-value.html");
		webRequestWrapper.Setup(x => x.GetContent(validLocationsPath)).Returns(mockLocationsHtmlWebPageContent);
		provider.TestWebRequestWrapper = webRequestWrapper.Object;
		var exception = Assert.Throws<LocationsProviderException>(() => provider.GetLocationItems(locationsURL, locationsOfficesURL, twentySixFebruary2021));
		Assert.That(exception.Message, Is.EqualTo("Empty key or value. Or key can't have a value for 'ESTADO_COLS', 'VEZ'."));

		provider.TestWebRequestWrapper = GetAndConfigureMockWebRequest("web-mock-locations-exceptions-invalid-value.html", mockLocationsOfficesHtmlWebPageContent);
		exception = Assert.Throws<LocationsProviderException>(() => provider.GetLocationItems(locationsURL, locationsOfficesURL, twentySixFebruary2021));
		Assert.That(exception.Message, Is.EqualTo("Cannot continue due to empty or invalid 'NUM_RESULTADOS_RS'"));

		provider.TestWebRequestWrapper = GetAndConfigureMockWebRequest("web-mock-locations-exceptions-no-key.html", mockLocationsOfficesHtmlWebPageContent);
		exception = Assert.Throws<LocationsProviderException>(() => provider.GetLocationItems(locationsURL, locationsOfficesURL, twentySixFebruary2021));
		Assert.That(exception.Message, Is.EqualTo("Cannot continue due to empty or invalid 'NUM_RESULTADOS_RS'"));

		provider.TestWebRequestWrapper = GetAndConfigureMockWebRequest("web-mock-locations-exceptions-no-form.html", mockLocationsOfficesHtmlWebPageContent);
		exception = Assert.Throws<LocationsProviderException>(() => provider.GetLocationItems(locationsURL, locationsOfficesURL, twentySixFebruary2021));
		Assert.That(exception.Message, Is.EqualTo("Cannot process the requested page. Invalid nodes. Office Code: 0111"));
	}

	readonly DateTime twentySixFebruary2021 = new (2021, 2, 26);
}
