using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.XmlService;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	public class XmlServiceTests
	{
		[Test]
		public void ValidateRefCusTariffDataNode_WithoutError()
		{
			var today = DateTime.Today;

			var refCusTariff = new RefCusTariff("0102030405");

			var rateWrapper = new ScrappedRate()
			{
				RateCode = "912",
				RateType = "LEV",
				RateFormula = "150 EURO/1000 kg",
				StartDate = today,
				Applicability = new ScrappedApplicability()
				{
					AdditionalCode = null,
					StartDate = today,
					TradeGroup = "1011",
				},
				MeasurementUnits = new HashSet<string>()
			};

			refCusTariff.CusRates.Add(RefCusRate.Create(rateWrapper));
			refCusTariff.VatApplicabilities.Add(new RefCusVatApplicability("ORD", "T001", today));

			_xmlService.SerializeThenAppend(refCusTariff);

			var universalXml = _xmlService.GetUniversalXml();
			var actualNode = universalXml.SelectSingleNode("//UniversalReferenceData/RefCusTariff");

			var expectedXmlContent = new Regex("[\t\r\n]").Replace(_sampleXml, string.Empty);

			var expectedXml = new XmlDocument();
			expectedXml.LoadXml(expectedXmlContent);
			var expectedNode = expectedXml.SelectSingleNode("//UniversalReferenceData/RefCusTariff");

			Assert.IsNotNull(actualNode);
			Assert.IsNotNull(expectedNode);
			Assert.AreEqual(expectedNode.InnerXml, actualNode.InnerXml);
		}

		[Test]
		public void CheckPublicationDateUpdate()
		{
			var today = DateTime.Today;

			_xmlService.UpdatePublicationDate(today);
			var doc = _xmlService.GetUniversalXml();
			var publicationNode = doc.SelectSingleNode("//PublicationTime");

			Assert.IsNotNull(publicationNode);
			Assert.AreEqual(today.ToString("s"), publicationNode.InnerText);
		}

		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new XmlService.XmlService(logger: null));
		}

		[SetUp]
		protected void Setup()
		{
			_xmlService = new XmlService.XmlService(new Mock<ILogger>().Object);
		}

		IXmlService _xmlService;

		readonly string _sampleXml = @"<UniversalReferenceData>
    <RefCusTariff>
    <ZZ1_TariffCode>0102030405</ZZ1_TariffCode>
    <RefCusVATApplicability>
      <ZX5_ZZF_NKTaxOrFeeCode>ORD</ZX5_ZZF_NKTaxOrFeeCode>
      <ZX5_StartDate>" + $"{DateTime.Today:s}" + @"</ZX5_StartDate>
      <ZX5_AdditionalCode>T001</ZX5_AdditionalCode>
    </RefCusVATApplicability>
    <RefCusRate>
      <ZZ2_StartDate>" + $"{DateTime.Today:s}" + @"</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>912</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>LEV</ZZ2_ZY1_ZZR_NKRateType>
      <ZZ2_RateFormula>150 EURO/1000 kg</ZZ2_RateFormula>
      <RefCusApplicability>
        <ZZT_StartDate>" + $"{DateTime.Today:s}" + @"</ZZT_StartDate>
        <ZZT_AdditionalCode />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
      </RefCusApplicability>
    </RefCusRate>
  </RefCusTariff>
</UniversalReferenceData>";
	}
}
