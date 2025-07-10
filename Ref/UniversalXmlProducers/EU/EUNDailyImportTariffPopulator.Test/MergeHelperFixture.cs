using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator.Test
{
	[TestFixture]
	public class MergeHelperFixture
	{
		[Test]
		public void MergeImportRates()
		{
			var dailyTariffWithRatesOnDayOne =
				new RefCusTariff
				{
					ZZ1_TariffCode = "001",
					ZZ1_Description = "Tariff1",
					RefCusRates = new RefCusRate[]
					{
						new RefCusRate
						{
							ZZ2_StartDate = new DateTime(),
							ZZ2_EndDate = new DateTime(),
							ZZ2_RateFormula = "4*1",
							ZZ2_ZY1_NKRateCode = "RC1",
							ZZ2_ZY1_ZZR_NKRateType = "RT1",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="1011"
								}
							}
						},
						new RefCusRate
						{
							ZZ2_StartDate = new DateTime(),
							ZZ2_EndDate = new DateTime(),
							ZZ2_RateFormula = "2*1",
							ZZ2_ZY1_NKRateCode = "RC2",
							ZZ2_ZY1_ZZR_NKRateType = "RT2",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="GB"
								}
							}
						}
					}
				};

			var incomingRatesDayTwo = new RefCusRate[]
					{
						new RefCusRate
						{
							ZZ2_StartDate = new DateTime(),
							ZZ2_EndDate = new DateTime(),
							ZZ2_RateFormula = "7*1",
							ZZ2_ZY1_NKRateCode = "RC2",
							ZZ2_ZY1_ZZR_NKRateType = "RT2",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="GB"
								}
							}
						}
					};

			var incomingRatesAsToday = new RefCusRate[]
					{
						new RefCusRate
						{
							ZZ2_StartDate = new DateTime(),
							ZZ2_EndDate = new DateTime(),
							ZZ2_RateFormula = "3*1",
							ZZ2_ZY1_NKRateCode = "RC3",
							ZZ2_ZY1_ZZR_NKRateType = "RT3",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="C874",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="KR"
								}
							}
						}
					};

			MergeHelper.MergeRates(dailyTariffWithRatesOnDayOne, incomingRatesDayTwo);
			MergeHelper.MergeRates(dailyTariffWithRatesOnDayOne, incomingRatesAsToday);

			Assert.That(dailyTariffWithRatesOnDayOne.RefCusRates.Length, Is.EqualTo(3));

			//rate rc1
			var rc1Rate = dailyTariffWithRatesOnDayOne.RefCusRates.FirstOrDefault(x => x.ZZ2_ZY1_NKRateCode == "RC1");
			Assert.IsNotNull(rc1Rate);
			Assert.That(rc1Rate.ZZ2_RateFormula, Is.EqualTo("4*1"));

			//rate rc2
			var rc2Rate = dailyTariffWithRatesOnDayOne.RefCusRates.FirstOrDefault(x => x.ZZ2_ZY1_NKRateCode == "RC2");
			Assert.IsNotNull(rc2Rate);
			Assert.That(rc2Rate.ZZ2_RateFormula, Is.EqualTo("7*1"));

			//rate rc3
			var rc3Rate = dailyTariffWithRatesOnDayOne.RefCusRates.FirstOrDefault(x => x.ZZ2_ZY1_NKRateCode == "RC3");
			Assert.IsNotNull(rc3Rate);
			Assert.That(rc3Rate.ZZ2_RateFormula, Is.EqualTo("3*1"));
		}

		[Test]
		public void MergeRatesWithException()
		{
			var dailyTariffWithRate1 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "Tariff1",
				RefCusRates = new RefCusRate[]
				{
					new RefCusRate
					{
						ZZ2_StartDate = new DateTime(),
						ZZ2_EndDate = new DateTime(),
						ZZ2_RateFormula = "3*1",
						ZZ2_ZY1_NKRateCode = "RC3",
						ZZ2_ZY1_ZZR_NKRateType = "RT3"
					}
				}
			};

			var dailyTariffWithRate2 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "Tariff1",
				RefCusRates = new RefCusRate[]
				{
					new RefCusRate
					{
						ZZ2_StartDate = new DateTime(),
						ZZ2_EndDate = new DateTime(),
						ZZ2_RateFormula = "3*1",
						ZZ2_ZY1_NKRateCode = "RC3",
						ZZ2_ZY1_ZZR_NKRateType = "RT3",
						RefCusApplicabilities= new RefCusApplicability[]
						{
							new RefCusApplicability
							{
								ZZT_AdditionalCode="",
								ZZT_OrderNumber="",
								ZZT_StartDate=new DateTime(),
								ZZT_ZZA_NKTradeGroup="1011"
							},
							new RefCusApplicability
							{
								ZZT_AdditionalCode="",
								ZZT_OrderNumber="",
								ZZT_StartDate=new DateTime(),
								ZZT_ZZA_NKTradeGroup="1012"
							}
						}
					}
				}
			};

			var dailyTariffWithRate3 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "Tariff1",
				RefCusRates = new RefCusRate[]
				{
					new RefCusRate
					{
						ZZ2_StartDate = new DateTime(),
						ZZ2_EndDate = new DateTime(),
						ZZ2_RateFormula = "3*1",
						ZZ2_ZY1_NKRateCode = "RC3",
						ZZ2_ZY1_ZZR_NKRateType = "RT3",
						RefCusApplicabilities= new RefCusApplicability[]
						{
							new RefCusApplicability
							{
								ZZT_AdditionalCode="",
								ZZT_OrderNumber="",
								ZZT_StartDate=new DateTime(),
								ZZT_ZZA_NKTradeGroup="1011"
							}
						}
					}
				}
			};

			var incomingRate1 = new RefCusRate
			{
				ZZ2_StartDate = new DateTime(),
				ZZ2_EndDate = new DateTime(),
				ZZ2_RateFormula = "3*1",
				ZZ2_ZY1_NKRateCode = "RC3",
				ZZ2_ZY1_ZZR_NKRateType = "RT3",
			};

			var incomingRate2 = new RefCusRate
			{
				ZZ2_StartDate = new DateTime(),
				ZZ2_EndDate = new DateTime(),
				ZZ2_RateFormula = "3*1",
				ZZ2_ZY1_NKRateCode = "RC3",
				ZZ2_ZY1_ZZR_NKRateType = "RT3",
				RefCusApplicabilities = new RefCusApplicability[]
				{
					new RefCusApplicability
					{
						ZZT_AdditionalCode="C874",
						ZZT_OrderNumber="",
						ZZT_StartDate=new DateTime(),
						ZZT_ZZA_NKTradeGroup="KR"
					},
					new RefCusApplicability
					{
						ZZT_AdditionalCode="C875",
						ZZT_OrderNumber="",
						ZZT_StartDate=new DateTime(),
						ZZT_ZZA_NKTradeGroup="KR"
					}
				}
			};

			var incomingRate3 = new RefCusRate
			{
				ZZ2_StartDate = new DateTime(),
				ZZ2_EndDate = new DateTime(),
				ZZ2_RateFormula = "3*1",
				ZZ2_ZY1_NKRateCode = "RC3",
				ZZ2_ZY1_ZZR_NKRateType = "RT3",
				RefCusApplicabilities = new RefCusApplicability[]
				{
					new RefCusApplicability
					{
						ZZT_AdditionalCode="C874",
						ZZT_OrderNumber="",
						ZZT_StartDate=new DateTime(),
						ZZT_ZZA_NKTradeGroup="KR"
					}
				}
			};

			Assert.Throws(typeof(NotSupportedException), () => MergeHelper.MergeRates(dailyTariffWithRate1, new RefCusRate[] { incomingRate3 }));
			Assert.Throws(typeof(NotSupportedException), () => MergeHelper.MergeRates(dailyTariffWithRate2, new RefCusRate[] { incomingRate3 }));
			Assert.Throws(typeof(NotSupportedException), () => MergeHelper.MergeRates(dailyTariffWithRate3, new RefCusRate[] { incomingRate1 }));
			Assert.Throws(typeof(NotSupportedException), () => MergeHelper.MergeRates(dailyTariffWithRate3, new RefCusRate[] { incomingRate2 }));
			Assert.DoesNotThrow(() => MergeHelper.MergeRates(dailyTariffWithRate3, new RefCusRate[] { incomingRate3 }));
		}

		[Test]
		public void MergeImportConditions()
		{
			var dailyTariffWithConditionsOnDayOne =
				new RefCusTariff
				{
					ZZ1_TariffCode = "001",
					ZZ1_Description = "Tariff1",
					RefCusConditions = new[]
					{
						new RefCusCondition
						{
							ZX1_StartDate = new DateTime(),
							ZX1_EndDate = new DateTime(),
							ZX1_Comment = "CMT 1",
							ZX1_ZX2_NKConditionType = "CT1",
							ZX1_IsImport = true,
							ZX1_Source = "SRC 1",
							ZX1_LogicalANDWithinGroup = 1,
							ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZS_NKPreference = "P1",
							ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZZ_NKDataGrouping = "ZA",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="1011"
								}
							}
						},
						new RefCusCondition
						{
							ZX1_StartDate = new DateTime(),
							ZX1_EndDate = new DateTime(),
							ZX1_Comment = "CMT 2",
							ZX1_ZX2_NKConditionType = "CT2",
							ZX1_IsImport = true,
							ZX1_Source = "SRC 2",
							ZX1_LogicalANDWithinGroup = 1,
							ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZS_NKPreference = "P1",
							ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZZ_NKDataGrouping = "ZA",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="GB"
								}
							}
						}
					}
				};

			var incomingConditionsDayTwo = new[]
					{
						new RefCusCondition
						{
							ZX1_StartDate = new DateTime(),
							ZX1_EndDate = new DateTime(),
							ZX1_Comment = "CMT 2",
							ZX1_ZX2_NKConditionType = "CT2",
							ZX1_IsImport = true,
							ZX1_Source = "SRC 2 Update",
							ZX1_LogicalANDWithinGroup = 1,
							ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZS_NKPreference = "P1",
							ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZZ_NKDataGrouping = "ZA",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="GB"
								}
							},
							RefCusConditionValues = new[]
							{
								new RefCusConditionValue
								{
									ZX3_Value = "V2",
									ZX3_ZX4_NKValueType = "VT2",
									ZX3_LogicalORWithinGroup = 1
								}
							}
						},
						new RefCusCondition
						{
							ZX1_StartDate = new DateTime(),
							ZX1_EndDate = new DateTime(),
							ZX1_Comment = "CMT 1",
							ZX1_ZX2_NKConditionType = "CT1",
							ZX1_IsImport = true,
							ZX1_Source = "SRC 1 Update",
							ZX1_LogicalANDWithinGroup = 1,
							ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZS_NKPreference = "P1",
							ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZZ_NKDataGrouping = "ZA",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="1011"
								}
							}
						},
						new RefCusCondition
						{
							ZX1_StartDate = new DateTime(),
							ZX1_EndDate = new DateTime(),
							ZX1_Comment = "CMT 3",
							ZX1_ZX2_NKConditionType = "CT3",
							ZX1_IsImport = true,
							ZX1_Source = "SRC 3",
							ZX1_LogicalANDWithinGroup = 1,
							ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZS_NKPreference = "P1",
							ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZZ_NKDataGrouping = "ZA",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="KR"
								}
							}
						},
					};

			var incomingConditionsAsToday = new[]
					{
						new RefCusCondition
						{
							ZX1_StartDate = new DateTime(),
							ZX1_EndDate = new DateTime(),
							ZX1_Comment = "CMT 2",
							ZX1_ZX2_NKConditionType = "CT2",
							ZX1_IsImport = true,
							ZX1_Source = "SRC 2 Today",
							ZX1_LogicalANDWithinGroup = 1,
							ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZS_NKPreference = "P1",
							ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
							ZX1_ZZZ_NKDataGrouping = "ZA",
							RefCusApplicabilities= new RefCusApplicability[]
							{
								new RefCusApplicability
								{
									ZZT_AdditionalCode="",
									ZZT_OrderNumber="",
									ZZT_StartDate=new DateTime(),
									ZZT_ZZA_NKTradeGroup="GB"
								}
							},
							RefCusConditionValues = new[]
							{
								new RefCusConditionValue
								{
									ZX3_Value = "V2 New",
									ZX3_ZX4_NKValueType = "VT2 New",
									ZX3_LogicalORWithinGroup = 1
								}
							}
						}
					};

			MergeHelper.MergeConditions(dailyTariffWithConditionsOnDayOne, incomingConditionsDayTwo);
			MergeHelper.MergeConditions(dailyTariffWithConditionsOnDayOne, incomingConditionsAsToday);

			Assert.That(dailyTariffWithConditionsOnDayOne.RefCusConditions.Length, Is.EqualTo(3));

			//condition CT1
			var ct1Condition = dailyTariffWithConditionsOnDayOne.RefCusConditions.FirstOrDefault(x => x.ZX1_ZX2_NKConditionType == "CT1");
			Assert.IsNotNull(ct1Condition);
			Assert.That(ct1Condition.ZX1_Source, Is.EqualTo("SRC 1 Update"));

			//condition CT3
			var ct3Condition = dailyTariffWithConditionsOnDayOne.RefCusConditions.FirstOrDefault(x => x.ZX1_ZX2_NKConditionType == "CT3");
			Assert.IsNotNull(ct3Condition);
			Assert.That(ct3Condition.ZX1_Source, Is.EqualTo("SRC 3"));

			//condition CT2
			var ct2Condition = dailyTariffWithConditionsOnDayOne.RefCusConditions.FirstOrDefault(x => x.ZX1_ZX2_NKConditionType == "CT2");
			Assert.IsNotNull(ct2Condition);
			Assert.That(ct2Condition.ZX1_Source, Is.EqualTo("SRC 2 Today"));

			//conditionValue for CT2
			Assert.That(ct2Condition.RefCusConditionValues, Has.Length.EqualTo(1));
			Assert.That(!ct2Condition.RefCusConditionValues.Any(x => x.ZX3_Value == "V2"));
			Assert.That(ct2Condition.RefCusConditionValues.Any(x => x.ZX3_Value == "V2 New"));
		}

		[Test]
		public void MergeConditionWithException()
		{
			var dailyTariffWithCondition1 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "Tariff1",
				RefCusConditions = new[]
				{
					new RefCusCondition
					{
						ZX1_StartDate = new DateTime(),
						ZX1_EndDate = new DateTime(),
						ZX1_Comment = "CMT 2",
						ZX1_ZX2_NKConditionType = "CT2",
						ZX1_IsImport = true,
						ZX1_Source = "SRC 2",
						ZX1_LogicalANDWithinGroup = 1,
						ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
						ZX1_ZZS_NKPreference = "P1",
						ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
						ZX1_ZZZ_NKDataGrouping = "ZA",
					}
				}
			};

			var dailyTariffWithCondition2 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "Tariff1",
				RefCusConditions = new[]
				{
					new RefCusCondition
					{
						ZX1_StartDate = new DateTime(),
						ZX1_EndDate = new DateTime(),
						ZX1_Comment = "CMT 2",
						ZX1_ZX2_NKConditionType = "CT2",
						ZX1_IsImport = true,
						ZX1_Source = "SRC 2",
						ZX1_LogicalANDWithinGroup = 1,
						ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
						ZX1_ZZS_NKPreference = "P1",
						ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
						ZX1_ZZZ_NKDataGrouping = "ZA",
						RefCusApplicabilities= new RefCusApplicability[]
						{
							new RefCusApplicability
							{
								ZZT_AdditionalCode="",
								ZZT_OrderNumber="",
								ZZT_StartDate=new DateTime(),
								ZZT_ZZA_NKTradeGroup="1011"
							},
							new RefCusApplicability
							{
								ZZT_AdditionalCode="",
								ZZT_OrderNumber="",
								ZZT_StartDate=new DateTime(),
								ZZT_ZZA_NKTradeGroup="1012"
							}
						}
					}
				}
			};

			var dailyTariffWithCondition3 = new RefCusTariff
			{
				ZZ1_TariffCode = "001",
				ZZ1_Description = "Tariff1",
				RefCusConditions = new[]
				{
					new RefCusCondition
					{
						ZX1_StartDate = new DateTime(),
						ZX1_EndDate = new DateTime(),
						ZX1_Comment = "CMT 2",
						ZX1_ZX2_NKConditionType = "CT2",
						ZX1_IsImport = true,
						ZX1_Source = "SRC 2",
						ZX1_LogicalANDWithinGroup = 1,
						ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
						ZX1_ZZS_NKPreference = "P1",
						ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
						ZX1_ZZZ_NKDataGrouping = "ZA",
						RefCusApplicabilities= new RefCusApplicability[]
						{
							new RefCusApplicability
							{
								ZZT_AdditionalCode="",
								ZZT_OrderNumber="",
								ZZT_StartDate=new DateTime(),
								ZZT_ZZA_NKTradeGroup="1011"
							}
						}
					}
				}
			};

			var incomingCondition1 = new RefCusCondition
			{
				ZX1_StartDate = new DateTime(),
				ZX1_EndDate = new DateTime(),
				ZX1_Comment = "CMT 2",
				ZX1_ZX2_NKConditionType = "CT2",
				ZX1_IsImport = true,
				ZX1_Source = "SRC 2 Today",
				ZX1_LogicalANDWithinGroup = 1,
				ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZZS_NKPreference = "P1",
				ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZZZ_NKDataGrouping = "ZA"
			};

			var incomingCondition2 = new RefCusCondition
			{
				ZX1_StartDate = new DateTime(),
				ZX1_EndDate = new DateTime(),
				ZX1_Comment = "CMT 2",
				ZX1_ZX2_NKConditionType = "CT2",
				ZX1_IsImport = true,
				ZX1_Source = "SRC 2 Today",
				ZX1_LogicalANDWithinGroup = 1,
				ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZZS_NKPreference = "P1",
				ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZZZ_NKDataGrouping = "ZA",
				RefCusApplicabilities = new RefCusApplicability[]
				{
					new RefCusApplicability
					{
						ZZT_AdditionalCode="",
						ZZT_OrderNumber="",
						ZZT_StartDate=new DateTime(),
						ZZT_ZZA_NKTradeGroup="GB"
					},
					new RefCusApplicability
					{
						ZZT_AdditionalCode="",
						ZZT_OrderNumber="",
						ZZT_StartDate=new DateTime(),
						ZZT_ZZA_NKTradeGroup="KR"
					}
				}
			};

			var incomingCondition3 = new RefCusCondition
			{
				ZX1_StartDate = new DateTime(),
				ZX1_EndDate = new DateTime(),
				ZX1_Comment = "CMT 2",
				ZX1_ZX2_NKConditionType = "CT2",
				ZX1_IsImport = true,
				ZX1_Source = "SRC 2 Today",
				ZX1_LogicalANDWithinGroup = 1,
				ZX1_ZX2_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZZS_NKPreference = "P1",
				ZX1_ZZS_ZZZ_NKDataGrouping = "ZA",
				ZX1_ZZZ_NKDataGrouping = "ZA",
				RefCusApplicabilities = new RefCusApplicability[]
				{
					new RefCusApplicability
					{
						ZZT_AdditionalCode="",
						ZZT_OrderNumber="",
						ZZT_StartDate=new DateTime(),
						ZZT_ZZA_NKTradeGroup="GB"
					}
				}
			};

			Assert.Throws(typeof(NotSupportedException), () => MergeHelper.MergeConditions(dailyTariffWithCondition1, new RefCusCondition[] { incomingCondition3 }));
			Assert.Throws(typeof(NotSupportedException), () => MergeHelper.MergeConditions(dailyTariffWithCondition2, new RefCusCondition[] { incomingCondition3 }));
			Assert.Throws(typeof(NotSupportedException), () => MergeHelper.MergeConditions(dailyTariffWithCondition3, new RefCusCondition[] { incomingCondition1 }));
			Assert.Throws(typeof(NotSupportedException), () => MergeHelper.MergeConditions(dailyTariffWithCondition3, new RefCusCondition[] { incomingCondition2 }));
			Assert.DoesNotThrow(() => MergeHelper.MergeConditions(dailyTariffWithCondition3, new RefCusCondition[] { incomingCondition3 }));
		}

		[Test]
		public void InsertTariffUOMIfNotExists()
		{
			var dailyTariffWithTariffUOM =
				new RefCusTariff
				{
					ZZ1_TariffCode = "001",
					ZZ1_Description = "Tariff1",
					RefCusTariffUOMs = new[]
					{
						TariffUOMCreator.GetDefaultTariffUOM
					}
				};

			var incomingTariffUOMs = new[]
			{
				new RefCusTariffUOM
				{
					ZZ8_Type = "TP2",
					ZZ8_UOM = "UOM",
					ZZ8_ZZA_NKTradeGroup = "CN"
				},
				TariffUOMCreator.GetDefaultTariffUOM
			};

			MergeHelper.InsertTariffUOMIfNotExists(dailyTariffWithTariffUOM, incomingTariffUOMs);

			Assert.That(dailyTariffWithTariffUOM.RefCusTariffUOMs, Has.Length.EqualTo(2));

			Assert.That(dailyTariffWithTariffUOM.RefCusTariffUOMs[0].ZZ8_Type, Is.EqualTo("CU1"));
			Assert.That(dailyTariffWithTariffUOM.RefCusTariffUOMs[0].ZZ8_UOM, Is.EqualTo("KGM"));
			Assert.IsNull(dailyTariffWithTariffUOM.RefCusTariffUOMs[0].ZZ8_ZZA_NKTradeGroup);

			Assert.That(dailyTariffWithTariffUOM.RefCusTariffUOMs[1].ZZ8_Type, Is.EqualTo("TP2"));
			Assert.That(dailyTariffWithTariffUOM.RefCusTariffUOMs[1].ZZ8_UOM, Is.EqualTo("UOM"));
			Assert.That(dailyTariffWithTariffUOM.RefCusTariffUOMs[1].ZZ8_ZZA_NKTradeGroup, Is.EqualTo("CN"));
		}
	}
}
