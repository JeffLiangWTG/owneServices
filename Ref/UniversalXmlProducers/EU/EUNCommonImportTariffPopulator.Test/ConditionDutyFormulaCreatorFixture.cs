using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class ConditionDutyFormulaCreatorFixture
	{
		[Test]
		public void Condition_L()
		{
			var condition1 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "L",
				dutyAmount = 133.4m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "003",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 50.0m,
						dutyExpressionId = "36",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 130.065m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition2 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "L",
				dutyAmount = 200.1m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "002",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 30.0m,
						dutyExpressionId = "36",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 90.045m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition3 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "L",
				dutyAmount = 300.15m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "001",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition4 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "L",
				dutyAmount = 0m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "005",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 90m,
						dutyExpressionId = "36",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 173.42m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition5 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "L",
				dutyAmount = 83.375m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "004",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 70.0m,
						dutyExpressionId = "36",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 156.745m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var creator = new ConditionDutyFormulaCreator();
			Assert.AreEqual("If(CIF/[DTN] > 300.15, 0, If(CIF/[DTN] > 200.1, 90.045 * [DTN] - CIF * 0.3, If(CIF/[DTN] > 133.4, 130.065 * [DTN] - CIF * 0.5, If(CIF/[DTN] > 83.375, 156.745 * [DTN] - CIF * 0.7, 173.42 * [DTN] - CIF * 0.9))))",
creator.Get("L", new[] { condition1, condition2, condition3, condition4, condition5 }, null));
		}

		[Test]
		public void Condition_F()
		{
			var condition1 = new measureCondition
			{
				actionCode = "11",
				conditionCodeId = "F",
				dutyAmount = 0.0m,
				dutyAmountSpecified = true,
				measurementUnitCode = "MIL",
				monetaryUnitCode = "EUR",
				sequenceNumber = "002",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 325.0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true,
						measurementUnitCode = "MIL",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition2 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "F",
				dutyAmount = 325.0m,
				dutyAmountSpecified = true,
				measurementUnitCode = "MIL",
				monetaryUnitCode = "EUR",
				sequenceNumber = "001",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true,
						measurementUnitCode = "MIL",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var creator = new ConditionDutyFormulaCreator();
			Assert.AreEqual("If(VFD/[MIL] >= 325.0, 0, (325.0 - VFD/[MIL]) * [MIL])",
creator.Get("F", new[] { condition1, condition2 }, null));
		}

		[Test]
		public void Condition_V()
		{
			// <measure at:dateEnd="2022-06-20" at:geographicalAreaId="XS" at:goodsNomenclatureCode="0809100000" at:SIDGoodsNomenclature="31302" at:justificationRegulationId="R2118320" at:justificationRegulationRoleType="4" at:measureType="142" at:national="0" at:regulationId="D1304900" at:regulationRoleType="1" at:SID="3888588" at:SIDGeographicalArea="346" at:dateStart="2022-06-01" at:stoppedFlag="0" at:changeType="U">
			var condition1 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "V",
				dutyAmount = 105.0m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "002",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 0.0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 2.1m,
						dutyExpressionId = "04",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition2 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "V",
				dutyAmount = 107.1m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "001",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 0.0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true
					},
				}
			};
			var condition3 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "V",
				dutyAmount = 100.7m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "004",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 0.0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 6.4m,
						dutyExpressionId = "04",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition4 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "V",
				dutyAmount = 102.8m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "003",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 0.0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 4.3m,
						dutyExpressionId = "04",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition5 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "V",
				dutyAmount = 0.0m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "006",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 0.0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 22.7m,
						dutyExpressionId = "04",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var condition6 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "V",
				dutyAmount = 98.5m,
				dutyAmountSpecified = true,
				measurementUnitCode = "DTN",
				monetaryUnitCode = "EUR",
				sequenceNumber = "005",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 0.0m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true
					},
					new measureConditionComponent {
						dutyAmount = 8.6m,
						dutyExpressionId = "04",
						dutyAmountSpecified = true,
						measurementUnitCode = "DTN",
						monetaryUnitCode = "EUR"
					}
				}
			};
			var creator = new ConditionDutyFormulaCreator();
			Assert.AreEqual("If(VFD/[DTN] >= 107.1, 0, If(VFD/[DTN] >= 105.0, 0 + 2.1 * [DTN], If(VFD/[DTN] >= 102.8, 0 + 4.3 * [DTN], If(VFD/[DTN] >= 100.7, 0 + 6.4 * [DTN], If(VFD/[DTN] >= 98.5, 0 + 8.6 * [DTN], 0 + 22.7 * [DTN])))))",
creator.Get("V", new[] { condition1, condition2, condition3, condition4, condition5, condition6 }, null));
		}

		[Test]
		public void Condition_A()
		{
			var condition1 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "A",
				national = 0L,
				sequenceNumber = "002",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 71.9m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true
					}
				}
			};
			var condition2 = new measureCondition
			{
				actionCode = "01",
				conditionCodeId = "A",
				certificateCode = "008",
				certificateType = "D",
				national = 0L,
				sequenceNumber = "001",
				measureConditionComponent = new[] {
					new measureConditionComponent {
						dutyAmount = 56.9m,
						dutyExpressionId = "01",
						dutyAmountSpecified = true
					}
				}
			};
			var creator = new ConditionDutyFormulaCreator();
			Assert.AreEqual("If(HAS(\"CERT\",\"D008\"), VFD * 0.569, VFD * 0.719)",
creator.Get("A", new[] { condition1, condition2 }, null));
		}
	}
}
