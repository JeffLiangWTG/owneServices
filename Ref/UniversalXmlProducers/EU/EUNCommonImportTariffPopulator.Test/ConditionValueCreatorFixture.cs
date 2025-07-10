using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class ConditionValueCreatorFixture
	{
		[TestCase("A", "008", "D", "", false, 0, "D008", "SNR")]
		[TestCase("A", "", "", "", false, 0, "Apply the mentioned duty", "INF")]
		[TestCase("B", "990", "C", "", false, 0, "C990", "SUP")]
		[TestCase("E", "", "", "KGM", true, 20.0, "VFD/[KGM] <= 20", "FRM")]
		public void Get(string conditionCode, string certificateCode, string certificateType, string unitCode, bool dutyAmountSpecified, decimal dutyAmount, string zx3_value, string valueType)
		{
			var conditions = new []
			{
				new measureCondition
				{
					certificateCode = certificateCode,
					certificateType = certificateType,
					conditionCodeId = conditionCode,
					measurementUnitCode = unitCode,
					dutyAmountSpecified = dutyAmountSpecified,
					dutyAmount = dutyAmount,
					national = 0L,
					sequenceNumber = "001",
					SID = 529777
				}
			};
			var conditionValueCreator = new ConditionValueCreator(new ConditionValueTypeCreator());
			foreach(var conditionPerType in conditions.GroupBy(x => x.conditionCodeId))
			{
				var conditionValues = conditionValueCreator.Get(conditionPerType, null);
				Assert.AreEqual(zx3_value, conditionValues.First().ZX3_Value);
				Assert.AreEqual(valueType, conditionValues.First().ZX3_ZX4_NKValueType);
			}
		}

		[Test]
		public void GetWhenConditionCodeIsR()
		{
			var measures = new[]
			{
				new measure
				{
					goodsNomenclatureCode="7326909240",
					measureComponent=new[]
					{
						new measureComponent
						{
							dutyExpressionId="99",
							measurementUnitCode="MTQ"
						}
					}
				}
			};
			var conditions = new[]
			{
				new measureCondition
				{
					actionCode = "10",
					conditionCodeId = "R",
					dutyAmountSpecified = true,
					dutyAmount = 0.036m,
					measurementUnitCode = "KGM",
					national = 0L,
					sequenceNumber = "001",
					SID = 1629315
				},
				new measureCondition
				{
					actionCode = "28",
					conditionCodeId = "R",
					dutyAmountSpecified = true,
					dutyAmount = 0.0m,
					measurementUnitCode = "KGM",
					national = 0L,
					sequenceNumber = "002",
					SID = 1629316
				}
			};
			var conditionValueCreator = new ConditionValueCreator(new ConditionValueTypeCreator());
			foreach (var conditionPerType in conditions.GroupBy(x => x.conditionCodeId))
			{
				var conditionValues = conditionValueCreator.Get(conditionPerType, "MTQ");
				Assert.AreEqual("[KGM]/[MTQ] < 0.036 & [KGM]/[MTQ] >= 0.0", conditionValues.First().ZX3_Value);
				Assert.AreEqual("FRM", conditionValues.First().ZX3_ZX4_NKValueType);
			}
		}

		[Test]
		public void GetWhenConditionCodeIsU()
		{
			var conditions = new[]
			{
				new measureCondition
				{
					actionCode = "10",
					conditionCodeId = "U",
					dutyAmountSpecified = true,
					dutyAmount = 0.0m,
					measurementUnitCode = "NAR",
					national = 0L,
					sequenceNumber = "002",
					SID = 1629316
				},
				new measureCondition
				{
					actionCode = "28",
					conditionCodeId = "U",
					dutyAmountSpecified = true,
					dutyAmount = 65.0m,
					measurementUnitCode = "NAR",
					national = 0L,
					sequenceNumber = "001",
					SID = 1629315
				}
			};
			var conditionValueCreator = new ConditionValueCreator(new ConditionValueTypeCreator());
			foreach (var conditionPerType in conditions.GroupBy(x => x.conditionCodeId))
			{
				var conditionValues = conditionValueCreator.Get(conditionPerType, null);
				Assert.AreEqual("VFD/[NAR] > 65.0", conditionValues.First().ZX3_Value);
				Assert.AreEqual("FRM", conditionValues.First().ZX3_ZX4_NKValueType);
			}
		}
	}
}
