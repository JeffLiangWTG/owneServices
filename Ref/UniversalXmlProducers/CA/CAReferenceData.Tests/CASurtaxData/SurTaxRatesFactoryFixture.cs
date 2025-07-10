using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CASurtaxData
{
	[TestFixture]
	class SurTaxRatesFactoryFixture
	{
		[TestCase("Table 1", 0.25)]
		[TestCase("Table 2", 0.1)]
		public void GetTariffs(string tableName, decimal duty)
		{
			var tariffProducer = new Mock<ITariffDataProducer>();
			var tariff = new RefCusTariff();
			tariffProducer.Setup(x => x.GetTariff(It.IsAny<IEnumerable<string>>(), 10))
				.Returns(new[] { tariff });
			var webText = new WebSurTaxText
			{
				Table = tableName,
			};
			var producer = new SurTaxRatesFactory(tariffProducer.Object);
			var result = producer.GetTariffs(webText).FirstOrDefault();
			var rate = result.RefCusRates[0];
			Assert.AreEqual(new DateTime(2018, 07, 01), rate.ZZ2_StartDate);
			Assert.AreEqual("SUR", rate.ZZ2_ZY1_NKRateCode);
			Assert.AreEqual($"{duty} * VFD", rate.ZZ2_RateFormula);
			var app = rate.RefCusApplicabilities[0];
			Assert.AreEqual(new DateTime(2018, 07, 01), app.ZZT_StartDate);
			Assert.AreEqual("US", app.ZZT_ZZA_NKTradeGroup);
		}
	}
}
