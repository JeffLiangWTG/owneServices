using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	public class CloneHelperFixture
	{
		[Test]
		public void Clone()
		{
			var expiredParentRecordPk = Guid.NewGuid();
			var newParentRecordPK = Guid.NewGuid();
			var languageToIgnorePk = Guid.NewGuid();
			var attributePK = Guid.NewGuid();
			var transportModePK = Guid.NewGuid();
			var dataProcessingClonePK = Guid.NewGuid();

			var languages = new RefCusCodeListLanguage[]
			{
				new RefCusCodeListLanguage
					{
						ZXA_Description = "OI",
						ZXA_PK = Guid.NewGuid(),
						ZXA_ZX6_NKLanguage = "EN",
						ZXA_ZZD_CodeList = expiredParentRecordPk
					},
				new RefCusCodeListLanguage
					{
						ZXA_Description = "OBA",
						ZXA_PK = languageToIgnorePk,
						ZXA_ZX6_NKLanguage = "PT",
						ZXA_ZZD_CodeList = expiredParentRecordPk
					}
			};
			var attribute = new RefCusCodeListAttribute
			{
				ZZE_PK = attributePK,
				ZZE_ZZD_CodeList = expiredParentRecordPk,
				RefCusCodeOrAttributeTransportModes = new[] { new RefCusCodeOrAttributeTransportMode { ZZU_PK = transportModePK } }
			};
			var existingObject = new RefCusCodeList
			{
				ZZD_PK = expiredParentRecordPk,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE1",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms",
				ZZD_StartDate = new DateTime(2019, 01, 01),
				ZZD_EndDate = new DateTime(2020, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA",
				RefCusCodeListLanguages = languages,
				RefCusCodeListAttributes = new[] { attribute }
			};
			var newObject = new RefCusCodeList
			{
				ZZD_PK = newParentRecordPK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE2",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms",
				ZZD_StartDate = new DateTime(2020, 06, 07),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA",
			};
			var exceptItems = new Dictionary<string, List<Guid>>
			{
				{ "ZXA", new List<Guid>() { languageToIgnorePk } }
			};
			var cloneProcessObject = new CloneProcessObject
			{
				DataProcessingClonePK = dataProcessingClonePK,
				ExpiredRecordPk = expiredParentRecordPk,
				NewRecordPk = newParentRecordPK,
				ExceptionListForCloning = exceptItems
			};
			var genericCollectionDic = new Dictionary<Type, List<Type>>
			{
				{ typeof(RefCusCodeList), new List<Type> { typeof(ICollection<RefCusCodeListLanguage>), typeof(ICollection<RefCusCodeListAttribute>) } },
				{ typeof(RefCusCodeListAttribute), new List<Type>() { typeof(ICollection<RefCusCodeOrAttributeTransportMode>) } }
			};

			var cloneHelper = new CloneHelper();
			var cloneResult = cloneHelper.Clone(existingObject, newObject, cloneProcessObject, genericCollectionDic);
			Assert.AreEqual(3, cloneResult.Count());
			var clonedLanguages = cloneResult.Where(x => x.ClonedRecordTypeName == "RefCusCodeListLanguage").ToArray();
			Assert.AreEqual(1, clonedLanguages.Length);
			Assert.AreEqual("EN", (clonedLanguages[0].ClonedRecord as RefCusCodeListLanguage).ZXA_ZX6_NKLanguage);

			var clonedAttributes = cloneResult.Where(x => x.ClonedRecordTypeName == "RefCusCodeListAttribute").ToArray();
			Assert.AreEqual(1, clonedAttributes.Length);
			Assert.AreEqual(attributePK, clonedAttributes[0].OriginalRecordPK);
			Assert.AreEqual(newParentRecordPK, clonedAttributes[0].DataSetPK);
			Assert.AreEqual(newParentRecordPK, clonedAttributes[0].ClonedRecordExpirableAncestorPK);
			var clonedTransportModes = cloneResult.Where(x => x.ClonedRecordTypeName == "RefCusCodeOrAttributeTransportMode").ToArray();
			Assert.AreEqual(1, clonedTransportModes.Length);
			Assert.AreEqual(transportModePK, clonedTransportModes[0].OriginalRecordPK);
			Assert.AreEqual(newParentRecordPK, clonedTransportModes[0].DataSetPK);
			Assert.AreEqual(clonedAttributes[0].ClonedRecordPK, clonedTransportModes[0].ClonedRecordExpirableAncestorPK);
		}

		[Test]
		public void CloneCollection()
		{
			var oldRefCusCodeListPK = Guid.NewGuid();
			var newRefCusCodeListPK = Guid.NewGuid();
			var languageToIgnorePk = Guid.NewGuid();

			var languages = new RefCusCodeListLanguage[]
			{
				new RefCusCodeListLanguage
					{
						ZXA_Description = "OI",
						ZXA_PK = Guid.NewGuid(),
						ZXA_ZX6_NKLanguage = "EN",
						ZXA_ZZD_CodeList = oldRefCusCodeListPK
					},
				new RefCusCodeListLanguage
					{
						ZXA_Description = "OBA",
						ZXA_PK = languageToIgnorePk,
						ZXA_ZX6_NKLanguage = "PT",
						ZXA_ZZD_CodeList = oldRefCusCodeListPK
					}
			};
			var newRefCusCodeList = new RefCusCodeList
			{
				ZZD_PK = newRefCusCodeListPK,
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE2",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms",
				ZZD_StartDate = new DateTime(2020, 06, 07),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA",
			};
			var exceptTheseRecords = new Dictionary<string, List<Guid>>
			{
				{ "ZXA",new List<Guid>{ languageToIgnorePk} }
			};
			var genericCollectionDic = new Dictionary<Type, List<Type>>
			{
				{ typeof(RefCusCodeList), new List<Type> { typeof(ICollection<RefCusCodeListLanguage>) } }
			};

			var cloneHelper = new CloneHelper();
			var result = cloneHelper.CloneCollection(languages, newRefCusCodeList, exceptTheseRecords, genericCollectionDic, newRefCusCodeListPK);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("EN", (result.First().ClonedRecord as RefCusCodeListLanguage).ZXA_ZX6_NKLanguage);
		}

		[Test]
		public void Clone_DoNotCloneExpiredData()
		{
			var oldTariffPK = Guid.NewGuid();
			var newTariffPK = Guid.NewGuid();
			var tariffTypePK = Guid.NewGuid();
			var rate1PK = Guid.NewGuid();
			var rate2PK = Guid.NewGuid();
			var rate3PK = Guid.NewGuid();
			var tradeGroupPK = Guid.NewGuid();

			//rate1-app1: not expired
			var applicability1 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate1PK,
				ZZT_StartDate = new DateTime(1900, 1, 1),
				ZZT_EndDate = new DateTime(2079, 12, 31),
				ZZT_AdditionalCode = "A1"
			};
			//non-expirable type
			applicability1.RefCusExcludedTradeGroups = new[] { new RefCusExcludedTradeGroup { ZZC_ZZT_Applicability = rate1PK, ZZC_ZZA_TradeGroup = tradeGroupPK } };
			//rate1-app2: expired
			var applicability2 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate1PK,
				ZZT_StartDate = new DateTime(2000, 1, 1),
				ZZT_EndDate = new DateTime(2023, 12, 31),
				ZZT_AdditionalCode = "A2"
			};
			//rate2-app3: expired
			var applicability3 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate2PK,
				ZZT_StartDate = new DateTime(2023, 1, 1),
				ZZT_EndDate = new DateTime(2079, 12, 31),
				ZZT_AdditionalCode = "A3"
			};
			//rate3-app4: expired
			var applicability4 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate3PK,
				ZZT_StartDate = new DateTime(1900, 1, 1),
				ZZT_EndDate = new DateTime(2023, 12, 31),
				ZZT_AdditionalCode = "A3"
			};
			//need clone
			var rate1 = new RefCusRate
			{
				ZZ2_PK = rate1PK,
				ZZ2_ZZ1_Tariff = oldTariffPK,
				ZZ2_StartDate = new DateTime(2023, 1, 1),
				ZZ2_EndDate = new DateTime(2079, 12, 31),
				ZZ2_RateFormula = "R1",
				RefCusApplicabilities = new[] { applicability1, applicability2 }
			};
			//not clone
			var rate2 = new RefCusRate
			{
				ZZ2_PK = rate2PK,
				ZZ2_ZZ1_Tariff = oldTariffPK,
				ZZ2_StartDate = new DateTime(1900, 1, 1),
				ZZ2_EndDate = new DateTime(2023, 12, 31),
				ZZ2_RateFormula = "R2",
				RefCusApplicabilities = new[] { applicability3 }
			};
			//not clone
			var rate3 = new RefCusRate
			{
				ZZ2_PK = rate3PK,
				ZZ2_ZZ1_Tariff = oldTariffPK,
				ZZ2_StartDate = new DateTime(1900, 1, 1),
				ZZ2_EndDate = new DateTime(2079, 12, 31),
				ZZ2_RateFormula = "R3",
				RefCusApplicabilities = new[] { applicability4 }
			};
			//expired
			var tariffNationalCode1 = new RefCusTariffNationalCode
			{
				ZZW_ZZ1_Tariff = oldTariffPK,
				ZZW_NationalCode = "N1",
				ZZW_StartDate = new DateTime(2000, 1, 1),
				ZZW_EndDate = new DateTime(2023, 12, 31)
			};
			//not expired
			var tariffNationalCode2 = new RefCusTariffNationalCode
			{
				ZZW_ZZ1_Tariff = oldTariffPK,
				ZZW_NationalCode = "N2",
				ZZW_StartDate = new DateTime(2000, 1, 1),
				ZZW_EndDate = new DateTime(2024, 12, 31)
			};

			//non-expirable type
			var tariffAttribute = new RefCusTariffAttribute
			{
				ZZ3_PK = Guid.NewGuid(),
				ZZ3_ZZ1_Tariff = oldTariffPK,
				ZZ3_Name = "Att1",
				ZZ3_Value = "Val1"
			};
			var oldTariff = new RefCusTariff
			{
				ZZ1_PK = oldTariffPK,
				ZZ1_TariffCode = "00001",
				ZZ1_StartDate = new DateTime(1900, 1, 1),
				ZZ1_EndDate = new DateTime(2023, 12, 31),
				ZZ1_ZZI_TariffType = tariffTypePK,
				RefCusRates = new[] { rate1, rate2, rate3 },
				RefCusTariffAttributes = new[] { tariffAttribute },
				RefCusTariffNationalCodes = new[] { tariffNationalCode1, tariffNationalCode2 }
			};
			var newTariff = new RefCusTariff
			{
				ZZ1_PK = newTariffPK,
				ZZ1_TariffCode = "00001",
				ZZ1_StartDate = new DateTime(2024, 1, 1),
				ZZ1_EndDate = new DateTime(2079, 12, 31),
				ZZ1_ZZI_TariffType = tariffTypePK
			};

			var exceptTheseRecords = new Dictionary<string, List<Guid>>();
			var genericCollectionDic = new Dictionary<Type, List<Type>>
			{
				{ typeof(RefCusTariff), new List<Type> { typeof(ICollection<RefCusRate>), typeof(ICollection<RefCusTariffAttribute>), typeof(ICollection<RefCusTariffNationalCode>) } },
				{ typeof(RefCusRate), new List<Type> { typeof(ICollection<RefCusApplicability>) } },
				{ typeof(RefCusApplicability), new List<Type> { typeof(ICollection<RefCusExcludedTradeGroup>) } }
			};
			var cloneProcessObject = new CloneProcessObject
			{
				ExpiredRecordPk = oldTariffPK,
				NewRecordPk = newTariffPK,
				ExceptionListForCloning = exceptTheseRecords
			};

			var cloneHelper = new CloneHelper();
			var result = cloneHelper.Clone(oldTariff, newTariff, cloneProcessObject, genericCollectionDic).ToArray();
			Assert.AreEqual(5, result.Length);
			var clonedTariffNationalCode = result.First(x => x.ClonedRecordTypeName == nameof(RefCusTariffNationalCode)).ClonedRecord as RefCusTariffNationalCode;
			Assert.AreEqual("N2", clonedTariffNationalCode.ZZW_NationalCode);
			Assert.AreEqual(newTariffPK, clonedTariffNationalCode.ZZW_ZZ1_Tariff);
			var clonedTariffAttribute = result.First(x => x.ClonedRecordTypeName == nameof(RefCusTariffAttribute)).ClonedRecord as RefCusTariffAttribute;
			Assert.AreEqual("Att1", clonedTariffAttribute.ZZ3_Name);
			Assert.AreEqual("Val1", clonedTariffAttribute.ZZ3_Value);
			Assert.AreEqual(newTariffPK, clonedTariffAttribute.ZZ3_ZZ1_Tariff);
			var clonedRate = result.First(x => x.ClonedRecordTypeName == nameof(RefCusRate)).ClonedRecord as RefCusRate;
			Assert.AreEqual("R1", clonedRate.ZZ2_RateFormula);
			Assert.AreEqual(newTariffPK, clonedRate.ZZ2_ZZ1_Tariff);
			var clonedApplicability = result.First(x => x.ClonedRecordTypeName == nameof(RefCusApplicability)).ClonedRecord as RefCusApplicability;
			Assert.AreEqual("A1", clonedApplicability.ZZT_AdditionalCode);
			Assert.AreEqual(clonedRate.ZZ2_PK, clonedApplicability.ZZT_ZZ2_Rate);
			var clonedExcludedTradeGroup = result.First(x => x.ClonedRecordTypeName == nameof(RefCusExcludedTradeGroup)).ClonedRecord as RefCusExcludedTradeGroup;
			Assert.AreEqual(tradeGroupPK, clonedExcludedTradeGroup.ZZC_ZZA_TradeGroup);
			Assert.AreEqual(clonedApplicability.ZZT_PK, clonedExcludedTradeGroup.ZZC_ZZT_Applicability);
		}
	}
}
