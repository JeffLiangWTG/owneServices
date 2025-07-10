using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{

	[TestFixture]
	public class HsnTariffDTYRateFootnoteExplainerTest
	: HsnTariffDTYRateExplainerBaseTests<HsnTariffDTYRateFootnoteExplainer>
	{
		protected override HsnTariffDTYRateFootnoteExplainer CreateExplainer()
		{
			var path = ExtractEmbeddedExcel("CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.DTYRateExplanation.xlsx");
			return new HsnTariffDTYRateFootnoteExplainer(path, LoggerMock.Object);
		}

		[Test]
		public void TestAllRules_ShouldHaveAllRows()
		{
			var rules = Explainer.AllRules.ToList();
			Assert.That(rules, Is.Not.Null);
			Assert.That(rules.Count, Is.EqualTo(13));
			Assert.That(rules[11].Section, Is.EqualTo("13-15.FASIL"));
		}

		[Test]
		public void TestExplain_ShouldReturnRowBasedOnSectionColumn()
		{
			string section = "4.FASIL";
			string cellValue = "5%(1)";
			bool isPreference = true;

			var results = Explainer.Explain(section, cellValue, isPreference, relatedCell => "SomeValue").ToList();
			Assert.That(results, Is.Not.Null);
			var rule = results[0];

			Assert.That(rule.Section, Is.EqualTo("4.FASIL"));
			Assert.That(rule.FootnoteCode, Is.EqualTo("1"));
			Assert.That(rule.TradingPartners.First, Is.EqualTo(new HsnTariffDTYRateTradingPartner("IR")));
			Assert.That(rule.Formula, Is.EqualTo("VFD*SomeValue/100*0.80"));
			Assert.That(rule.RateType, Is.EqualTo("DTY"));
			Assert.That(rule.RateCode, Is.EqualTo("10"));
			Assert.That(rule.StartDate, Is.EqualTo(DtyRateConstants.DefaultStartDate));
			Assert.That(rule.EndDate, Is.EqualTo(DtyRateConstants.DefaultEndDate));
			Assert.That(cellValue.Contains(rule.FootnoteCode));
		}
	}
}
