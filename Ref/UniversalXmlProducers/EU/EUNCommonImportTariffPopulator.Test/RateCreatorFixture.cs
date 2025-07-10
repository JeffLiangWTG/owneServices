using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class RateCreatorFixture
	{
		[Test]
		public void Get()
		{
			var dutyFormula = new Mock<IDutyFormulaCreator>();
			dutyFormula.Setup(x => x.Get(It.IsAny<IEnumerable<measureComponent>>(), null)).Returns("0.15 * VFD");
			var conditionDutyFormula = new Mock<IConditionDutyFormulaCreator>();
			conditionDutyFormula.Setup(x => x.Get("A", It.IsAny<IEnumerable<measureCondition>>(), null))
				.Returns("If(VFD/[TNE] >= 1259.02, (1536.0 - VFD/[TNE]) * [TNE], VFD * 0.22)");
			var uomCreator = new Mock<IRateUOMCreator>();
			var rateCreator = new RateCreator(dutyFormula.Object, conditionDutyFormula.Object, uomCreator.Object);
			var measure = new measure
			{
				measureComponent = new[] { new measureComponent() },
				measureCondition = new[] { new measureCondition { conditionCodeId = "A" } }
			};
			var rates = rateCreator.Get(measure, "A00", "DTY", new[] { "100", "200" }).ToArray();
			Assert.AreEqual("0.15 * VFD", rates[0].ZZ2_RateFormula);
			Assert.AreEqual("100", rates[0].ZZ2_ZZS_NKPreference);
			Assert.AreEqual("A00", rates[0].ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("DTY", rates[0].ZZ2_ZY1_ZZR_NKRateType);
			Assert.AreEqual("0.15 * VFD", rates[1].ZZ2_RateFormula);
			Assert.AreEqual("200", rates[1].ZZ2_ZZS_NKPreference);
			Assert.AreEqual("A00", rates[1].ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("DTY", rates[1].ZZ2_ZY1_ZZR_NKRateType);
			Assert.AreEqual("If(VFD/[TNE] >= 1259.02, (1536.0 - VFD/[TNE]) * [TNE], VFD * 0.22)", rates[2].ZZ2_RateFormula);
			Assert.AreEqual("100", rates[2].ZZ2_ZZS_NKPreference);
			Assert.AreEqual("A00", rates[2].ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("DTY", rates[2].ZZ2_ZY1_ZZR_NKRateType);
			Assert.AreEqual("If(VFD/[TNE] >= 1259.02, (1536.0 - VFD/[TNE]) * [TNE], VFD * 0.22)", rates[3].ZZ2_RateFormula);
			Assert.AreEqual("200", rates[3].ZZ2_ZZS_NKPreference);
			Assert.AreEqual("A00", rates[3].ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("DTY", rates[3].ZZ2_ZY1_ZZR_NKRateType);
		}
	}
}
