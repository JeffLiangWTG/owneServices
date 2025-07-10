using CargoWise.RefDbRepo.ESReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests.CmdLine;

[TestFixture]
class ApplicationConfigTests
{
	[Test]
	public void OutputDirectory()
	{
		Assert.That(@"..\..\UXmlFiles", Is.EqualTo(ApplicationConfig.OutputPath));
	}

	[Test]
	public void ExchangeRatesURL()
	{
		Assert.That("https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml", Is.EqualTo(ApplicationConfig.ExchangeRatesURL));
	}

	[Test]
	public void MeasuresURL()
	{
		Assert.That("https://atenea.int.taric.es/measures_es.json", Is.EqualTo(ApplicationConfig.MeasuresURL));
	}

	[Test]
	public void MeasuresCodesURL()
	{
		Assert.That("https://atenea.int.taric.es/measures_codes.json", Is.EqualTo(ApplicationConfig.MeasuresCodesURL));
	}

	[Test]
	public void FootnotesURL()
	{
		Assert.That("https://atenea.int.taric.es/measures_notes.json", Is.EqualTo(ApplicationConfig.FootnotesURL));
	}

	[Test]
	public void CanaryIslandMeasuresURL()
	{
		Assert.That("https://atenea.int.taric.es/measures_can.json", Is.EqualTo(ApplicationConfig.CanaryIslandMeasuresURL));
	}

	[Test]
	public void CanaryIslandFootnotesURL()
	{
		Assert.That("https://atenea.int.taric.es/measures_can_notes.json", Is.EqualTo(ApplicationConfig.CanaryIslandFootnotesURL));
	}

	[Test]
	public void CanaryIslandCodesURL()
	{
		Assert.That("https://atenea.int.taric.es/measures_can_codes.json", Is.EqualTo(ApplicationConfig.CanaryIslandCodesURL));
	}

	[Test]
	public void CanaryIslandExciseURL()
	{
		Assert.That("https://atenea.int.taric.es/excise_base_type.json", Is.EqualTo(ApplicationConfig.CanaryIslandExciseURL));
	}

	[Test]
	public void C44DocumentsURL()
	{
		Assert.That("https://www1.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.adta.trans.bdm.TtCodCerIntQuery", Is.EqualTo(ApplicationConfig.C44DocumentsURL));
	}

	[Test]
	public void LocationsURL()
	{
		Assert.That("https://www1.agenciatributaria.gob.es/l/inwinvoc/es.aeat.dit.adu.adaa.ubica.consulta.QUbicacInt", Is.EqualTo(ApplicationConfig.LocationsURL));
	}

	[Test]
	public void ElementoQueryAeatURL()
	{
		Assert.That("https://www1.agenciatributaria.gob.es/wlpl/ADTB-JDIT/ElementoQuery", Is.EqualTo(ApplicationConfig.ElementoQueryAeatURL));
	}

	[Test]
	public void RefDbServiceURI()
	{
		Assert.That("https://refdbrepoupdate.wisecloud.zone/Update/odata/", Is.EqualTo(ApplicationConfig.RefDbServiceURI));
	}

	[Test]
	public void TariffOneURL()
	{
		Assert.That("https://tariffone.com/wtg_data_export/", Is.EqualTo(ApplicationConfig.TariffOneURL));
	}
}
