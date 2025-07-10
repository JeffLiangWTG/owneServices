using System;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class ConditionCreatorFixture
	{
		[Test]
		public void Get()
		{
			var measure = new measure
			{
				measureType = "711",
				measureCondition = new[] {
					new measureCondition { conditionCodeId = "A", national = 0, },
					new measureCondition { certificateCode = "001", certificateType = "L", conditionCodeId = "B", national = 0 },
				},
				dateEnd = new DateTime(2025, 02, 05, 00, 00, 0),
				dateEndSpecified = true,
			};
			var conditionCodes = new[] {
				new measureConditionCode{
					conditionCode="A",
					measureConditionCodeDescription = new[] {
						new measureConditionCodeDescription { description = "Antidumpnings-/utjämningstulldokument ska uppvisas", languageId = "SV" },
						new measureConditionCodeDescription { description = "Presentation of an anti-dumping/countervailing document", languageId = "EN" }
					}
				},
				new measureConditionCode
				{
					conditionCode="B",
					measureConditionCodeDescription = new[]
					{
						new measureConditionCodeDescription { description = "Certifikat/licens/dokument skall uppvisas", languageId = "SV"},
						new measureConditionCodeDescription { description = "Presentation of a certificate/licence/document", languageId = "EN"}
					}
				}
			};
			var conditionCreator = new ConditionCreator(new ConditionValueCreator(new ConditionValueTypeCreator()));
			var conditions = conditionCreator.Get(measure, new[] { "A01", "A02" }, conditionCodes, null).ToArray();

			Assert.AreEqual("711", conditions[0].ZX1_ZX2_NKConditionType);
			Assert.AreEqual("A01", conditions[0].ZX1_ZZS_NKPreference);
			Assert.AreEqual("Condition A:Presentation of an anti-dumping/countervailing document", conditions[0].ZX1_Comment);
			Assert.AreEqual(new DateTime(2025,02,05,23,59,0,0), conditions[0].ZX1_EndDate);
			Assert.AreEqual("711", conditions[1].ZX1_ZX2_NKConditionType);
			Assert.AreEqual("A02", conditions[1].ZX1_ZZS_NKPreference);
			Assert.AreEqual("Condition A:Presentation of an anti-dumping/countervailing document", conditions[1].ZX1_Comment);
			Assert.AreEqual("711", conditions[2].ZX1_ZX2_NKConditionType);
			Assert.AreEqual("A01", conditions[2].ZX1_ZZS_NKPreference);
			Assert.AreEqual("Condition B:Presentation of a certificate/licence/document", conditions[2].ZX1_Comment);
			Assert.AreEqual("711", conditions[3].ZX1_ZX2_NKConditionType);
			Assert.AreEqual("A02", conditions[3].ZX1_ZZS_NKPreference);
			Assert.AreEqual("Condition B:Presentation of a certificate/licence/document", conditions[3].ZX1_Comment);
		}
	}
}
