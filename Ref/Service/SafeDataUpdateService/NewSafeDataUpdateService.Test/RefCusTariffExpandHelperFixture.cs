using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	public class RefCusTariffExpandHelperFixture
	{
		[Test]
		public void ExpandTariff()
		{
			var tariff1 = new RefCusTariff { ZZ1_PK = tariffPK1 };
			var tariff2 = new RefCusTariff { ZZ1_PK = tariffPK2 };

			var tariffUomItem = new Mock<IExpandedItemWrapper>();
			tariffUomItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffUOM));

			var rateUomItem = new Mock<IExpandedItemWrapper>();
			rateUomItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusRateUOM));
			var exTradeItem = new Mock<IExpandedItemWrapper>();
			exTradeItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusExcludedTradeGroup));
			var appClause = new Mock<IExpandClauseWrapper>();
			appClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { exTradeItem.Object });
			var appItem = new Mock<IExpandedItemWrapper>();
			appItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusApplicability));
			appItem.Setup(x => x.GetExpandClause()).Returns(appClause.Object);
			var rateClause = new Mock<IExpandClauseWrapper>();
			rateClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { rateUomItem.Object, appItem.Object });
			var rateItem = new Mock<IExpandedItemWrapper>();
			rateItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusRate));
			rateItem.Setup(x => x.GetExpandClause()).Returns(rateClause.Object);

			var conditionItem = new Mock<IExpandedItemWrapper>();
			conditionItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusCondition));
			var conditionValueItem = new Mock<IExpandedItemWrapper>();
			conditionValueItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusConditionValue));
			var conditionLanItem = new Mock<IExpandedItemWrapper>();
			conditionLanItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusConditionLanguage));
			var conditionClause = new Mock<IExpandClauseWrapper>();
			conditionClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] {
				conditionValueItem.Object, conditionLanItem.Object, appItem.Object });
			conditionItem.Setup(x => x.GetExpandClause()).Returns(conditionClause.Object);

			var vatItem = new Mock<IExpandedItemWrapper>();
			vatItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusVATApplicability));

			var additionalCodeItem = new Mock<IExpandedItemWrapper>();
			additionalCodeItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffAdditionalCode));
			var addCodeLanItem = new Mock<IExpandedItemWrapper>();
			addCodeLanItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffAdditionalCodeLanguage));
			var additionCodeClause = new Mock<IExpandClauseWrapper>();
			additionCodeClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { addCodeLanItem.Object, appItem.Object });
			additionalCodeItem.Setup(x => x.GetExpandClause()).Returns(additionCodeClause.Object);

			var relationshipItem = new Mock<IExpandedItemWrapper>();
			relationshipItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffRelationship));

			var tariffAttributeItem = new Mock<IExpandedItemWrapper>();
			tariffAttributeItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffAttribute));

			var nationalCodeItem = new Mock<IExpandedItemWrapper>();
			nationalCodeItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffNationalCode));
			var nationalCodeClause = new Mock<IExpandClauseWrapper>();
			nationalCodeClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] {
				rateItem.Object, tariffAttributeItem.Object, vatItem.Object, tariffUomItem.Object, additionalCodeItem.Object });
			nationalCodeItem.Setup(x => x.GetExpandClause()).Returns(nationalCodeClause.Object);

			var tariffLanItem = new Mock<IExpandedItemWrapper>();
			tariffLanItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffLanguage));

			var brItem = new Mock<IExpandedItemWrapper>();
			brItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffBRCharacteristic));
			var brAttributeItem = new Mock<IExpandedItemWrapper>();
			brAttributeItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffBRCharacteristicAttribute));
			var brValueItem = new Mock<IExpandedItemWrapper>();
			brValueItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffBRCharacteristicValue));
			var brClause = new Mock<IExpandClauseWrapper>();
			brClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { brAttributeItem.Object, brValueItem.Object });
			brItem.Setup(x => x.GetExpandClause()).Returns(brClause.Object);

			var expandClause = new Mock<IExpandClauseWrapper>();
			expandClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] {
				tariffUomItem.Object, rateItem.Object, conditionItem.Object, vatItem.Object, additionalCodeItem.Object,
				relationshipItem.Object, tariffAttributeItem.Object, nationalCodeItem.Object, tariffLanItem.Object, brItem.Object });

			var repo = GetRepo();
			repo.Object.ExpandTariff(new[] { tariff1, tariff2 }, expandClause.Object);
			Assert.That(tariff1.RefCusTariffUOMs, Contains.Item(tariffUom1));
			Assert.That(tariff1.RefCusRates, Contains.Item(rate1));
			Assert.That(rate1.RefCusApplicabilities, Contains.Item(app1));
			Assert.That(rate1.RefCusRateUOMs, Contains.Item(rateUom1));
			Assert.That(app1.RefCusExcludedTradeGroups, Contains.Item(exTrade1));
			Assert.That(tariff1.RefCusConditions, Contains.Item(condition1));
			Assert.That(condition1.RefCusApplicabilities, Contains.Item(app7));
			Assert.That(condition1.RefCusConditionValues, Contains.Item(conditionValue1));
			Assert.That(condition1.RefCusConditionLanguages, Contains.Item(conditionLan1));
			Assert.That(tariff1.RefCusVATApplicabilities, Contains.Item(vat1));
			Assert.That(tariff1.RefCusTariffAdditionalCodes, Contains.Item(additionalCode1));
			Assert.That(additionalCode1.RefCusTariffAdditionalCodeLanguages, Contains.Item(additionalCodeLanguage1));
			Assert.That(additionalCode1.RefCusApplicabilities, Contains.Item(app5));
			Assert.That(tariff1.RefCusTariffRelationships, Contains.Item(relationship1));
			Assert.That(tariff1.RefCusTariffAttributes, Contains.Item(tariffAttribute1));
			Assert.That(tariff1.RefCusTariffNationalCodes, Contains.Item(nationalCode1));
			Assert.That(nationalCode1.RefCusRates, Contains.Item(rate3));
			Assert.That(nationalCode1.RefCusTariffAttributes, Contains.Item(tariffAttribute3));
			Assert.That(nationalCode1.RefCusVATApplicabilities, Contains.Item(vat3));
			Assert.That(nationalCode1.RefCusTariffUOMs, Contains.Item(tariffUom3));
			Assert.That(nationalCode1.RefCusTariffAdditionalCodes, Contains.Item(additionalCode3));
			Assert.That(tariff1.RefCusTariffLanguages, Contains.Item(tariffLan1));
			Assert.That(tariff1.RefCusTariffBRCharacteristics, Contains.Item(brCharacteristic1));
			Assert.That(brCharacteristic1.RefCusTariffBRCharacteristicAttributes, Contains.Item(brAttribute1));
			Assert.That(brCharacteristic1.RefCusTariffBRCharacteristicValues, Contains.Item(brValue1));

			Assert.That(tariff2.RefCusTariffUOMs, Contains.Item(tariffUom2));
			Assert.That(tariff2.RefCusRates, Contains.Item(rate2));
			Assert.That(rate2.RefCusApplicabilities, Contains.Item(app2));
			Assert.That(rate2.RefCusRateUOMs, Contains.Item(rateUom2));
			Assert.That(app2.RefCusExcludedTradeGroups, Contains.Item(exTrade2));
			Assert.That(tariff2.RefCusConditions, Contains.Item(condition2));
			Assert.That(condition2.RefCusApplicabilities, Contains.Item(app8));
			Assert.That(condition2.RefCusConditionValues, Contains.Item(conditionValue2));
			Assert.That(condition2.RefCusConditionLanguages, Contains.Item(conditionLan2));
			Assert.That(tariff2.RefCusVATApplicabilities, Contains.Item(vat2));
			Assert.That(tariff2.RefCusTariffAdditionalCodes, Contains.Item(additionalCode2));
			Assert.That(additionalCode2.RefCusTariffAdditionalCodeLanguages, Contains.Item(additionalCodeLanguage2));
			Assert.That(additionalCode2.RefCusApplicabilities, Contains.Item(app6));
			Assert.That(tariff2.RefCusTariffRelationships, Contains.Item(relationship2));
			Assert.That(tariff2.RefCusTariffAttributes, Contains.Item(tariffAttribute2));
			Assert.That(tariff2.RefCusTariffNationalCodes, Contains.Item(nationalCode2));
			Assert.That(nationalCode2.RefCusRates, Contains.Item(rate4));
			Assert.That(nationalCode2.RefCusTariffAttributes, Contains.Item(tariffAttribute4));
			Assert.That(nationalCode2.RefCusVATApplicabilities, Contains.Item(vat4));
			Assert.That(nationalCode2.RefCusTariffUOMs, Contains.Item(tariffUom4));
			Assert.That(nationalCode2.RefCusTariffAdditionalCodes, Contains.Item(additionalCode4));
			Assert.That(tariff2.RefCusTariffLanguages, Contains.Item(tariffLan2));
			Assert.That(tariff2.RefCusTariffBRCharacteristics, Contains.Item(brCharacteristic2));
			Assert.That(brCharacteristic2.RefCusTariffBRCharacteristicAttributes, Contains.Item(brAttribute2));
			Assert.That(brCharacteristic2.RefCusTariffBRCharacteristicValues, Contains.Item(brValue2));
		}

		[Test]
		public void ExpandTariffWithExpandClause()
		{
			var tariff1 = new RefCusTariff { ZZ1_PK = new Guid("00000000-0000-0000-0000-000000000001") };

			var tariffUomItem = new Mock<IExpandedItemWrapper>();
			tariffUomItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffUOM));
			var rateUomItem = new Mock<IExpandedItemWrapper>();
			rateUomItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusRateUOM));
			var exTradeItem = new Mock<IExpandedItemWrapper>();
			exTradeItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusExcludedTradeGroup));
			var appClause = new Mock<IExpandClauseWrapper>();
			appClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { exTradeItem.Object });
			var appItem = new Mock<IExpandedItemWrapper>();
			appItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusApplicability));
			appItem.Setup(x => x.GetExpandClause()).Returns(appClause.Object);
			var rateClause = new Mock<IExpandClauseWrapper>();
			rateClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { rateUomItem.Object, appItem.Object });
			var rateItem = new Mock<IExpandedItemWrapper>();
			rateItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusRate));
			rateItem.Setup(x => x.GetExpandClause()).Returns(rateClause.Object);
			var expandClause = new Mock<IExpandClauseWrapper>();
			expandClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { tariffUomItem.Object, rateItem.Object });

			var repo = GetRepo();
			repo.Object.ExpandTariff(new[] { tariff1 }, expandClause.Object);
			Assert.That(tariff1.RefCusTariffUOMs, Contains.Item(tariffUom1));
			Assert.That(tariff1.RefCusRates, Contains.Item(rate1));
			Assert.That(rate1.RefCusApplicabilities, Contains.Item(app1));
			Assert.That(rate1.RefCusRateUOMs, Contains.Item(rateUom1));
			Assert.That(app1.RefCusExcludedTradeGroups, Contains.Item(exTrade1));
			Assert.That(tariff1.RefCusConditions, Is.Empty);
			Assert.That(tariff1.RefCusVATApplicabilities, Is.Empty);
			Assert.That(tariff1.RefCusTariffAdditionalCodes, Is.Empty);
			Assert.That(tariff1.RefCusTariffRelationships, Is.Empty);
			Assert.That(tariff1.RefCusTariffAttributes, Is.Empty);
			Assert.That(tariff1.RefCusTariffNationalCodes, Is.Empty);
			Assert.That(tariff1.RefCusTariffLanguages, Is.Empty);
			Assert.That(tariff1.RefCusTariffBRCharacteristics, Is.Empty);
		}

		[Test]
		public void AllChildDataRetrieved()
		{
			var repo = new Mock<IReferenceDataRepository>();
			var tariff1 = new RefCusTariff { ZZ1_PK = tariffPK1 };
			var expandClause = new Mock<IExpandClauseWrapper>();
			repo.Object.ExpandTariff(new[] { tariff1 }, expandClause.Object);

			var tariffDataSet = DataSetStructureProvider.StructuredDataSets.FirstOrDefault(x => x.Contains("RefCusTariff"));
			var childTableNames = tariffDataSet.Where(x => !x.Equals("RefCusTariff", StringComparison.Ordinal));
			foreach (var childTableName in childTableNames)
			{
				var relatedType = typeof(RefCusTariff).Assembly.GetType("CargoWise.RefDbRepo.Service.Schema_0_9_New." + childTableName);
				var expressionToVerify = this.InvokeGenericMethod(nameof(GetExpressionToVerify), relatedType) as Expression<Action<IReferenceDataRepository>>;
				repo.Verify(expressionToVerify);
			}
		}

		Expression<Action<IReferenceDataRepository>> GetExpressionToVerify<TChild>()
		{
			var parameterExp = Expression.Parameter(typeof(IReferenceDataRepository), "x");
			var genericMethod = typeof(IReferenceDataRepository).GetMethod("Get");
			var method = genericMethod.MakeGenericMethod(new Type[] { typeof(TChild) });
			var callExp = Expression.Call(parameterExp, method);
			return Expression.Lambda<Action<IReferenceDataRepository>>(callExp, new ParameterExpression[] { parameterExp });
		}

		Mock<IReferenceDataRepository> GetRepo()
		{
			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefCusTariffNationalCode>()).Returns(new[] { nationalCode1, nationalCode2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffUOM>()).Returns(new[] { tariffUom1, tariffUom2, tariffUom3, tariffUom4 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusRate>()).Returns(new[] { rate1, rate2, rate3, rate4 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusRateUOM>()).Returns(new[] { rateUom1, rateUom2, rateUom3, rateUom4 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusVATApplicability>()).Returns(new[] { vat1, vat2, vat3, vat4 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffAdditionalCode>()).Returns(new[] { additionalCode1, additionalCode2, additionalCode3, additionalCode4 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffAdditionalCodeLanguage>()).Returns(new[] { additionalCodeLanguage1, additionalCodeLanguage2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffRelationship>()).Returns(new[] { relationship1, relationship2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffAttribute>()).Returns(new[] { tariffAttribute1, tariffAttribute2, tariffAttribute3, tariffAttribute4 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusCondition>()).Returns(new[] { condition1, condition2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusConditionValue>()).Returns(new[] { conditionValue1, conditionValue2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusConditionLanguage>()).Returns(new[] { conditionLan1, conditionLan2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffLanguage>()).Returns(new[] { tariffLan1, tariffLan2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffBRCharacteristic>()).Returns(new[] { brCharacteristic1, brCharacteristic2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffBRCharacteristicAttribute>()).Returns(new[] { brAttribute1, brAttribute2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariffBRCharacteristicValue>()).Returns(new[] { brValue1, brValue2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusApplicability>()).Returns(new[] { app1, app2, app3, app4, app5, app6, app7, app8 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusExcludedTradeGroup>()).Returns(new[] { exTrade1, exTrade2, exTrade3, exTrade4, exTrade5, exTrade6, exTrade7, exTrade8 }.AsQueryable());
			return repo;
		}

		[SetUp]
		public void SetUp()
		{
			nationalCode1 = new RefCusTariffNationalCode { ZZW_PK = Guid.NewGuid(), ZZW_ZZ1_Tariff = tariffPK1 };
			nationalCode2 = new RefCusTariffNationalCode { ZZW_PK = Guid.NewGuid(), ZZW_ZZ1_Tariff = tariffPK2 };

			tariffUom1 = new RefCusTariffUOM { ZZ8_PK = Guid.NewGuid(), ZZ8_DataSetPK = tariffPK1, ZZ8_ZZ1_Tariff = tariffPK1 };
			tariffUom2 = new RefCusTariffUOM { ZZ8_PK = Guid.NewGuid(), ZZ8_DataSetPK = tariffPK2, ZZ8_ZZ1_Tariff = tariffPK2 };
			tariffUom3 = new RefCusTariffUOM { ZZ8_PK = Guid.NewGuid(), ZZ8_DataSetPK = tariffPK1, ZZ8_ZZW_TariffNationalCode = nationalCode1.ZZW_PK };
			tariffUom4 = new RefCusTariffUOM { ZZ8_PK = Guid.NewGuid(), ZZ8_DataSetPK = tariffPK2, ZZ8_ZZW_TariffNationalCode = nationalCode2.ZZW_PK };

			rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_DataSetPK = tariffPK1, ZZ2_ZZ1_Tariff = tariffPK1 };
			rate2 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_DataSetPK = tariffPK2, ZZ2_ZZ1_Tariff = tariffPK2 };
			rate3 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_DataSetPK = tariffPK1, ZZ2_ZZW_TariffNationalCode = nationalCode1.ZZW_PK };
			rate4 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_DataSetPK = tariffPK2, ZZ2_ZZW_TariffNationalCode = nationalCode2.ZZW_PK };

			rateUom1 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid(), ZXG_DataSetPK = tariffPK1, ZXG_ZZ2_Rate = rate1.ZZ2_PK };
			rateUom2 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid(), ZXG_DataSetPK = tariffPK2, ZXG_ZZ2_Rate = rate2.ZZ2_PK };
			rateUom3 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid(), ZXG_DataSetPK = tariffPK1, ZXG_ZZ2_Rate = rate3.ZZ2_PK };
			rateUom4 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid(), ZXG_DataSetPK = tariffPK2, ZXG_ZZ2_Rate = rate4.ZZ2_PK };

			vat1 = new RefCusVATApplicability { ZX5_PK = Guid.NewGuid(), ZX5_DataSetPK = tariffPK1, ZX5_ZZ1_Tariff = tariffPK1 };
			vat2 = new RefCusVATApplicability { ZX5_PK = Guid.NewGuid(), ZX5_DataSetPK = tariffPK2, ZX5_ZZ1_Tariff = tariffPK2 };
			vat3 = new RefCusVATApplicability { ZX5_PK = Guid.NewGuid(), ZX5_DataSetPK = tariffPK1, ZX5_ZZW_TariffNationalCode = nationalCode1.ZZW_PK };
			vat4 = new RefCusVATApplicability { ZX5_PK = Guid.NewGuid(), ZX5_DataSetPK = tariffPK2, ZX5_ZZW_TariffNationalCode = nationalCode2.ZZW_PK };

			additionalCode1 = new RefCusTariffAdditionalCode { ZY2_PK = Guid.NewGuid(), ZY2_DataSetPK = tariffPK1, ZY2_ZZ1_Tariff = tariffPK1 };
			additionalCode2 = new RefCusTariffAdditionalCode { ZY2_PK = Guid.NewGuid(), ZY2_DataSetPK = tariffPK2, ZY2_ZZ1_Tariff = tariffPK2 };
			additionalCode3 = new RefCusTariffAdditionalCode { ZY2_PK = Guid.NewGuid(), ZY2_DataSetPK = tariffPK1, ZY2_ZZW_NationalCode = nationalCode1.ZZW_PK };
			additionalCode4 = new RefCusTariffAdditionalCode { ZY2_PK = Guid.NewGuid(), ZY2_DataSetPK = tariffPK2, ZY2_ZZW_NationalCode = nationalCode2.ZZW_PK };

			additionalCodeLanguage1 = new RefCusTariffAdditionalCodeLanguage { ZY4_PK = Guid.NewGuid(), ZY4_DataSetPK = tariffPK1, ZY4_ZY2_TariffAdditionalCode = additionalCode1.ZY2_PK };
			additionalCodeLanguage2 = new RefCusTariffAdditionalCodeLanguage { ZY4_PK = Guid.NewGuid(), ZY4_DataSetPK = tariffPK2, ZY4_ZY2_TariffAdditionalCode = additionalCode2.ZY2_PK };

			relationship1 = new RefCusTariffRelationship { ZZH_PK = Guid.NewGuid(), ZZH_ZZ1_Tariff = tariffPK1, ZZH_DataSetPK = tariffPK1 };
			relationship2 = new RefCusTariffRelationship { ZZH_PK = Guid.NewGuid(), ZZH_ZZ1_Tariff = tariffPK2, ZZH_DataSetPK = tariffPK2 };

			tariffAttribute1 = new RefCusTariffAttribute { ZZ3_PK = Guid.NewGuid(), ZZ3_DataSetPK = tariffPK1, ZZ3_ZZ1_Tariff = tariffPK1 };
			tariffAttribute2 = new RefCusTariffAttribute { ZZ3_PK = Guid.NewGuid(), ZZ3_DataSetPK = tariffPK2, ZZ3_ZZ1_Tariff = tariffPK2 };
			tariffAttribute3 = new RefCusTariffAttribute { ZZ3_PK = Guid.NewGuid(), ZZ3_DataSetPK = tariffPK1, ZZ3_ZZW_TariffNationalCode = nationalCode1.ZZW_PK };
			tariffAttribute4 = new RefCusTariffAttribute { ZZ3_PK = Guid.NewGuid(), ZZ3_DataSetPK = tariffPK2, ZZ3_ZZW_TariffNationalCode = nationalCode2.ZZW_PK };

			condition1 = new RefCusCondition { ZX1_PK = Guid.NewGuid(), ZX1_DataSetPK = tariffPK1 };
			condition2 = new RefCusCondition { ZX1_PK = Guid.NewGuid(), ZX1_DataSetPK = tariffPK2 };

			conditionValue1 = new RefCusConditionValue { ZX3_PK = Guid.NewGuid(), ZX3_DataSetPK = tariffPK1, ZX3_ZX1_Condition = condition1.ZX1_PK };
			conditionValue2 = new RefCusConditionValue { ZX3_PK = Guid.NewGuid(), ZX3_DataSetPK = tariffPK2, ZX3_ZX1_Condition = condition2.ZX1_PK };

			conditionLan1 = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid(), ZXJ_DataSetPK = tariffPK1, ZXJ_ZX1_Condition = condition1.ZX1_PK };
			conditionLan2 = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid(), ZXJ_DataSetPK = tariffPK2, ZXJ_ZX1_Condition = condition2.ZX1_PK };

			tariffLan1 = new RefCusTariffLanguage { ZX7_PK = Guid.NewGuid(), ZX7_ZZ1_Tariff = tariffPK1 };
			tariffLan2 = new RefCusTariffLanguage { ZX7_PK = Guid.NewGuid(), ZX7_ZZ1_Tariff = tariffPK2 };

			brCharacteristic1 = new RefCusTariffBRCharacteristic { ZB1_PK = Guid.NewGuid(), ZB1_DataSetPK = tariffPK1 };
			brCharacteristic2 = new RefCusTariffBRCharacteristic { ZB1_PK = Guid.NewGuid(), ZB1_DataSetPK = tariffPK2 };

			brAttribute1 = new RefCusTariffBRCharacteristicAttribute { ZB3_PK = Guid.NewGuid(), ZB3_DataSetPK = tariffPK1, ZB3_ZB1_Characteristic = brCharacteristic1.ZB1_PK };
			brAttribute2 = new RefCusTariffBRCharacteristicAttribute { ZB3_PK = Guid.NewGuid(), ZB3_DataSetPK = tariffPK2, ZB3_ZB1_Characteristic = brCharacteristic2.ZB1_PK };

			brValue1 = new RefCusTariffBRCharacteristicValue { ZB2_PK = Guid.NewGuid(), ZB2_DataSetPK = tariffPK1, ZB2_ZB1_Characteristic = brCharacteristic1.ZB1_PK };
			brValue2 = new RefCusTariffBRCharacteristicValue { ZB2_PK = Guid.NewGuid(), ZB2_DataSetPK = tariffPK2, ZB2_ZB1_Characteristic = brCharacteristic2.ZB1_PK };

			app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_DataSetPK = tariffPK1, ZZT_ZZ2_Rate = rate1.ZZ2_PK };
			app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_DataSetPK = tariffPK2, ZZT_ZZ2_Rate = rate2.ZZ2_PK };
			app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_DataSetPK = tariffPK1, ZZT_ZZ2_Rate = rate3.ZZ2_PK };
			app4 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_DataSetPK = tariffPK2, ZZT_ZZ2_Rate = rate4.ZZ2_PK };
			app5 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_DataSetPK = tariffPK1, ZZT_ZY2_AdditionalCode = additionalCode1.ZY2_PK };
			app6 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_DataSetPK = tariffPK2, ZZT_ZY2_AdditionalCode = additionalCode2.ZY2_PK };
			app7 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_DataSetPK = tariffPK1, ZZT_ZX1_Conditions = condition1.ZX1_PK };
			app8 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_DataSetPK = tariffPK2, ZZT_ZX1_Conditions = condition2.ZX1_PK };

			exTrade1 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), ZZC_DataSetPK = tariffPK1, ZZC_ZZT_Applicability = app1.ZZT_PK };
			exTrade2 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), ZZC_DataSetPK = tariffPK2, ZZC_ZZT_Applicability = app2.ZZT_PK };
			exTrade3 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), ZZC_DataSetPK = tariffPK2, ZZC_ZZT_Applicability = app3.ZZT_PK };
			exTrade4 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), ZZC_DataSetPK = tariffPK2, ZZC_ZZT_Applicability = app4.ZZT_PK };
			exTrade5 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), ZZC_DataSetPK = tariffPK2, ZZC_ZZT_Applicability = app5.ZZT_PK };
			exTrade6 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), ZZC_DataSetPK = tariffPK2, ZZC_ZZT_Applicability = app6.ZZT_PK };
			exTrade7 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), ZZC_DataSetPK = tariffPK1, ZZC_ZZT_Applicability = app7.ZZT_PK };
			exTrade8 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), ZZC_DataSetPK = tariffPK2, ZZC_ZZT_Applicability = app8.ZZT_PK };
		}

		Guid tariffPK1 = new Guid("00000000-0000-0000-0000-000000000001");
		Guid tariffPK2 = new Guid("00000000-0000-0000-0000-000000000002");
		RefCusTariffNationalCode nationalCode1;
		RefCusTariffNationalCode nationalCode2;
		RefCusTariffUOM tariffUom1;
		RefCusTariffUOM tariffUom2;
		RefCusTariffUOM tariffUom3;
		RefCusTariffUOM tariffUom4;
		RefCusRate rate1;
		RefCusRate rate2;
		RefCusRate rate3;
		RefCusRate rate4;
		RefCusRateUOM rateUom1;
		RefCusRateUOM rateUom2;
		RefCusRateUOM rateUom3;
		RefCusRateUOM rateUom4;
		RefCusVATApplicability vat1;
		RefCusVATApplicability vat2;
		RefCusVATApplicability vat3;
		RefCusVATApplicability vat4;
		RefCusTariffAdditionalCode additionalCode1;
		RefCusTariffAdditionalCode additionalCode2;
		RefCusTariffAdditionalCode additionalCode3;
		RefCusTariffAdditionalCode additionalCode4;
		RefCusTariffAdditionalCodeLanguage additionalCodeLanguage1;
		RefCusTariffAdditionalCodeLanguage additionalCodeLanguage2;
		RefCusTariffRelationship relationship1;
		RefCusTariffRelationship relationship2;
		RefCusTariffAttribute tariffAttribute1;
		RefCusTariffAttribute tariffAttribute2;
		RefCusTariffAttribute tariffAttribute3;
		RefCusTariffAttribute tariffAttribute4;
		RefCusCondition condition1;
		RefCusCondition condition2;
		RefCusConditionValue conditionValue1;
		RefCusConditionValue conditionValue2;
		RefCusConditionLanguage conditionLan1;
		RefCusConditionLanguage conditionLan2;
		RefCusTariffLanguage tariffLan1;
		RefCusTariffLanguage tariffLan2;
		RefCusTariffBRCharacteristic brCharacteristic1;
		RefCusTariffBRCharacteristic brCharacteristic2;
		RefCusTariffBRCharacteristicAttribute brAttribute1;
		RefCusTariffBRCharacteristicAttribute brAttribute2;
		RefCusTariffBRCharacteristicValue brValue1;
		RefCusTariffBRCharacteristicValue brValue2;
		RefCusApplicability app1;
		RefCusApplicability app2;
		RefCusApplicability app3;
		RefCusApplicability app4;
		RefCusApplicability app5;
		RefCusApplicability app6;
		RefCusApplicability app7;
		RefCusApplicability app8;
		RefCusExcludedTradeGroup exTrade1;
		RefCusExcludedTradeGroup exTrade2;
		RefCusExcludedTradeGroup exTrade3;
		RefCusExcludedTradeGroup exTrade4;
		RefCusExcludedTradeGroup exTrade5;
		RefCusExcludedTradeGroup exTrade6;
		RefCusExcludedTradeGroup exTrade7;
		RefCusExcludedTradeGroup exTrade8;
	}
}
