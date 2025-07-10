using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Assertion = NUnit.Framework.Assertion;

namespace Enterprise.Customs.Universal.Testing
{
	public class UniversalReferenceTestDataHelper : Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper
	{
		public UniversalReferenceTestDataHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		#region RefCusQuota

		public static RefCusQuota CreateOrFindExistingRefCusQuota(BusinessObjectFactory factory, ZString dataGroupingCode, ZDecimal balance, ZDecimal initialAmount, ZString orderNumber,
			ZString unitOfMeasure, ZDateTime startDate, ZDateTime endDate, bool ensureDataGroupingExists = true)
		{
			var query = new ZQuery(RefCusQuotaSchema.ZXQ_Balance, balance);
			query.AddToFilter(RefCusQuotaSchema.ZXQ_EndDate, endDate);
			query.AddToFilter(RefCusQuotaSchema.ZXQ_InitialAmount, initialAmount);
			query.AddToFilter(RefCusQuotaSchema.ZXQ_OrderNumber, orderNumber);
			query.AddToFilter(RefCusQuotaSchema.ZXQ_StartDate, startDate);
			query.AddToFilter(RefCusQuotaSchema.ZXQ_UnitOfMeasure, unitOfMeasure);
			query.AddToFilter(RefCusQuotaSchema.ZXQ_ZZZ_NKDataGrouping, dataGroupingCode);
			return factory.LoadTop1<RefCusQuota>(query) ?? CreateRefCusQuota(factory, dataGroupingCode, balance, initialAmount, orderNumber,
				unitOfMeasure, startDate, endDate, ensureDataGroupingExists);
		}

		public static RefCusQuota CreateRefCusQuota(BusinessObjectFactory factory, ZString dataGroupingCode, ZDecimal balance, ZDecimal initialAmount, ZString orderNumber,
			ZString unitOfMeasure, ZDateTime startDate, ZDateTime endDate, bool ensureDataGroupingExists = true)
		{
			if (ensureDataGroupingExists)
			{
				new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			}

			var cusQuota = factory.New<RefCusQuota>();
			cusQuota.ZXQ_Balance = balance;
			cusQuota.ZXQ_EndDate = endDate;
			cusQuota.ZXQ_InitialAmount = initialAmount;
			cusQuota.ZXQ_OrderNumber = orderNumber;
			cusQuota.ZXQ_StartDate = startDate;
			cusQuota.ZXQ_UnitOfMeasure = unitOfMeasure;
			cusQuota.ZXQ_ZZZ_NKDataGrouping = dataGroupingCode;
			return cusQuota;
		}

		#endregion

		#region RefCusAUNexdocECMCode

		public RefCusAUNexdocECMCode CreateOrFindExistingRefCusAUNexdocECMCode(ZString commodityCode, ZString preservationCode, ZString productTypeCode, ZString packTypeCode, ZString supplementaryCode)
		{
			var query = new ZQuery(RefCusAUNexdocECMCodeSchema.ZY5_CommodityCode, commodityCode);
			query.AddToFilter(RefCusAUNexdocECMCodeSchema.ZY5_PreservationCode, preservationCode);
			query.AddToFilter(RefCusAUNexdocECMCodeSchema.ZY5_ProductTypeCode, productTypeCode);
			query.AddToFilter(RefCusAUNexdocECMCodeSchema.ZY5_PackTypeCode, packTypeCode);
			query.AddToFilter(RefCusAUNexdocECMCodeSchema.ZY5_SupplementaryCode, supplementaryCode);
			var refCusProc = factory.LoadTop1<RefCusAUNexdocECMCode>(query);
			return refCusProc ?? CreateRefCusAUNexdocECMCode(commodityCode, preservationCode, productTypeCode, packTypeCode, supplementaryCode);
		}

		public RefCusAUNexdocECMCode CreateRefCusAUNexdocECMCode(ZString commodityCode, ZString preservationCode, ZString productTypeCode, ZString packTypeCode, ZString supplementaryCode)
		{
			var result = factory.New<RefCusAUNexdocECMCode>();
			result.ZY5_CommodityCode = commodityCode;
			result.ZY5_PreservationCode = preservationCode;
			result.ZY5_ProductTypeCode = productTypeCode;
			result.ZY5_PackTypeCode = packTypeCode;
			result.ZY5_SupplementaryCode = supplementaryCode;
			return result;
		}

		#endregion

		/// <summary>
		/// Note, the lookup for an existing record heeds only the columns that comprise the unique index  'IX_RefCusProcedure_ZZ6_ZZZ_NKDataGrouping_ZZ6_ProcedureCode_ZZ6_PreviousProcedureCode_ZZ6_Concession_ZZ6_Category'
		/// </summary>
		public RefCusProcedure CreateOrFindExistingRefCusProcedure(
			ZString zzzDataGrouping,
			ZString category,
			ZString procedureCode,
			ZString previousProcedureCode,
			ZString concession,
			ZString description,
			ZString shipmentType,
			string group = "",
			bool calculateDuty = true,
			bool landedCostOnly = false,
			bool intoWarehouse = false,
			bool outOfWarehouse = false)
		{
			var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, zzzDataGrouping);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, previousProcedureCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, concession);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_Category, category);
			var refCusProc = factory.LoadTop1<RefCusProcedure>(query);
			if (refCusProc != null)
			{
				refCusProc.ZZ6_ShipmentType = shipmentType;
				refCusProc.ZZ6_Group = group;
				refCusProc.ZZ6_Description = description;
			}
			return refCusProc ?? CreateRefCusProcedure(zzzDataGrouping, category, procedureCode, previousProcedureCode, concession, description, shipmentType, calculateDuty, landedCostOnly, intoWarehouse, outOfWarehouse, group);
		}

		public RefCusProcedure CreateRefCusProcedure(
			ZString dataGroupingCode,
			ZString category,
			ZString procedureCode,
			ZString previousProcedureCode,
			ZString concession,
			ZString description,
			ZString shipmentType,
			bool calculateDuty = true,
			bool landedCostOnly = false,
			bool intoWarehouse = false,
			bool outOfWarehouse = false,
			string group = "")
		{
			if (description.IsEmpty)
			{
				description = "Description for " + procedureCode;
			}
			var result = factory.New<RefCusProcedure>();
			CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			result.ZZ6_ZZZ_NKDataGrouping = dataGroupingCode;
			result.ZZ6_Category = category;
			result.ZZ6_ProcedureCode = procedureCode;
			result.ZZ6_PreviousProcedureCode = previousProcedureCode;
			result.ZZ6_Concession = concession;
			result.ZZ6_Description = description;
			result.ZZ6_ShipmentType = shipmentType;
			result.ZZ6_CalculateDuty = calculateDuty;
			result.ZZ6_LandedCost = landedCostOnly;
			result.ZZ6_IntoWarehouse = intoWarehouse ? WarehouseMoveStatus.Codes.Yes : WarehouseMoveStatus.Codes.No;
			result.ZZ6_OutOfWarehouse = outOfWarehouse ? WarehouseMoveStatus.Codes.Yes : WarehouseMoveStatus.Codes.No;
			result.ZZ6_Group = group;
			return result;
		}

		public RefCusProcedure CreateRefCusProcedureWithDefaultDate(
			ZString dataGroupingCode,
			ZString category,
			ZString procedureCode,
			ZString previousProcedureCode,
			ZString concession,
			ZString description,
			ZString shipmentType,
			ZDateTime startDate,
			ZDateTime endDate,
			bool calculateDuty = true,
			bool landedCostOnly = false,
			bool intoWarehouse = false,
			bool outOfWarehouse = false,
			string group = "")
		{
			var result = CreateRefCusProcedure(dataGroupingCode, category, procedureCode, previousProcedureCode, concession, description, shipmentType, calculateDuty, landedCostOnly, intoWarehouse, outOfWarehouse, group);
			result.ZZ6_StartDate = startDate;
			result.ZZ6_EndDate = endDate;
			return result;
		}

		#region RefCusProcedureAttribute

		public RefCusProcedureAttribute CreateRefCusProcedureAttribute(ZGuid refCusProcedurePK, ZString attributeName, ZString attributeValue)
		{
			var refCusProcedureAttrib = factory.New<RefCusProcedureAttribute>();
			refCusProcedureAttrib.ZXB_ZZ6_ProcedureCode = refCusProcedurePK;
			refCusProcedureAttrib.ZXB_Name = attributeName;
			refCusProcedureAttrib.ZXB_Value = attributeValue;
			return refCusProcedureAttrib;
		}

		#endregion

		public RefCusProcedureLanguage CreateRefCusProcedureLanguage(RefCusProcedure procedure, string languageCode, string localLanguageDescription)
		{
			var procedureLanguage = factory.New<RefCusProcedureLanguage>();
			procedureLanguage.ZXV_ZX6_NKLanguage = languageCode;
			procedureLanguage.ZXV_ZZ6_Procedure = procedure.PK;
			procedureLanguage.ZXV_Description = localLanguageDescription;
			return procedureLanguage;
		}

		public RefCusRateType CreateNewOrGetExistingRateType(ZString dataGroupingCode, ZString rateType, string description = null, bool isExport = false)
		{
			var result = RefCusRateType.Loader.Load(factory, dataGroupingCode, rateType) ?? CreateCusRateType(dataGroupingCode, rateType, description, isExport: isExport);
			return result;
		}

		public RefCusRateType CreateCusRateType(ZString dataGroupingCode, ZString rateType, string description = null, bool ensureDataGroupingExists = true, bool isExport = false)
		{
			return CreateCusRateType(factory, dataGroupingCode, rateType, description, ensureDataGroupingExists: ensureDataGroupingExists, isExport: isExport);
		}

		public static RefCusRateType CreateCusRateType(BusinessObjectFactory factory, ZString dataGroupingCode, ZString rateType, string description = null, bool ensureDataGroupingExists = true, bool isExport = false)
		{
			var cusRateType = factory.New<RefCusRateType>();
			if (ensureDataGroupingExists)
			{
				new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			}
			cusRateType.ZZR_ZZZ_NKDataGrouping = dataGroupingCode;
			cusRateType.ZZR_RateType = rateType;
			cusRateType.ZZR_Description = description ?? rateType + " DESC";
			cusRateType.ZZR_IsExport = isExport;
			factory.Save();
			return cusRateType;
		}

		public RefCusTariffType CreateTariffType(ZString dataGroupingCode, ZString tariffType, string nomenclatureGroupType = null, bool ensureDataGroupingExists = true)
		{
			return CreateTariffType(factory, dataGroupingCode, tariffType, nomenclatureGroupType, ensureDataGroupingExists: ensureDataGroupingExists);
		}

		public static RefCusTariffType CreateTariffType(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, string nomenclatureGroupType = null, bool ensureDataGroupingExists = true)
		{
			var cusTariffType = factory.New<RefCusTariffType>();
			if (ensureDataGroupingExists)
			{
				new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			}
			cusTariffType.ZZI_ZZZ_NKDataGrouping = dataGroupingCode;
			cusTariffType.ZZI_TariffType = tariffType;
			cusTariffType.ZZI_Description = tariffType + " DESC";
			cusTariffType.ZZI_ZZ9_NKNomenclatureGroupType = nomenclatureGroupType ?? "";
			return cusTariffType;
		}

		public RefCusTariffTypeLanguage CreateRefCusTariffTypeLanguage(RefCusTariffType tariffType, string languageCode, string localLanguageDescription)
		{
			var tariffTypeLanguage = factory.New<RefCusTariffTypeLanguage>();
			tariffTypeLanguage.ZXK_ZX6_NKLanguage = languageCode;
			tariffTypeLanguage.ZXK_ZZI_TariffType = tariffType.PK;
			tariffTypeLanguage.ZXK_Description = localLanguageDescription;
			return tariffTypeLanguage;
		}

		public RefCusTariffType CreateNewOrGetExistingTariffType(ZString dataGroupingCode, ZString typeCode, string nomenclatureGroupType = null)
		{
			return CreateNewOrGetExistingRefCusTariffType(factory, dataGroupingCode, typeCode, nomenclatureGroupType);
		}

		public static RefCusTariffType CreateNewOrGetExistingRefCusTariffType(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, string nomenclatureGroupType = null, bool ensureDataGroupingExists = true)
		{
			var tariffTypeQuery = new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, tariffType);
			tariffTypeQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, dataGroupingCode);
			var cusTariffType = factory.LoadTop1<RefCusTariffType>(tariffTypeQuery) ?? CreateTariffType(factory, dataGroupingCode, tariffType, nomenclatureGroupType, ensureDataGroupingExists);
			return cusTariffType;
		}

		#region RefCusConditionValueType & RefCusCondition & RefCusConditionType & RefCusConditionValue & RefCusConditionCode

		public RefCusCondition CreateOrGetExistingRefCusCondition(ZString dataGroupingCode, ZGuid conditionType, ZGuid tariffOrNomenclature, ZString comment, ZBool isImport,
			ZBool isExport, ZDateTime startDate, ZDateTime endDate, Action<RefCusCondition> afterCreate = null, ZGuid? preferencePK = null, bool isTariff = true, ZString? severity = null)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCusConditionSchema.ZX1_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(RefCusConditionSchema.ZX1_ZX2_ConditionType, conditionType);
			query.AddToFilter(isTariff ? RefCusConditionSchema.ZX1_ZZ1_Tariff : RefCusConditionSchema.ZX1_ZZ5_Nomenclature, tariffOrNomenclature);
			query.AddToFilter(RefCusConditionSchema.ZX1_Comment, comment);
			query.AddToFilter(RefCusConditionSchema.ZX1_IsImport, isImport);

			query.AddToFilter(RefCusConditionSchema.ZX1_IsExport, isExport);
			query.AddToFilter(RefCusConditionSchema.ZX1_StartDate, startDate);
			query.AddToFilter(RefCusConditionSchema.ZX1_EndDate, endDate);
			if (preferencePK.HasValue)
			{
				query.AddToFilter(RefCusConditionSchema.ZX1_ZZS_Preference, preferencePK.Value);
			}

			var result = factory.LoadTop1<RefCusCondition>(query);

			if (result == null)
			{
				var newFactory = new BusinessObjectFactory();
				result = newFactory.New<RefCusCondition>();
				CreateNewOrGetExistingDataGrouping(dataGroupingCode);

				result.ZX1_ZZZ_NKDataGrouping = dataGroupingCode;
				result.ZX1_ZX2_ConditionType = conditionType;
				if (isTariff)
				{
					result.ZX1_ZZ1_Tariff = tariffOrNomenclature;
				}
				else
				{
					result.ZX1_ZZ5_Nomenclature = tariffOrNomenclature;
				}

				result.ZX1_Comment = comment;
				result.ZX1_IsImport = isImport;

				result.ZX1_IsExport = isExport;
				result.ZX1_StartDate = startDate;
				result.ZX1_EndDate = endDate;
				if (preferencePK.HasValue)
				{
					result.ZX1_ZZS_Preference = preferencePK.Value;
				}
				if (severity.HasValue)
				{
					result.ZX1_Severity = severity.Value;
				}
				afterCreate?.Invoke(result);
				newFactory.Save();
			}

			return result;
		}

		public RefCusConditionType CreateOrGetExistingRefCusConditionType(ZString dataGroupingCode, ZString conditionClass, ZString conditionType,
			string description = "Default Condition Description")
		{
			var query = new ZQuery();
			query.AddToFilter(RefCusConditionTypeSchema.ZX2_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(RefCusConditionTypeSchema.ZX2_ConditionClass, conditionClass);
			query.AddToFilter(RefCusConditionTypeSchema.ZX2_ConditionType, conditionType);

			var result = factory.LoadTop1<RefCusConditionType>(query);

			if (result == null)
			{
				var newFactory = new BusinessObjectFactory();
				result = newFactory.New<RefCusConditionType>();
				CreateNewOrGetExistingDataGrouping(dataGroupingCode);
				result.ZX2_ZZZ_NKDataGrouping = dataGroupingCode;
				result.ZX2_ConditionClass = conditionClass;
				result.ZX2_ConditionType = conditionType;
				result.ZX2_Description = description;
				newFactory.Save();
			}

			return result;
		}

		public RefCusConditionValue CreateOrGetExistingRefCusConditionValue(ZGuid valueType, ZGuid condition, ZString value, Action<RefCusConditionValue> afterCreate = null)
		{
			return CreateOrGetExistingRefCusConditionValue(valueType, condition, value, 0, afterCreate);
		}

		public RefCusConditionValue CreateOrGetExistingRefCusConditionValue(ZGuid valueType, ZGuid condition, ZString value, ZByte logicalOrWithinGroup, Action<RefCusConditionValue> afterCreate = null)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCusConditionValueSchema.ZX3_ZX4_ValueType, valueType);
			query.AddToFilter(RefCusConditionValueSchema.ZX3_ZX1_Condition, condition);
			query.AddToFilter(RefCusConditionValueSchema.ZX3_Value, value);
			query.AddToFilter(RefCusConditionValueSchema.ZX3_LogicalORWithinGroup, logicalOrWithinGroup);
			var result = factory.LoadTop1<RefCusConditionValue>(query);

			if (result == null)
			{
				var newFactory = new BusinessObjectFactory();
				result = newFactory.New<RefCusConditionValue>();
				result.ZX3_ZX4_ValueType = valueType;
				result.ZX3_ZX1_Condition = condition;
				result.ZX3_Value = value;
				result.ZX3_LogicalORWithinGroup = logicalOrWithinGroup;
				afterCreate?.Invoke(result);
				newFactory.Save();
			}

			return result;
		}

		public RefCusConditionLanguage CreateOrGetExistingRefCusConditionLanguage(ZGuid condition, ZString languageType, ZString source, ZString comment)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCusConditionLanguageSchema.ZXJ_ZX6_NKLanguage, languageType);
			query.AddToFilter(RefCusConditionLanguageSchema.ZXJ_ZX1_Condition, condition);
			var result = factory.LoadTop1<RefCusConditionLanguage>(query);

			if (result == null)
			{
				var newFactory = new BusinessObjectFactory();
				result = newFactory.New<RefCusConditionLanguage>();
				result.ZXJ_ZX6_NKLanguage = languageType;
				result.ZXJ_ZX1_Condition = condition;
				result.ZXJ_Source = source;
				result.ZXJ_Comment = comment;
				newFactory.Save();
			}

			return result;
		}

		public RefCusConditionValueType CreateOrGetExistingRefCusConditionValueType(ZString dataGroupingCode, ZString valueType, string description = "Default Value Type Description", bool isFormula = false, Action<RefCusConditionValueType> afterCreate = null)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCusConditionValueTypeSchema.ZX4_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(RefCusConditionValueTypeSchema.ZX4_ValueType, valueType);
			var result = factory.LoadTop1<RefCusConditionValueType>(query);

			if (result == null)
			{
				var newFactory = new BusinessObjectFactory();
				result = newFactory.New<RefCusConditionValueType>();
				CreateNewOrGetExistingDataGrouping(dataGroupingCode);
				result.ZX4_ValueType = valueType;
				result.ZX4_Description = description;
				result.ZX4_IsFormula = isFormula;
				result.ZX4_ZZZ_NKDataGrouping = dataGroupingCode;
				afterCreate?.Invoke(result);
				newFactory.Save();
			}
			return result;
		}

		public RefCusConditionCode CreateOrGetExistingRefCusConditionCode(ZString dataGroupingCode, ZString conditionCode)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCusConditionCodeSchema.ZY7_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(RefCusConditionCodeSchema.ZY7_ConditionCode, conditionCode);
			var result = factory.LoadTop1<RefCusConditionCode>(query);

			if (result == null)
			{
				var newFactory = new BusinessObjectFactory();
				result = newFactory.New<RefCusConditionCode>();
				CreateNewOrGetExistingDataGrouping(dataGroupingCode);
				result.ZY7_ConditionCode = conditionCode;
				result.ZY7_ZZZ_NKDataGrouping = dataGroupingCode;
				newFactory.Save();
			}
			return result;
		}

		#endregion

		public TariffAttributeView CreateTariffAttribute(ZString attributeName, ZString attributeValue, TariffView tariffView)
		{
			var newFactory = new BusinessObjectFactory();
			var attribute = CreateInternalRefCusTariffAttribute(newFactory, attributeName, attributeValue, tariffView.IsTariffNationalCode, tariffView.PK);
			newFactory.Save();
			return factory.Load<TariffAttributeView>(attribute.PK);
		}

		public TariffAttributeView CreateNewOrGetExistingTariffAttribute(ZString attributeName, ZString attributeValue, TariffView tariffView)
		{
			var tariffAttributeQuery = new ZQuery(TariffAttributeViewSchema.ZZ3_Name, attributeName);
			tariffAttributeQuery.AddToFilter(TariffAttributeViewSchema.ZZ3_Value, attributeValue);
			tariffAttributeQuery.AddToFilter(TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode, tariffView.PK);
			var tariffAttribute = factory.LoadTop1<TariffAttributeView>(tariffAttributeQuery) ?? CreateTariffAttribute(attributeName, attributeValue, tariffView);
			return tariffAttribute;
		}

		public static Internal.RefCusTariffAttribute CreateInternalRefCusTariffAttribute(BusinessObjectFactory factory, ZString attributeName, ZString attributeValue, bool isTariffNationalCode, ZGuid parentPK)
		{
			var cusTariffAttrib = factory.New<Internal.RefCusTariffAttribute>();
			cusTariffAttrib.ZZ3_Name = attributeName;
			cusTariffAttrib.ZZ3_Value = attributeValue;
			if (isTariffNationalCode)
			{
				cusTariffAttrib.ZZ3_ZZW_TariffNationalCode = parentPK;
			}
			else
			{
				cusTariffAttrib.ZZ3_ZZ1_Tariff = parentPK;
			}
			return cusTariffAttrib;
		}

		public Internal.RefCusTariffAttributeName CreateNewOrGetExistingRefCusTariffAttributeName(BusinessObjectFactory factory, ZString name, ZString description, ZString columnCaption, ZString dataGrouping, ZString tariffType)
		{
			var query = new ZQuery(RefCusTariffAttributeNameSchema.ZY6_Name, name);
			query.AddToFilter(RefCusTariffAttributeNameSchema.ZY6_ZZZ_NKDataGrouping, dataGrouping);
			query.AddToFilter(RefCusTariffAttributeNameSchema.ZY6_ZZI_NKTariffType, tariffType);
			var attrName = factory.LoadTop1<Internal.RefCusTariffAttributeName>(query);

			if (attrName == null)
			{
				attrName = factory.New<Internal.RefCusTariffAttributeName>();
				attrName.ZY6_Name = name;
				attrName.ZY6_ZZI_NKTariffType = tariffType;
				attrName.ZY6_ZZZ_NKDataGrouping = dataGrouping;
			}
			attrName.ZY6_Description = description;
			attrName.ZY6_ColumnCaption = columnCaption;

			return attrName;
		}

		public RefCusNomenclatureGroupNote CreateNomenclatureGroupNote(RefCusNomenclatureGroup group, ZString languageType, ZString noteType, ZString note, ZString? dataGrouping = null)
		{
			var groupNote = factory.New<RefCusNomenclatureGroupNote>();
			groupNote.ZZL_ZZ5_NomenclatureGroup = group.PK;
			groupNote.ZZL_ZX6_NKLanguage = languageType;
			groupNote.ZZL_NoteType = noteType;
			groupNote.ZZL_Note = note;
			groupNote.ZZL_ZZZ_NKDataGrouping = dataGrouping ?? group.ZZ5_ZZZ_NKDataGrouping;

			return groupNote;
		}

		public RefCusNomenclatureGroupType CreateNomenclatureGroupType(ZString groupType, ZString description)
		{
			var groupTypeBO = factory.New<RefCusNomenclatureGroupType>();
			groupTypeBO.ZZ9_GroupType = groupType;
			groupTypeBO.ZZ9_Description = description;

			return groupTypeBO;
		}

		public RefLanguageType CreateOrGetLanguage(string languageCode, string languageDesc)
		{
			var language = factory.LoadFromNaturalKey<RefLanguageType>(RefLanguageTypeSchema.ZX6_Language, languageCode);

			if (language == null)
			{
				language = factory.New<RefLanguageType>();
				language.ZX6_Language = languageCode;
				language.ZX6_Description = languageDesc;
			}

			return language;
		}

		public RefSysConfig CreateRefSysConfig<T>(ZString configcode, T value, ZDateTime startdate, ZDateTime enddate)
		{
			var refsysconfig = factory.New<RefSysConfig>();
			refsysconfig.ZRC_ZRT_NKConfigCode = configcode;
			SetRefSysConfigValue(refsysconfig, value);
			refsysconfig.ZRC_StartDate = startdate;
			refsysconfig.ZRC_EndDate = enddate;
			return refsysconfig;
		}

		public RefSysConfig CreateOrUpdateExistingRefSysConfig<T>(ZString configcode, T value, ZDateTime startdate, ZDateTime enddate)
		{
			var existingConfig = factory.LoadFromNaturalKey<RefSysConfig>(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, configcode);
			if (existingConfig is null)
			{
				return CreateRefSysConfig(configcode, value, startdate, enddate);
			}

			SetRefSysConfigValue(existingConfig, value);
			existingConfig.ZRC_StartDate = startdate;
			existingConfig.ZRC_EndDate = enddate;
			return existingConfig;
		}

		static void SetRefSysConfigValue<T>(RefSysConfig refsysconfig, T value)
		{
			if (value is string stringValue)
			{
				refsysconfig.ZRC_StringValue = stringValue;
			}
			else if (value is ZString zstringValue)
			{
				refsysconfig.ZRC_StringValue = zstringValue;
			}
			else if (value is byte[] binaryValue)
			{
				refsysconfig.ZRC_BinaryValue = binaryValue;
			}
			else if (value is ZBlob blobValue)
			{
				refsysconfig.ZRC_BinaryValue = blobValue;
			}
			else if (ZDecimalTypeConverter.Instance.CanConvertFrom(typeof(T)))
			{
				refsysconfig.ZRC_DecimalValue = new(value);
			}
			else
			{
				throw new InvalidOperationException($"Unrecognized value type '{typeof(T).FullName}'");
			}
		}

		public RefSysConfigType CreateRefSysConfigType(ZString configcode, ZString description, ZString longdescription)
		{
			var refsysconfigtype = factory.New<RefSysConfigType>();
			refsysconfigtype.ZRT_ConfigCode = configcode;
			refsysconfigtype.ZRT_Description = description;
			refsysconfigtype.ZRT_LongDescription = longdescription;
			return refsysconfigtype;
		}

		public RefSysConfigType CreateOrGetExistingRefSysConfigType(ZString configcode, ZString description, ZString longdescription)
		{
			return factory.LoadFromNaturalKey<RefSysConfigType>(RefSysConfigTypeSchema.ZRT_ConfigCode, configcode) ?? CreateRefSysConfigType(configcode, description, longdescription);
		}

		public RefCusCodeListLanguage CreateCusCodeListLanguage(RefCusCodeList zzd, string languageCode, string localLanguageDescription)
		{
			var cusCodeListLanguage = factory.New<RefCusCodeListLanguage>();
			cusCodeListLanguage.ZXA_ZX6_NKLanguage = languageCode;
			cusCodeListLanguage.ZXA_ZZD_CodeList = zzd.PK;
			cusCodeListLanguage.ZXA_Description = localLanguageDescription;

			return cusCodeListLanguage;
		}

		public RefCusCodeListLanguage CreateNewOrGetExistingCusCodeListLanguage(RefCusCodeList zzd, string languageCode, string localLanguageDescription)
		{
			var cusCodeListLanguageQuery = new ZQuery(RefCusCodeListLanguageSchema.ZXA_ZX6_NKLanguage, languageCode);
			cusCodeListLanguageQuery.AddToFilter(RefCusCodeListLanguageSchema.ZXA_ZZD_CodeList, zzd.PK);
			var cusCodeListLanguage = factory.LoadTop1<RefCusCodeListLanguage>(cusCodeListLanguageQuery) ?? CreateCusCodeListLanguage(zzd, languageCode, localLanguageDescription);
			return cusCodeListLanguage;
		}

		public RefCusRateTypeLanguage CreateCusRateTypeLanguage(RefCusRateType zzr, string languageCode, string localLanguageDescription)
		{
			var refCusRateTypeLanguage = factory.New<RefCusRateTypeLanguage>();
			refCusRateTypeLanguage.ZXT_ZX6_NKLanguage = languageCode;
			refCusRateTypeLanguage.ZXT_ZZR_RateType = zzr.PK;
			refCusRateTypeLanguage.ZXT_Description = localLanguageDescription;

			return refCusRateTypeLanguage;
		}

		public RefCusRateTypeLanguage CreateNewOrGetExistingCusRateTypeLanguage(RefCusRateType zzr, string languageCode, string localLanguageDescription)
		{
			var cusRateTypeLanguageQuery = new ZQuery(RefCusRateTypeLanguageSchema.ZXT_ZX6_NKLanguage, languageCode);
			cusRateTypeLanguageQuery.AddToFilter(RefCusRateTypeLanguageSchema.ZXT_ZZR_RateType, zzr.PK);
			var cusRateTypeLanguage = factory.LoadTop1<RefCusRateTypeLanguage>(cusRateTypeLanguageQuery) ?? CreateCusRateTypeLanguage(zzr, languageCode, localLanguageDescription);
			return cusRateTypeLanguage;
		}

		public TariffRelationshipView CreateTariffRelationship(ZGuid tariffPK, ZGuid tariffType, ZString tariffCode)
		{
			var newFactory = new BusinessObjectFactory();
			var relationship = CreateInternalRefCusTariffRelationship(newFactory, tariffPK, tariffType, tariffCode);
			newFactory.Save();
			return factory.Load<TariffRelationshipView>(relationship.PK);
		}

		internal static Internal.RefCusTariffRelationship CreateInternalRefCusTariffRelationship(BusinessObjectFactory factory, ZGuid tariffPK, ZGuid tariffType, ZString tariffCode)
		{
			var result = factory.New<Internal.RefCusTariffRelationship>();
			result.ZZH_ZZ1_Tariff = tariffPK;
			result.ZZH_ZZI_TariffType = tariffType;
			result.ZZH_TariffCode = tariffCode;
			return result;
		}

		public RefCusNomenclatureGroup CreateNomenclatureGroup(ZString dataGroupingCode, ZString value, ZDateTime startDate, ZDateTime endDate, string description = null, string compositeKey = null, string nomenclatureGroupType = null, bool ensureDataGroupingExists = true)
		{
			var group = factory.New<RefCusNomenclatureGroup>();
			if (ensureDataGroupingExists)
			{
				CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			}
			group.ZZ5_ZZZ_NKDataGrouping = dataGroupingCode;
			group.ZZ5_Value = value;
			group.ZZ5_StartDate = startDate;
			group.ZZ5_EndDate = endDate;
			group.ZZ5_Description = description ?? "Default Description";
			group.ZZ5_CompositeKey = compositeKey ?? ZString.Empty;
			group.ZZ5_ZZ9_NKNomenclatureGroupType = nomenclatureGroupType ?? ZString.Empty;
			return group;
		}

		public RefCusNomenclatureLanguage CreateNomenclatureGroupLanguage(ZGuid cusNomenclatureGroupPK, ZString language, ZString description)
		{
			var groupLanguage = factory.New<RefCusNomenclatureLanguage>();
			groupLanguage.ZX8_ZZ5_NomenclatureGroup = cusNomenclatureGroupPK;
			groupLanguage.ZX8_ZX6_NKLanguage = language;
			groupLanguage.ZX8_Description = description;
			return groupLanguage;
		}

		public TariffUOMView CreateTariffUOM(TariffView tariffView, ZString uomType, ZString uom, string dataGrouping = "", CusRefTradeGroupView tradeGroup = null)
		{
			TariffUOMView result;
			if (tariffView.ZZ1_IsSystem)
			{
				result = CreateTariffUOM(tariffView.PK, uomType, uom, dataGrouping, tradeGroup);
			}
			else
			{
				result = factory.New<TariffUOMView>();
				result.ZZ8_ZZ1_ParentTariffOrNationalCode = tariffView.PK;
				result.ZZ8_Type = uomType;
				result.ZZ8_UOM = uom;
				if (tradeGroup != null)
				{
					result.ZZ8_ZZA_TradeGroup = tradeGroup.PK;
				}
			}
			return result;
		}

		public TariffUOMView CreateTariffUOM(ZGuid tariffPK, ZString type, ZString uom, string dataGrouping = "", CusRefTradeGroupView tradeGroup = null)
		{
			var newFactory = new BusinessObjectFactory();
			var tariffUOM = CreateInternalRefCusTariffUOM(newFactory, tariffPK, type, uom, dataGrouping, tradeGroup);
			newFactory.Save();
			return factory.Load<TariffUOMView>(tariffUOM.PK);
		}

		public static Internal.RefCusTariffUOM CreateInternalRefCusTariffUOM(BusinessObjectFactory factory, ZGuid tariffPK, ZString type, ZString uom, string dataGrouping = "", CusRefTradeGroupView tradeGroup = null)
		{
			var tariffUOM = factory.New<Internal.RefCusTariffUOM>();
			tariffUOM.ZZ8_ZZ1_Tariff = tariffPK;
			tariffUOM.ZZ8_Type = type;
			tariffUOM.ZZ8_UOM = uom;
			tariffUOM.ZZ8_ZZZ_NKDataGrouping = dataGrouping;
			if (tradeGroup != null)
			{
				tariffUOM.ZZ8_ZZA_TradeGroup = tradeGroup.PK;
			}
			return tariffUOM;
		}

		public CusRefRateUOMView CreateRateUOM(ZGuid ratePk, ZString uomType, bool isSystem = true)
		{
			CusRefRateUOMView rateUOM;

			if (isSystem)
			{
				var refRateUOM = CreateInternalRefCusRateUOM(ratePk, uomType);
				rateUOM = factory.Load<CusRefRateUOMView>(refRateUOM.PK);
			}
			else
			{
				rateUOM = factory.New<CusRefRateUOMView>();
				rateUOM.ZXG_ZZ2_Rate = ratePk;
				rateUOM.ZXG_UOM = uomType;
			}

			return rateUOM;
		}

		RefCusRateUOM CreateInternalRefCusRateUOM(ZGuid ratePk, ZString uomType)
		{
			var newFactory = new BusinessObjectFactory();
			var rateUOM = newFactory.New<RefCusRateUOM>();
			rateUOM.ZXG_ZZ2_Rate = ratePk;
			rateUOM.ZXG_UOM = uomType;
			newFactory.Save();

			return rateUOM;
		}

		public TariffView LoadOrCreateNewTariffNationalCode(ZString dataGroupingCode, ZGuid tariffPK, ZString nationalCode, ZDateTime startDate, ZDateTime endDate, ZDate publishedDate, string description = null, string taxOrFeeCode = null)
		{
			var query = new ZQuery(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.EndsWith, nationalCode);
			query.AddToFilter(TariffViewSchema.ZZ1_ZZ1_Tariff, tariffPK);
			query.AddToFilter(TariffViewSchema.ZZ1_StartDate, startDate);
			query.AddToFilter(TariffViewSchema.ZZ1_IsSystem, true);
			var result = factory.LoadTop1<TariffView>(query) ?? CreateTariffNationalCode(dataGroupingCode, tariffPK, nationalCode, startDate, endDate, publishedDate, description, taxOrFeeCode);

			return result;
		}

		public TariffView CreateTariffNationalCode(ZString dataGroupingCode, ZGuid tariffPK, ZString nationalCode, ZDateTime startDate, ZDateTime endDate, ZDate publishedDate, string description = null, string taxOrFeeCode = null, bool ensureDataGroupingExists = true)
		{
			var newFactory = new BusinessObjectFactory();
			var cusTariffNationalCode = CreateInternalRefCusTariffNationalCode(newFactory, dataGroupingCode, tariffPK, nationalCode, startDate, endDate, publishedDate, description, taxOrFeeCode, ensureDataGroupingExists);
			newFactory.Save();
			return factory.Load<TariffView>(cusTariffNationalCode.PK);
		}

		internal static Internal.RefCusTariffNationalCode CreateInternalRefCusTariffNationalCode(BusinessObjectFactory factory, ZString dataGroupingCode, ZGuid tariffPK, ZString nationalCode, ZDateTime startDate, ZDateTime endDate, ZDate publishedDate, string description = null, string taxOrFeeCode = null, bool ensureDataGroupingExists = true)
		{
			var tariffNationalCode = factory.New<Internal.RefCusTariffNationalCode>();
			if (ensureDataGroupingExists)
			{
				new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			}
			tariffNationalCode.ZZW_ZZ1_Tariff = tariffPK;
			tariffNationalCode.ZZW_ZZZ_NKDataGrouping = dataGroupingCode;
			tariffNationalCode.ZZW_Description = description ?? "Default Description";
			tariffNationalCode.ZZW_EndDate = endDate;
			tariffNationalCode.ZZW_NationalCode = nationalCode;
			tariffNationalCode.ZZW_PublishedDate = publishedDate;
			tariffNationalCode.ZZW_StartDate = startDate;
			tariffNationalCode.ZZW_ZZF_NKTaxOrFeeCode = taxOrFeeCode;
			return tariffNationalCode;
		}

		public TariffView LoadOrCreateNewTariff(ZString dataGroupingCode, ZGuid tariffTypePK, ZString tariffCode, ZDateTime startDate, ZDateTime endDate, string description = null, short iamUnique = 0, string taxOrFeeCode = null, string relatedTariffCode = null, string compositeKey = null, bool isSystem = true, bool ensureDataGroupingExists = true, string versionCode = "")
		{
			var tariffType = factory.Load<RefCusTariffType>(tariffTypePK).ZZI_TariffType;
			var query = new ZQuery(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(TariffViewSchema.ZZ1_TariffCode, tariffCode);

			if (isSystem)
			{
				query.AddToFilter(TariffViewSchema.ZZ1_ZZI_TariffType, tariffTypePK);
			}
			else
			{
				query.AddToFilter(TariffViewSchema.ZZ1_ZZI_NKTariffType, tariffType);
			}

			query.AddToFilter(TariffViewSchema.ZZ1_StartDate, startDate);
			query.AddToFilter(TariffViewSchema.ZZ1_IsSystem, isSystem);
			if (!string.IsNullOrEmpty(versionCode))
			{
				query.AddToFilter(TariffViewSchema.ZZ1_CRT_NKTariffVersion, versionCode);
			}

			var result = factory.LoadTop1<TariffView>(query);
			if (result == null)
			{
				if (isSystem)
				{
					var newTariff = CreateRefCusTariff(dataGroupingCode, tariffTypePK, tariffCode, startDate, endDate, description, iamUnique, taxOrFeeCode, relatedTariffCode, compositeKey, ensureDataGroupingExists);
					factory.Save();
					result = factory.Load<TariffView>(newTariff.PK);
				}
				else
				{
					var newTariff = CreateInternalManualTariff(dataGroupingCode, tariffType, tariffCode, startDate, endDate, description, taxOrFeeCode, versionCode, ensureDataGroupingExists);
					factory.Save();
					result = factory.Load<TariffView>(newTariff.PK);
				}
			}

			return result;
		}

		public TariffView CreateTariff(ZString dataGroupingCode, ZGuid tariffTypePK, ZString tariffCode, ZDateTime startDate, ZDateTime endDate, string description = null, short iamUnique = 0, string taxOrFeeCode = null, string relatedTariffCode = null, string compositeKey = null, bool ensureDataGroupingExists = true)
		{
			var newFactory = new BusinessObjectFactory();
			var cusTariff = CreateInternalRefCusTariff(newFactory, dataGroupingCode, tariffTypePK, tariffCode, startDate, endDate, description, iamUnique, taxOrFeeCode, relatedTariffCode, compositeKey, ensureDataGroupingExists);
			newFactory.Save();
			return factory.Load<TariffView>(cusTariff.PK);
		}

		internal Internal.RefCusTariff CreateRefCusTariff(ZString dataGroupingCode, ZGuid tariffTypePK, ZString tariffCode, ZDateTime startDate, ZDateTime endDate, string description = null, short iamUnique = 0, string taxOrFeeCode = null, string relatedTariffCode = null, string compositeKey = null, bool ensureDataGroupingExists = true)
		{
			return CreateInternalRefCusTariff(factory, dataGroupingCode, tariffTypePK, tariffCode, startDate, endDate, description, iamUnique, taxOrFeeCode, relatedTariffCode, compositeKey, ensureDataGroupingExists);
		}

		internal static Internal.RefCusTariff CreateInternalRefCusTariff(BusinessObjectFactory factory, ZString dataGroupingCode, ZGuid tariffTypePK, ZString tariffCode, ZDateTime startDate, ZDateTime endDate, string description = null, short iamUnique = 0, string taxOrFeeCode = null, string relatedTariffCode = null, string compositeKey = null, bool ensureDataGroupingExists = true)
		{
			var tariff = factory.New<Internal.RefCusTariff>();
			if (ensureDataGroupingExists)
			{
				new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			}
			tariff.ZZ1_ZZZ_NKDataGrouping = dataGroupingCode;
			tariff.ZZ1_ZZI_TariffType = tariffTypePK;
			tariff.ZZ1_TariffCode = tariffCode;
			tariff.ZZ1_StartDate = startDate;
			tariff.ZZ1_EndDate = endDate;
			tariff.ZZ1_Description = description ?? "Default Description";
			tariff.ZZ1_IAMUnique = iamUnique;
			tariff.ZZ1_ZZF_NKTaxOrFeeCode = taxOrFeeCode ?? ZString.Empty;
			tariff.ZZ1_CompositeKeyOnZZ5 = compositeKey ?? ZString.Empty;

			if (relatedTariffCode != null)
			{
				CreateInternalRefCusTariffRelationship(factory, tariff.PK, tariffTypePK, relatedTariffCode);
			}

			return tariff;
		}

		public TariffView CreateManualTariff(ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime startDate, ZDateTime endDate, string description = null, string taxOrFeeCode = null, string tariffVersion = null, bool ensureDataGroupingExists = true)
		{
			var newFactory = new BusinessObjectFactory();
			var tariff = CreateInternalManualTariff(newFactory, dataGroupingCode, tariffType, tariffCode, startDate, endDate, description, taxOrFeeCode, tariffVersion, ensureDataGroupingExists);
			newFactory.Save();
			return factory.Load<TariffView>(tariff.PK);
		}

		internal TariffView CreateInternalManualTariff(ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime startDate, ZDateTime endDate, string description = null, string taxOrFeeCode = null, string tariffVersion = null, bool ensureDataGroupingExists = true)
		{
			return CreateInternalManualTariff(factory, dataGroupingCode, tariffType, tariffCode, startDate, endDate, description, taxOrFeeCode, tariffVersion, ensureDataGroupingExists);
		}

		internal static TariffView CreateInternalManualTariff(BusinessObjectFactory factory, ZString dataGroupingCode, ZString tariffType, ZString tariffCode, ZDateTime startDate, ZDateTime endDate, string description = null, string taxOrFeeCode = null, string tariffVersion = null, bool ensureDataGroupingExists = true)
		{
			var tariff = factory.New<TariffView>();
			if (ensureDataGroupingExists)
			{
				new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			}
			tariff.ZZ1_ZZZ_NKDataGrouping = dataGroupingCode;
			tariff.ZZ1_ZZI_NKTariffType = tariffType;
			tariff.ZZ1_TariffCode = tariffCode;
			tariff.ZZ1_StartDate = startDate;
			tariff.ZZ1_EndDate = endDate;
			tariff.ZZ1_Description = description ?? "Default Description";
			tariff.ZZ1_ZZF_NKTaxOrFeeCode = taxOrFeeCode ?? ZString.Empty;
			if (!string.IsNullOrEmpty(tariffVersion))
			{
				tariff.ZZ1_CRT_NKTariffVersion = tariffVersion;
			}

			return tariff;
		}

		public TariffView CreateCommodity(TariffView tariff, ZString commodityCode, string dataGroupingCode, ZDateTime startDate = new ZDateTime(), ZDateTime endDate = new ZDateTime())
		{
			var commodityType = CreateNewOrGetExistingTariffType(dataGroupingCode, Constants.TariffTypes.Commodity);
			var hsnType = CreateNewOrGetExistingTariffType(dataGroupingCode, Constants.TariffTypes.HarmonizedSystem);
			factory.Save();
			startDate = !startDate.IsValid ? tariff.ZZ1_StartDate : startDate;
			endDate = !endDate.IsValid ? tariff.ZZ1_EndDate : endDate;
			var commodity = LoadOrCreateNewTariff(dataGroupingCode, commodityType.PK, commodityCode, startDate, endDate);
			CreateTariffRelationship(commodity.PK, hsnType.PK, tariff.ZZ1_TariffCode);
			return commodity;
		}

		public RateView CreateRate(TariffView tariffView, ZGuid rateCodePk, ZDateTime startDate, ZDateTime endDate, string rateFormula = null, ZGuid? preferencePk = null, string rateFormulaDeriveFrom = "", string dataGrouping = "", bool isSystem = true)
		{
			RateView rate;
			if (isSystem)
			{
				var newFactory = new BusinessObjectFactory();
				var refRate = CreateInternalRefCusRate(newFactory, tariffView.IsTariffNationalCode, tariffView.PK, rateCodePk, startDate, endDate, rateFormula, preferencePk, rateFormulaDeriveFrom, dataGrouping);
				newFactory.Save();
				rate = factory.Load<RateView>(refRate.PK);
			}
			else
			{
				var newFactory = new BusinessObjectFactory();
				rate = newFactory.New<RateView>();
				rate.ZZ2_ParentTableType = "CR2";
				rate.ZZ2_ZZ1_ParentTariffOrNationalCode = tariffView.PK;
				rate.ZZ2_ZY1_RateCode = rateCodePk;
				rate.ZZ2_StartDate = startDate;
				rate.ZZ2_EndDate = endDate;
				rate.ZZ2_RateFormula = rateFormula ?? "0";
				rate.ZZ2_ZZS_Preference = preferencePk.GetValueOrDefault();
				rate.ZZ2_RateFormulaDerivedFrom = rateFormulaDeriveFrom ?? ZString.Empty;
				rate.ZZ2_ZZZ_NKDataGrouping = dataGrouping;
				newFactory.Save();
			}
			return rate;
		}

		public Internal.RefCusRate CreateRefCusRate(ZGuid refCusTariffPk, ZGuid rateCodePk, ZDateTime startDate, ZDateTime endDate, string rateFormula = null, ZGuid? preferencePk = null, string rateFormulaDeriveFrom = "", string dataGrouping = "")
		{
			return CreateInternalRefCusRate(new BusinessObjectFactory(), isTariffNationalCode: false, refCusTariffPk, rateCodePk, startDate, endDate, rateFormula, preferencePk, rateFormulaDeriveFrom, dataGrouping);
		}

		internal static Internal.RefCusRate CreateInternalRefCusRate(BusinessObjectFactory factory, bool isTariffNationalCode, ZGuid parentPk, ZGuid rateCodePk, ZDateTime startDate, ZDateTime endDate, string rateFormula = null, ZGuid? preferencePk = null, string rateFormulaDeriveFrom = "", string dataGrouping = "")
		{
			var rate = factory.New<Internal.RefCusRate>();
			if (isTariffNationalCode)
			{
				rate.ZZ2_ZZW_TariffNationalCode = parentPk;
			}
			else
			{
				rate.ZZ2_ZZ1_Tariff = parentPk;
			}
			rate.ZZ2_ZY1_RateCode = rateCodePk;
			rate.ZZ2_StartDate = startDate;
			rate.ZZ2_EndDate = endDate;
			rate.ZZ2_RateFormula = rateFormula ?? "0";
			rate.ZZ2_ZZS_Preference = preferencePk.GetValueOrDefault();
			rate.ZZ2_RateFormulaDerivedFrom = rateFormulaDeriveFrom ?? ZString.Empty;
			rate.ZZ2_ZZZ_NKDataGrouping = dataGrouping;
			factory.Save();
			return rate;
		}

		public CusRefRateCodeView CreateCusRateCode(BusinessObjectFactory factory, ZString zy1RateCode, ZGuid refCusRateTypePK, bool isSystem = true, bool internalUse = false, string description = "", string cusRateType = "", string countryCode = "")
		{
			CusRefRateCodeView rateCode;
			if (isSystem)
			{
				var refRateCode = CreateInternalRefCusRateCode(zy1RateCode, refCusRateTypePK, internalUse, description);
				rateCode = factory.Load<CusRefRateCodeView>(refRateCode.PK);
			}
			else
			{
				if (internalUse)
				{
					throw new ArgumentException("internalUse property can be set only on System Rate Codes (isSystem = true)");
				}

				rateCode = factory.New<CusRefRateCodeView>();
				rateCode.ZY1_RateCode = zy1RateCode;
				rateCode.ZY1_Description = string.IsNullOrEmpty(description) ? zy1RateCode + " DESC" : description;
				rateCode.ZY1_RateType = cusRateType;  // CusRateType is not from DB table but RefCusRateTypeCustomizableList
				rateCode.ZY1_ZZZ_NKDataGrouping = countryCode;
				factory.Save();
			}

			return rateCode;
		}

		RefCusRateCode CreateInternalRefCusRateCode(ZString zy1RateCode, ZGuid refCusRateTypePK, bool internalUse = false, string description = "")
		{
			var newFactory = new BusinessObjectFactory();

			var zy1 = newFactory.New<RefCusRateCode>();
			zy1.ZY1_RateCode = zy1RateCode;
			zy1.ZY1_ZZR_RateType = refCusRateTypePK;
			zy1.ZY1_InternalUse = internalUse;
			zy1.ZY1_Description = string.IsNullOrEmpty(description) ? zy1RateCode + " DESC" : description;
			newFactory.Save();

			return zy1;
		}

		public CusRefRateCodeView LoadOrCreateNewCusRateCode(BusinessObjectFactory factory, ZString zy1RateCode, ZGuid refCusRateTypePK, bool isSystem = true, bool internalUse = false, string description = "", string cusRateType = "", string countryCode = "")
		{
			var rateType = cusRateType;
			var dataGrouping = countryCode;
			if (isSystem)
			{
				var refRateType = factory.Load<RefCusRateType>(refCusRateTypePK);
				rateType = refRateType.ZZR_RateType;
				dataGrouping = refRateType.ZZR_ZZZ_NKDataGrouping;
			}
			var query = new ZQuery(CusRefRateCodeViewSchema.ZY1_RateCode, zy1RateCode);
			query.AddToFilter(CusRefRateCodeViewSchema.ZY1_RateType, rateType);
			query.AddToFilter(CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, dataGrouping);
			query.AddToFilter(CusRefRateCodeViewSchema.ZY1_IsSystem, isSystem);
			var zy1 = factory.LoadTop1<CusRefRateCodeView>(query) ?? CreateCusRateCode(factory, zy1RateCode, refCusRateTypePK, isSystem, internalUse, description, cusRateType, countryCode);
			return zy1;
		}

		public RefCusRateCodeLanguage LoadOrCreateNewCusRateCodeLanguage(BusinessObjectFactory factory, ZString zy1RateCode, ZString language)
		{
			var codeQuery = new ZQuery(CusRefRateCodeViewSchema.ZY1_RateCode, zy1RateCode);
			var rateCode = factory.LoadTop1<CusRefRateCodeView>(codeQuery);

			return LoadOrCreateNewCusRateCodeLanguage(factory, rateCode, language);
		}

		public RefCusRateCodeLanguage LoadOrCreateNewCusRateCodeLanguage(BusinessObjectFactory factory, CusRefRateCodeView rateCode, ZString language)
		{
			var languageQuery = new ZQuery(RefCusRateCodeLanguageSchema.ZXC_ZY1_RateCode, rateCode.PK);
			var rateLanguage = factory.LoadTop1<RefCusRateCodeLanguage>(languageQuery);
			if (rateLanguage == null)
			{
				rateLanguage = factory.New<RefCusRateCodeLanguage>();
				rateLanguage.ZXC_ZX6_NKLanguage = language;
				rateLanguage.ZXC_ZY1_RateCode = rateCode.PK;
				rateLanguage.ZXC_Description = rateCode.ZY1_RateCode + language + " DESC";
			}

			return rateLanguage;
		}

		public CusRefTariffLanguageView LoadOrCreateNewCusRefTariffLanguageView(BusinessObjectFactory factory, ZGuid tariffGuid, ZString language, ZString description, bool isSystem = true)
		{
			var languageQuery = new ZQuery();
			languageQuery.AddToFilter(CusRefTariffLanguageViewSchema.ZX7_ZZ1_Tariff, tariffGuid);
			languageQuery.AddToFilter(CusRefTariffLanguageViewSchema.ZX7_ZX6_NKLanguage, language);
			languageQuery.AddToFilter(CusRefTariffLanguageViewSchema.ZX7_IsSystem, isSystem);

			var tariffLanguage = factory.LoadTop1<CusRefTariffLanguageView>(languageQuery);
			if (tariffLanguage == null)
			{
				if (isSystem)
				{
					var refTariffLanguage = CreateInternalRefCusTariffLanguage(tariffGuid, language, description);
					tariffLanguage = factory.Load<CusRefTariffLanguageView>(refTariffLanguage.PK);
				}
				else
				{
					tariffLanguage = factory.New<CusRefTariffLanguageView>();
					tariffLanguage.ZX7_ZZ1_Tariff = tariffGuid;
					tariffLanguage.ZX7_ZX6_NKLanguage = language;
					tariffLanguage.ZX7_Description = description;
				}
			}

			return tariffLanguage;
		}

		RefCusTariffLanguage CreateInternalRefCusTariffLanguage(ZGuid tariffGuid, ZString language, ZString description)
		{
			var factory = new BusinessObjectFactory();
			var tariffLanguage = factory.New<RefCusTariffLanguage>();
			tariffLanguage.ZX7_ZZ1_Tariff = tariffGuid;
			tariffLanguage.ZX7_ZX6_NKLanguage = language;
			tariffLanguage.ZX7_Description = description;
			factory.Save();
			return tariffLanguage;
		}

		public CusRefApplicabilityView CreateCusApplicability(Internal.RefCusRate rate, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", bool isSystem = true, CusRefTradeGroupView secondTradeGroup = null)
		{
			return CreateCusApplicability(factory.Load<RateView>(rate.PK), tradeGroup, startDate, endDate, additionalCode, orderNumber, isSystem, secondTradeGroup);
		}

		public CusRefApplicabilityView CreateCusApplicability(ZGuid ratePk, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			return CreateCusApplicability(factory.Load<RateView>(ratePk), tradeGroup, startDate, endDate, additionalCode, orderNumber, true, secondTradeGroup);
		}

		public CusRefApplicabilityView CreateCusApplicability(RateView rate, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", bool isSystem = true, CusRefTradeGroupView secondTradeGroup = null)
		{
			CusRefApplicabilityView result;
			if (isSystem)
			{
				var newApp = CreateCusApplicabilityInternal(rate, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
				newApp.Factory.Save();

				result = factory.Load<CusRefApplicabilityView>(newApp.PK);
			}
			else
			{
				var view = factory.New<CusRefApplicabilityView>();
				view.ZZT_ZZ2_Rate = rate.PK;
				view.ZZT_StartDate = startDate;
				view.ZZT_EndDate = endDate;
				view.ZZT_OrderNumber = orderNumber;
				view.ZZT_AdditionalCode = additionalCode;
				if (tradeGroup != null)
				{
					view.ZZT_ZZA_TradeGroup = tradeGroup.PK;
				}
				if (secondTradeGroup != null)
				{
					view.ZZT_ZZA_SecondTradeGroup = secondTradeGroup.PK;
				}
				result = factory.Load<CusRefApplicabilityView>(view.PK);
			}
			return result;
		}

		internal Internal.RefCusApplicability CreateNewOrLoadExistingCusApplicability(RateView rate, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", bool isSystem = true, CusRefTradeGroupView secondTradeGroup = null)
		{
			var query = new ZQuery(RefCusApplicabilitySchema.ZZT_ZZ2_Rate, rate.PK);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_ZZA_TradeGroup, tradeGroup.PK);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_StartDate, startDate);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_EndDate, endDate);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_AdditionalCode, additionalCode);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_OrderNumber, orderNumber);
			if (secondTradeGroup != null)
			{
				query.AddToFilter(RefCusApplicabilitySchema.ZZT_ZZA_SecondTradeGroup, secondTradeGroup.PK);
			}

			var result = factory.LoadTop1<Internal.RefCusApplicability>(query) ?? CreateCusApplicabilityInternal(rate, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
			return result;
		}

		internal Internal.RefCusApplicability CreateCusApplicabilityInternal(RateView rate, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			var applicability = factory.New<Internal.RefCusApplicability>();
			applicability.ZZT_ZZ2_Rate = rate.PK;
			PopulateCusApplicability(applicability, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
			return applicability;
		}

		public CusRefApplicabilityView CreateCusApplicability(TariffAdditionalCodeView tariffAdditionalCode, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", bool isSystem = true, CusRefTradeGroupView secondTradeGroup = null)
		{
			CusRefApplicabilityView result;
			if (isSystem)
			{
				var newApp = CreateCusApplicabilityInternal(tariffAdditionalCode, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
				newApp.Factory.Save();

				result = factory.Load<CusRefApplicabilityView>(newApp.PK);
			}
			else
			{
				var view = factory.New<CusRefApplicabilityView>();
				view.ZZT_ZY2_AdditionalCode = tariffAdditionalCode.PK;
				view.ZZT_StartDate = startDate;
				view.ZZT_EndDate = endDate;
				view.ZZT_OrderNumber = orderNumber;
				view.ZZT_AdditionalCode = additionalCode;
				if (tradeGroup != null)
				{
					view.ZZT_ZZA_TradeGroup = tradeGroup.PK;
				}
				if (secondTradeGroup != null)
				{
					view.ZZT_ZZA_SecondTradeGroup = secondTradeGroup.PK;
				}
				result = factory.Load<CusRefApplicabilityView>(view.PK);
			}
			return result;
		}

		internal Internal.RefCusApplicability CreateNewOrLoadExistingCusApplicability(TariffAdditionalCodeView tariffAdditionalCode, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", bool isSystem = true, CusRefTradeGroupView secondTradeGroup = null)
		{
			var query = new ZQuery(RefCusApplicabilitySchema.ZZT_ZY2_AdditionalCode, tariffAdditionalCode.PK);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_ZZA_TradeGroup, tradeGroup.PK);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_StartDate, startDate);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_EndDate, endDate);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_AdditionalCode, additionalCode);
			query.AddToFilter(RefCusApplicabilitySchema.ZZT_OrderNumber, orderNumber);
			if (secondTradeGroup != null)
			{
				query.AddToFilter(RefCusApplicabilitySchema.ZZT_ZZA_SecondTradeGroup, secondTradeGroup.PK);
			}

			var result = factory.LoadTop1<Internal.RefCusApplicability>(query) ?? CreateCusApplicabilityInternal(tariffAdditionalCode, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
			return result;
		}

		internal Internal.RefCusApplicability CreateCusApplicabilityInternal(TariffAdditionalCodeView tariffAdditionalCode, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			var applicability = factory.New<Internal.RefCusApplicability>();
			applicability.ZZT_ZY2_AdditionalCode = tariffAdditionalCode.PK;
			PopulateCusApplicability(applicability, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
			return applicability;
		}

		public CusRefApplicabilityView CreateCusApplicability(RefCusCondition condition, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			var newItem = CreateCusApplicabilityInternal(condition, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
			newItem.Factory.Save();
			return factory.Load<CusRefApplicabilityView>(newItem.PK);
		}

		internal Internal.RefCusApplicability CreateCusApplicabilityInternal(RefCusCondition condition, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			var newFactory = new BusinessObjectFactory();
			var applicability = newFactory.New<Internal.RefCusApplicability>();
			applicability.ZZT_ZX1_Conditions = condition.PK;
			PopulateCusApplicability(applicability, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
			newFactory.Save();
			return applicability;
		}

		public CusRefApplicabilityView CreateCusApplicability(Internal.RefCusTariffAdditionalCode tariffAdditionalCode, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			var newItem = CreateCusApplicabilityInternal(tariffAdditionalCode, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
			newItem.Factory.Save();
			return factory.Load<CusRefApplicabilityView>(newItem.PK);
		}

		internal Internal.RefCusApplicability CreateCusApplicabilityInternal(Internal.RefCusTariffAdditionalCode tariffAdditionalCode, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			var newFactory = new BusinessObjectFactory();
			var applicability = newFactory.New<Internal.RefCusApplicability>();
			applicability.ZZT_ZY2_AdditionalCode = tariffAdditionalCode.PK;
			PopulateCusApplicability(applicability, tradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup);
			newFactory.Save();
			return applicability;
		}

		void PopulateCusApplicability(Internal.RefCusApplicability applicability, CusRefTradeGroupView tradeGroup, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string orderNumber = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			applicability.ZZT_EndDate = endDate;
			applicability.ZZT_StartDate = startDate;
			applicability.ZZT_AdditionalCode = additionalCode;
			applicability.ZZT_OrderNumber = orderNumber;

			if (tradeGroup != null)
			{
				applicability.ZZT_ZZA_TradeGroup = tradeGroup.PK;
			}

			if (secondTradeGroup != null)
			{
				applicability.ZZT_ZZA_SecondTradeGroup = secondTradeGroup.PK;
			}
		}

		public CusRefTradeGroupCountryView AddNewOrExistingCountry(CusRefTradeGroupView tradeGroup, ZString tradeAgreementCountryCode, ZDate? startDate = null, ZDate? endDate = null, string countryCodeDescription = "")
		{
			var query = new ZQuery(CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup, tradeGroup.PK);
			query.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_RN_NKTradeGroupCountryCode, tradeAgreementCountryCode);
			query.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_StartDate, startDate != null && startDate.HasValue ? startDate.Value : ZDateTime.MinSmallDateTimeValue.Date);
			query.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_EndDate, endDate != null && endDate.HasValue ? endDate.Value : ZDateTime.MaxSmallDateTimeValue.Date);
			return factory.LoadTop1<CusRefTradeGroupCountryView>(query) ?? AddCountry(tradeGroup, tradeAgreementCountryCode, startDate, endDate, countryCodeDescription);
		}

		BusinessObject Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper.AddCountry(BusinessObject tradeGroup, ZString tradeAgreementCountryCode, ZDate? startDate, ZDate? endDate, string countryCodeDescription) => AddCountry((CusRefTradeGroupView)tradeGroup, tradeAgreementCountryCode, startDate, endDate, countryCodeDescription);
		public CusRefTradeGroupCountryView AddCountry(CusRefTradeGroupView tradeGroup, ZString tradeAgreementCountryCode, ZDate? startDate = null, ZDate? endDate = null, string countryCodeDescription = "")
		{
			CusRefTradeGroupCountryView tradeGroupCountry;
			if (tradeGroup.ZZA_IsSystem)
			{
				var newFactory = new BusinessObjectFactory();
				var refCountry = newFactory.New<RefCusTradeGroupCountry>();
				refCountry.ZZB_ZZA_TradeGroup = tradeGroup.PK;
				refCountry.ZZB_RN_NKTradeGroupCountryCode = tradeAgreementCountryCode;
				refCountry.ZZB_Description = countryCodeDescription;
				refCountry.ZZB_StartDate = startDate != null && startDate.HasValue ? startDate.Value : ZDateTime.MinSmallDateTimeValue.Date;
				refCountry.ZZB_EndDate = endDate != null && endDate.HasValue ? endDate.Value : ZDateTime.MaxSmallDateTimeValue.Date;
				newFactory.Save();
				tradeGroupCountry = factory.Load<CusRefTradeGroupCountryView>(refCountry.PK);
			}
			else
			{
				tradeGroupCountry = tradeGroup.TradeGroupCountries.AddNew();
				tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode = tradeAgreementCountryCode;
				tradeGroupCountry.ZZB_Description = countryCodeDescription;
				tradeGroupCountry.ZZB_StartDate = startDate != null && startDate.HasValue ? startDate.Value : ZDateTime.MinSmallDateTimeValue.Date;
				tradeGroupCountry.ZZB_EndDate = endDate != null && endDate.HasValue ? endDate.Value : ZDateTime.MaxSmallDateTimeValue.Date;
				tradeGroupCountry.ZZB_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
			}
			return tradeGroupCountry;
		}

		public CusRefTradeGroupView LoadOrCreateTradeGroup(string countryCode, string tradeGroup, ZDateTime? startDate = null, ZDateTime? endDate = null, string description = null, bool ensureDataGroupingExists = true, bool isSystem = true)
		{
			var query = new ZQuery(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, countryCode);
			query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, tradeGroup);
			query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_IsSystem, isSystem);
			var result = factory.LoadTop1<CusRefTradeGroupView>(query);

			if (result == null)
			{
				var startDateTime = startDate != null && startDate.HasValue ? startDate.Value : ZDateTime.MinSmallDateTimeValue;
				var endDateTime = endDate != null && endDate.HasValue ? endDate.Value : ZDateTime.MaxSmallDateTimeValue;
				result = CreateTradeGroup(countryCode, tradeGroup, startDateTime, endDateTime, description, ensureDataGroupingExists, isSystem);
			}
			return result;
		}

		BusinessObject Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper.CreateTradeGroup(ZString dataGroupingCode, ZString tradeGroup, ZDateTime startDate, ZDateTime endDate, string description, bool ensureDataGroupingExists, bool isSystem) => CreateTradeGroup(dataGroupingCode, tradeGroup, startDate, endDate, description, ensureDataGroupingExists, isSystem);
		public CusRefTradeGroupView CreateTradeGroup(ZString dataGroupingCode, ZString tradeGroup, ZDateTime startDate, ZDateTime endDate, string description = null, bool ensureDataGroupingExists = true, bool isSystem = true)
		{
			CusRefTradeGroupView result;
			if (isSystem)
			{
				var refTradeGroup = CreateInternalTradeGroup(dataGroupingCode, tradeGroup, startDate, endDate, description, ensureDataGroupingExists);
				result = factory.Load<CusRefTradeGroupView>(refTradeGroup.PK);
			}
			else
			{
				result = factory.New<CusRefTradeGroupView>();
				if (ensureDataGroupingExists)
				{
					CreateNewOrGetExistingDataGrouping(dataGroupingCode);
				}
				result.ZZA_ZZZ_NKDataGrouping = dataGroupingCode;
				result.ZZA_TradeGroup = tradeGroup;
				result.ZZA_Description = description ?? (dataGroupingCode + tradeGroup + " DESC");
				result.ZZA_StartDate = startDate;
				result.ZZA_EndDate = endDate;
				factory.Save();
			}
			return result;
		}

		RefCusTradeGroup CreateInternalTradeGroup(ZString dataGroupingCode, ZString tradeGroup, ZDateTime startDate, ZDateTime endDate, string description = null, bool ensureDataGroupingExists = true)
		{
			var newFactory = new BusinessObjectFactory();
			var result = newFactory.New<RefCusTradeGroup>();

			if (ensureDataGroupingExists)
			{
				CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			}
			result.ZZA_ZZZ_NKDataGrouping = dataGroupingCode;
			result.ZZA_TradeGroup = tradeGroup;
			result.ZZA_Description = description ?? (dataGroupingCode + tradeGroup + " DESC");
			result.ZZA_StartDate = startDate;
			result.ZZA_EndDate = endDate;
			newFactory.Save();

			return result;
		}

		public RefCusExcludedTradeGroup CreateExcludedTradeGroup(CusRefTradeGroupView tradeGroup, CusRefApplicabilityView applicability = null)
		{
			var result = factory.New<RefCusExcludedTradeGroup>();
			result.ZZC_ZZA_TradeGroup = tradeGroup.PK;
			if (applicability != null)
			{
				result.ZZC_ZZT_Applicability = applicability.PK;
			}
			return result;
		}

		#region CusCodeType

		public RefCusCodeType CreateCusCodeType(ZString code, ZString desc, string dataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, byte maxLength = 0, bool allowCreationOfFuncsOrPFunc = false, bool isReadonly = false)
		{
			if (!allowCreationOfFuncsOrPFunc && (code == Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC || code == Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS)) // set allowCreationOfFuncsOrPFunc to true for setting data for reports or sql scripts
			{
				throw new DeveloperNotificationException("If using FUNCS or PFUNC codes in your test, because ZZCustomsFunctionalityEffectiveDate.FunctionalityKeys is static, please use ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality to override any possible code created in a previous test");
			}

			var result = factory.New<RefCusCodeType>();
			result.ZZK_CodeType = code;
			result.ZZK_Description = desc;
			result.ZZK_MaxLength = maxLength;
			result.ZZK_ZZZ_NKDataGrouping = dataGrouping;
			result.ZZK_IsReadonly = isReadonly;
			return result;
		}

		BusinessObject Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeType(ZString code, ZString desc, string dataGrouping, byte maxLength, bool allowCreationOfFuncsOrPFunc) => CreateNewOrGetExistingCusCodeType(code, desc, dataGrouping, maxLength, allowCreationOfFuncsOrPFunc);

		public RefCusCodeType CreateNewOrGetExistingCusCodeType(ZString code, ZString desc, string dataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, byte maxLength = 0, bool allowCreationOfFuncsOrPFunc = false)
		{
			var query = new ZQuery(RefCusCodeTypeSchema.ZZK_CodeType, code);
			query.AddToFilter(RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping, dataGrouping);
			var cusCodeType = factory.LoadTop1<RefCusCodeType>(query);
			if (cusCodeType != null)
			{
				if (maxLength != 0 && cusCodeType.ZZK_MaxLength != maxLength)
				{
					cusCodeType.ZZK_MaxLength = maxLength;
				}
			}
			else
			{
				cusCodeType = CreateCusCodeType(code, desc, dataGrouping, maxLength, allowCreationOfFuncsOrPFunc: allowCreationOfFuncsOrPFunc);
			}
			return cusCodeType;
		}

		public RefCusCodeTypeLanguage CreateCusCodeTypeLanguage(RefCusCodeType type, string languageCode, string localLanguageDescription)
		{
			var cusCodeTypeLanguage = factory.New<RefCusCodeTypeLanguage>();
			cusCodeTypeLanguage.ZXI_ZX6_NKLanguage = languageCode;
			cusCodeTypeLanguage.ZXI_ZZK_CodeType = type.PK;
			cusCodeTypeLanguage.ZXI_Description = localLanguageDescription;

			return cusCodeTypeLanguage;
		}

		public void DeleteAllCusCodeTypes()
		{
			DeleteTable(RefCusCodeType.Schema.TableName);
		}

		#endregion

		#region CusCodeList

		public RefCusCodeList CreateCusCodeList(ZString dataGroupingCode, ZString codeType, ZString code, ZDateTime startDate, ZDateTime endDate)
		{
			return CreateCusCodeList(dataGroupingCode, codeType, code, code + " DESC", startDate, endDate);
		}

		public RefCusCodeList CreateCusCodeList(ZString dataGroupingCode, ZString codeType, ZString code, ZString description, ZDateTime startDate, ZDateTime endDate)
		{
			var cusCodeList = factory.New<RefCusCodeList>();
			CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			CreateNewOrGetExistingCusCodeType(codeType, codeType, dataGroupingCode);
			cusCodeList.ZZD_ZZZ_NKDataGrouping = dataGroupingCode;
			cusCodeList.ZZD_ZZK_NKCodeType = codeType;
			cusCodeList.ZZD_Code = code;
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_StartDate = startDate;
			cusCodeList.ZZD_EndDate = endDate;
			return cusCodeList;
		}

		public RefCusCodeList CreateCusCodeListWithAttributeNames(ZString dataGroupingCode, ZString codeType, ZString code, ZString description, ZDateTime startDate, ZDateTime endDate, KeyValuePair<string, string>[] attributeNameAndDescriptionKeyValuePairs)
		{
			var cusCodeList = CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, code, description, startDate, endDate);
			foreach (var attributeNameAndDescription in attributeNameAndDescriptionKeyValuePairs)
			{
				CreateNewOrGetExistingRefCusCodeListAttributeName(attributeNameAndDescription.Key, attributeNameAndDescription.Value, codeType, dataGroupingCode);
			}

			return cusCodeList;
		}

		public RefCusCodeList CreateCusCodeListWithAttribute(ZString dataGroupingCode, ZString codeType, ZString code, ZString description, ZDateTime startDate, ZDateTime endDate, ZString attributeName, ZString attributeValue)
		{
			var cusCodeList = CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, code, description, startDate, endDate);
			CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, attributeName, attributeValue);
			return cusCodeList;
		}

		BusinessObject Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeList(ZString dataGroupingCode, ZString codeType, ZString code, ZString description, ZDateTime startDate, ZDateTime endDate) => CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, code, description, startDate, endDate);

		public RefCusCodeList CreateNewOrGetExistingCusCodeList(ZString dataGroupingCode, ZString codeType, ZString code, ZDateTime startDate, ZDateTime endDate)
		{
			return CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, code, code + " DESC", startDate, endDate);
		}

		public RefCusCodeList CreateNewOrGetExistingCusCodeList(ZString dataGroupingCode, ZString codeType, ZString code, ZString description, ZDateTime startDate, ZDateTime endDate)
		{
			if (code == Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC || code == Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS)
			{
				throw new DeveloperNotificationException("If using FUNCS or PFUNC codes in your test, because ZZCustomsFunctionalityEffectiveDate.FunctionalityKeys is static, please use ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality to override any possible code created in a previous test");
			}
			var query = new ZQuery();
			query.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, codeType);
			query.AddToFilter(RefCusCodeListSchema.ZZD_Code, code);

			var cusCodeList = factory.LoadTop1<RefCusCodeList>(query) ?? CreateCusCodeList(dataGroupingCode, codeType, code, description, startDate, endDate);
			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_StartDate = startDate;
			cusCodeList.ZZD_EndDate = endDate;
			return cusCodeList;
		}

		public void CreateCusCodeListsForMultipleTypesWithAttributes(ZString dataGroupingCode, string[] codeTypes, ZString code, ZString description, Dictionary<string, string[]> attributeNameValuePairs, ZDateTime startDate, ZDateTime endDate)
		{
			foreach (var type in codeTypes)
			{
				CreateNewOrGetExistingCusCodeType(type, type + " DESC");
				var cusCodeList = CreateNewOrGetExistingCusCodeList(dataGroupingCode, type, code, description, startDate, endDate);
				foreach (var attribute in attributeNameValuePairs)
				{
					CreateNewOrGetExistingRefCusCodeListAttributeName(attribute.Key, "Desc.", type, dataGroupingCode, type);
					foreach (var value in attribute.Value)
					{
						CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, attribute.Key, value);
					}
				}
			}
		}

		#endregion

		#region CusCodeListAttribute

		public RefCusCodeListAttribute CreateCusCodeListAttribute(ZGuid cusCodeListPK, ZString attributeName, ZString attributeValue) => CreateCusCodeListAttribute(cusCodeListPK, attributeName, attributeValue, createAttributeName: true);

		public RefCusCodeListAttribute CreateCusCodeListAttribute(ZGuid cusCodeListPK, ZString attributeName, ZString attributeValue, bool createAttributeName = true)
		{
			if (createAttributeName)
			{
				var cusCodeList = factory.Load<RefCusCodeList>(cusCodeListPK);
				CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, "Desc.", cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping, cusCodeList.ZZD_ZZK_NKCodeType);
			}

			var cusCodeListAttrib = factory.New<RefCusCodeListAttribute>();
			cusCodeListAttrib.ZZE_ZZD_CodeList = cusCodeListPK;
			cusCodeListAttrib.ZZE_ZXE_NKName = attributeName;
			cusCodeListAttrib.ZZE_Value = attributeValue;

			return cusCodeListAttrib;
		}

		public RefCusCodeListAttribute CreateCusCodeListAttribute(ZGuid cusCodeListPK, ZString attributeName, ZString attributeValue, ZDateTime startDate, ZDateTime endDate, bool createAttributeName = true)
		{
			if (createAttributeName)
			{
				var cusCodeList = factory.Load<RefCusCodeList>(cusCodeListPK);
				CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, "Desc.", cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping, cusCodeList.ZZD_ZZK_NKCodeType, isDateRangeUsed: true);
			}

			var cusCodeListAttrib = factory.New<RefCusCodeListAttribute>();
			cusCodeListAttrib.ZZE_ZZD_CodeList = cusCodeListPK;
			cusCodeListAttrib.ZZE_ZXE_NKName = attributeName;
			cusCodeListAttrib.ZZE_Value = attributeValue;
			cusCodeListAttrib.ZZE_StartDate = startDate;
			cusCodeListAttrib.ZZE_EndDate = endDate;

			return cusCodeListAttrib;
		}

		BusinessObject Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeListAttribute(ZGuid cusCodeListPK, ZString attributeName, ZString attributeValue) => CreateNewOrGetExistingCusCodeListAttribute(cusCodeListPK, attributeName, attributeValue);

		public RefCusCodeListAttribute CreateNewOrGetExistingCusCodeListAttribute(ZGuid cusCodeListPK, ZString attributeName, ZString attributeValue) => CreateNewOrGetExistingCusCodeListAttribute(cusCodeListPK, attributeName, attributeValue, true);

		public RefCusCodeListAttribute CreateNewOrGetExistingCusCodeListAttribute(ZGuid cusCodeListPK, ZString attributeName, ZString attributeValue, bool createAttributeName = true)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList, cusCodeListPK);
			query.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZXE_NKName, attributeName);
			query.AddToFilter(RefCusCodeListAttributeSchema.ZZE_Value, attributeValue);

			var loadedCusCodeListAttribute = factory.LoadTop1<RefCusCodeListAttribute>(query);
			if (createAttributeName && loadedCusCodeListAttribute != null)
			{
				var cusCodeList = factory.Load<RefCusCodeList>(cusCodeListPK);
				CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, "Desc.", cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping, cusCodeList.ZZD_ZZK_NKCodeType);
			}

			var cusCodeListAttribute = loadedCusCodeListAttribute ?? CreateCusCodeListAttribute(cusCodeListPK, attributeName, attributeValue);
			return cusCodeListAttribute;
		}

		public RefCusCodeListAttribute CreateNewOrGetExistingCusCodeListAttribute(ZGuid cusCodeListPK, ZString attributeName, ZString attributeValue, ZDateTime startDate, ZDateTime endDate, bool createAttributeName = true)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList, cusCodeListPK);
			query.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZXE_NKName, attributeName);
			query.AddToFilter(RefCusCodeListAttributeSchema.ZZE_Value, attributeValue);
			query.AddToFilter(RefCusCodeListAttributeSchema.ZZE_StartDate, startDate);
			query.AddToFilter(RefCusCodeListAttributeSchema.ZZE_EndDate, endDate);

			var loadedCusCodeListAttribute = factory.LoadTop1<RefCusCodeListAttribute>(query);
			if (createAttributeName && loadedCusCodeListAttribute != null)
			{
				var cusCodeList = factory.Load<RefCusCodeList>(cusCodeListPK);
				CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, "Desc.", cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping, cusCodeList.ZZD_ZZK_NKCodeType, isDateRangeUsed: true);
			}

			var cusCodeListAttribute = loadedCusCodeListAttribute ?? CreateCusCodeListAttribute(cusCodeListPK, attributeName, attributeValue, startDate, endDate);
			return cusCodeListAttribute;
		}

		#endregion

		#region CusCodeListAttributeName

		public RefCusCodeListAttributeName CreateNewOrGetExistingRefCusCodeListAttributeName(string name, string description, string codeType, string countryCode, string codeTypeForValueList = null, bool? isMandatory = false, bool? allowDuplicates = false, bool isValueMandatory = false, bool isDateRangeUsed = false)
		{
			var query = new ZQuery(RefCusCodeListAttributeNameSchema.ZXE_Name, name);
			query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_ZZK_NKCodeType, codeType);
			query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_ZZZ_NKDataGrouping, countryCode);
			var attrName = factory.LoadTop1<RefCusCodeListAttributeName>(query);

			if (attrName == null)
			{
				attrName = factory.New<RefCusCodeListAttributeName>();
				attrName.ZXE_Name = name;
				attrName.ZXE_ZZK_NKCodeType = codeType;
				attrName.ZXE_ZZZ_NKDataGrouping = countryCode;
				attrName.ZXE_ZZK_NKCodeTypeForValueList = codeType;
				attrName.ZXE_ColumnCaption = name;
			}

			attrName.ZXE_Description = description;
			attrName.ZXE_ZZK_NKCodeTypeForValueList = codeTypeForValueList ?? attrName.ZXE_ZZK_NKCodeTypeForValueList;
			attrName.ZXE_IsMandatory = isMandatory ?? attrName.ZXE_IsMandatory;
			attrName.ZXE_AllowDuplicates = allowDuplicates ?? attrName.ZXE_AllowDuplicates;
			attrName.ZXE_IsValueMandatory = isValueMandatory;
			attrName.ZXE_IsDateRangeUsed = isDateRangeUsed;

			return attrName;
		}

		public RefCusCodeListAttributeNameLanguage CreateCusCodeListAttributeNameLanguage(RefCusCodeListAttributeName attributeName, string languageCode, string localLanguageName, string localLanguageDescription, string localLanguageColumnCaption)
		{
			var attributeNameLanguage = factory.New<RefCusCodeListAttributeNameLanguage>();
			attributeNameLanguage.ZXH_ZX6_NKLanguage = languageCode;
			attributeNameLanguage.ZXH_ZXE_CodeListAttributeName = attributeName.PK;
			attributeNameLanguage.ZXH_Name = localLanguageName;
			attributeNameLanguage.ZXH_Description = localLanguageDescription;
			attributeNameLanguage.ZXH_ColumnCaption = localLanguageColumnCaption;

			return attributeNameLanguage;
		}

		#endregion

		#region CusCodeOrAttributeTransportMode

		public RefCusCodeOrAttributeTransportMode CreateTransportModeForCusCodeList(ZGuid cusCodeListPK, ZString transportMode)
		{
			var result = factory.New<RefCusCodeOrAttributeTransportMode>();
			result.ZZU_ZZD_CodeList = cusCodeListPK;
			result.ZZU_TransportMode = transportMode;
			return result;
		}

		public RefCusCodeOrAttributeTransportMode CreateNewOrGetExistingTransportModeForCusCodeList(ZGuid cusCodeListPK, ZString transportMode)
		{
			var query = new ZQuery()
				.AddToFilter(RefCusCodeOrAttributeTransportModeSchema.ZZU_ZZD_CodeList, cusCodeListPK)
				.AddToFilter(RefCusCodeOrAttributeTransportModeSchema.ZZU_TransportMode, transportMode);

			return factory.LoadTop1<RefCusCodeOrAttributeTransportMode>(query) ?? CreateTransportModeForCusCodeList(cusCodeListPK, transportMode);
		}

		public RefCusCodeOrAttributeTransportMode CreateTransportModeForCusCodeAttribute(ZGuid cusCodeAttributePK, ZString transportMode)
		{
			var result = factory.New<RefCusCodeOrAttributeTransportMode>();
			result.ZZU_ZZE_Attribute = cusCodeAttributePK;
			result.ZZU_TransportMode = transportMode;
			return result;
		}

		public RefCusCodeOrAttributeTransportMode CreateNewOrGetExistingTransportModeForCusCodeAttribute(ZGuid cusCodeAttributePK, ZString transportMode)
		{
			var query = new ZQuery()
				.AddToFilter(RefCusCodeOrAttributeTransportModeSchema.ZZU_ZZE_Attribute, cusCodeAttributePK)
				.AddToFilter(RefCusCodeOrAttributeTransportModeSchema.ZZU_TransportMode, transportMode);

			return factory.LoadTop1<RefCusCodeOrAttributeTransportMode>(query) ?? CreateTransportModeForCusCodeAttribute(cusCodeAttributePK, transportMode);
		}

		#endregion

		BusinessObject Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(ZString zzzDataGrouping, string description, BusinessObject parent) => CreateNewOrGetExistingDataGrouping(zzzDataGrouping, description, (RefDataGrouping)parent);

		public static void ClearParentDataGroupingCachingFor(BusinessObjectFactory factory, ZString zzzDataGrouping) => factory.ClearCachedValue<ZString>("ParentDataGrouping_" + zzzDataGrouping);

		public RefDataGrouping CreateNewOrGetExistingDataGrouping(ZString zzzDataGrouping, string description = null, RefDataGrouping parent = null)
		{
			var refDataGrouping = factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, zzzDataGrouping);

			if (refDataGrouping == null)
			{
				var newFactory = new BusinessObjectFactory();

				var legacyKey = GetPrimaryKeyFromLegacyDatabase(zzzDataGrouping);
				var newDataGrouping = legacyKey != Guid.Empty ? newFactory.NewWithPrimaryKey<RefDataGrouping>(legacyKey) : newFactory.New<RefDataGrouping>();

				newDataGrouping.ZZZ_DataGrouping = zzzDataGrouping;
				newDataGrouping.ZZZ_Description = description ?? $"Auto-created test data grouping {zzzDataGrouping}";

				newFactory.Save();
				refDataGrouping = factory.Load<RefDataGrouping>(newDataGrouping.PK);
			}

			if (parent != null)
			{
				refDataGrouping.ZZZ_ZZZ_Grouping = parent.PK;
				ClearParentDataGroupingCachingFor(factory, zzzDataGrouping);
			}

			return refDataGrouping;
		}

		/// <summary>
		/// Pick the existing key from the legacy ZZ database, will remove it when the ZZ database is offline.
		/// </summary>
		Guid GetPrimaryKeyFromLegacyDatabase(ZString dataGrouping)
		{
			var result = Guid.Empty;

			var legacyDatabaseName = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, @"ZZ");

			if (!string.IsNullOrWhiteSpace(legacyDatabaseName))
			{
				var sql = $@"SELECT {RefDataGroupingSchema.Constants.PK} FROM [{legacyDatabaseName}].[dbo].[{nameof(RefDataGrouping)}] WHERE {RefDataGroupingSchema.Constants.ZZZ_DataGrouping} = '{dataGrouping}'";

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var value = connection.ExecuteScalar(sql);

					if (value != DBNull.Value && value != null && Guid.TryParse(value.ToString(), out var key))
					{
						result = key;
					}
				}
			}

			return result;
		}

		public RefCusTariffAdditionalCodeCategory CreateNewOrGetExistingTariffAdditionalCodeCategory(ZString dataGrouping, ZString category, string description = null)
		{
			var result = RefCusTariffAdditionalCodeCategory.Loader.Load(factory, dataGrouping, category) ?? CreateCusTariffAdditionalCodeCategory(dataGrouping, category, description);
			return result;
		}

		public RefCusTariffAdditionalCodeCategory CreateCusTariffAdditionalCodeCategory(ZString dataGrouping, ZString category, string description = null)
		{
			return CreateCusTariffAdditionalCodeCategory(factory, dataGrouping, category, description);
		}

		public static RefCusTariffAdditionalCodeCategory CreateCusTariffAdditionalCodeCategory(BusinessObjectFactory factory, ZString dataGrouping, ZString category, string description = null)
		{
			var cusTariffAdditionalCodeCategory = factory.New<RefCusTariffAdditionalCodeCategory>();
			new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(dataGrouping);
			cusTariffAdditionalCodeCategory.ZY3_ZZZ_NKDataGrouping = dataGrouping;
			cusTariffAdditionalCodeCategory.ZY3_Category = category;
			cusTariffAdditionalCodeCategory.ZY3_Description = description ?? category + " DESC";
			return cusTariffAdditionalCodeCategory;
		}

		internal static Internal.RefCusTariffAdditionalCode CreateInternalRefCusTariffAdditionalCode(BusinessObjectFactory factory, bool isTariffNationalCode, ZGuid parentPK, ZString dataGrouping, ZString category, ZString code, string description = null, bool ensureDataGroupingExists = true, bool ensureCategoryExists = true)
		{
			var cusTariffAdditionalCode = factory.New<Internal.RefCusTariffAdditionalCode>();
			if (ensureDataGroupingExists)
			{
				new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(dataGrouping);
			}
			if (ensureCategoryExists)
			{
				new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingTariffAdditionalCodeCategory(dataGrouping, category);
			}
			if (isTariffNationalCode)
			{
				cusTariffAdditionalCode.ZY2_ZZW_NationalCode = parentPK;
			}
			else
			{
				cusTariffAdditionalCode.ZY2_ZZ1_Tariff = parentPK;
			}
			cusTariffAdditionalCode.ZY2_ZZZ_NKDataGrouping = dataGrouping;
			cusTariffAdditionalCode.ZY2_ZY3_NKCategory = category;
			cusTariffAdditionalCode.ZY2_AdditionalCode = code;
			cusTariffAdditionalCode.ZY2_Description = description ?? code + " DESC";
			return cusTariffAdditionalCode;
		}

		public TariffAdditionalCodeView CreateNewOrGetExistingTariffAdditionalCodeView(TariffView tariff, ZString category, ZString code, string description = null, bool ensureDataGroupingExists = true, bool ensureCategoryExists = true)
		{
			var query = new ZQuery(TariffAdditionalCodeViewSchema.ZY2_ZZZ_NKDataGrouping, tariff.ZZ1_ZZZ_NKDataGrouping);
			query.AddToFilter(TariffAdditionalCodeViewSchema.ZY2_AdditionalCode, code);
			query.AddToFilter(TariffAdditionalCodeViewSchema.ZY2_ZZ1_ParentTariffOrNationalCode, tariff.PK);
			query.AddToFilter(TariffAdditionalCodeViewSchema.ZY2_ZY3_NKCategory, category);
			return tariff.Factory.Load<TariffAdditionalCodeView>(query).OrderBy(x => x.PK).FirstOrDefault() ?? CreateTariffAdditionalCodeView(tariff, category, code, description, ensureDataGroupingExists: ensureDataGroupingExists, ensureCategoryExists: ensureCategoryExists);
		}

		public TariffAdditionalCodeView CreateTariffAdditionalCodeView(TariffView tariff, ZString category, ZString code, string description = null, string dataGrouping = null, bool ensureDataGroupingExists = true, bool ensureCategoryExists = true)
		{
			var newFactory = new BusinessObjectFactory();
			var cusTariffAdditionalCode = CreateInternalRefCusTariffAdditionalCode(newFactory, tariff.IsTariffNationalCode, tariff.PK, dataGrouping ?? tariff.ZZ1_ZZZ_NKDataGrouping, category, code, description, ensureDataGroupingExists: ensureDataGroupingExists, ensureCategoryExists: ensureCategoryExists);
			newFactory.Save();
			return factory.Load<TariffAdditionalCodeView>(cusTariffAdditionalCode.PK);
		}

		public RefCusMapType CreateCusMapType(ZString mapType, ZString direction, ZString description, bool isReadonly)
		{
			var query = new ZQuery(RefCusMapTypeSchema.ZZP_MapType, mapType);
			var cusMapType = factory.LoadTop1<RefCusMapType>(query);

			if (cusMapType == null)
			{
				cusMapType = factory.New<RefCusMapType>();
				cusMapType.ZZP_MapType = mapType;
				cusMapType.ZZP_Direction = direction;
				cusMapType.ZZP_Description = description;
				cusMapType.ZZP_IsReadonly = isReadonly;
			}
			return cusMapType;
		}

		public RefCusMap CreateCusMap(ZString mapType, ZString cW1orCommercialValue, ZString customsValue, ZDateTime startDate, ZDateTime endDate, ZString countryOrGrouping)
		{
			var query = new ZQuery(RefCusMapSchema.ZZM_ZZP_NKMapType, mapType);
			query.AddToFilter(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, countryOrGrouping);
			query.AddToFilter(RefCusMapSchema.ZZM_CW1orCommercialValue, cW1orCommercialValue);
			query.AddToFilter(RefCusMapSchema.ZZM_CustomsValue, customsValue);
			var cusMap = factory.LoadTop1<RefCusMap>(query);
			if (cusMap == null)
			{
				cusMap = factory.New<RefCusMap>();
				cusMap.ZZM_ZZP_NKMapType = mapType;
				cusMap.ZZM_CW1orCommercialValue = cW1orCommercialValue;
				cusMap.ZZM_CustomsValue = customsValue;
				cusMap.ZZM_StartDate = startDate;
				cusMap.ZZM_EndDate = endDate;
				CreateNewOrGetExistingDataGrouping(countryOrGrouping);
				cusMap.ZZM_ZZZ_NKDataGrouping = countryOrGrouping;
			}
			return cusMap;
		}

		BusinessObject Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper.CreateTaxOrFee(ZString code, ZDecimal rate, ZString dataGroupingCode, ZDateTime? startDate, ZDateTime? endDate, string description, bool ensureDataGroupingExists, decimal threshold) => CreateTaxOrFee(code, rate, dataGroupingCode, startDate, endDate, description, ensureDataGroupingExists, threshold);

		public RefCusTaxOrFee CreateTaxOrFee(ZString code, ZDecimal rate, ZString dataGroupingCode, ZDateTime? startDate = null, ZDateTime? endDate = null, string description = null, bool ensureDataGroupingExists = true, decimal threshold = 0m)
		{
			var effStartDate = startDate ?? ZDateTime.Today.AddDays(-1);
			var effEndDate = endDate ?? ZDateTime.Today.AddDays(1);

			var query = new ZQuery(RefCusTaxOrFeeSchema.ZZF_Code, code);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_StartDate, effStartDate);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_EndDate, effEndDate);

			var taxOrFee = factory.LoadTop1<RefCusTaxOrFee>(query);
			if (taxOrFee == null)
			{
				taxOrFee = factory.New<RefCusTaxOrFee>();
				taxOrFee.ZZF_Code = code;
				if (ensureDataGroupingExists)
				{
					CreateNewOrGetExistingDataGrouping(dataGroupingCode);
				}
				taxOrFee.ZZF_ZZZ_NKDataGrouping = dataGroupingCode;
				taxOrFee.ZZF_StartDate = effStartDate;
				taxOrFee.ZZF_EndDate = effEndDate;
			}
			taxOrFee.ZZF_Value = rate;
			taxOrFee.ZZF_Description = description ?? ZString.Format("{0} DESC", code);
			taxOrFee.ZZF_Threshold = threshold;
			return taxOrFee;
		}

		public RefCusTaxOrFeeLanguage CreateTaxOrFeeLanguage(RefCusTaxOrFee zzf, string languageCode, string localLanguageDescription)
		{
			var cusTaxOrFeeLanguage = factory.New<RefCusTaxOrFeeLanguage>();
			cusTaxOrFeeLanguage.ZXU_ZX6_NKLanguage = languageCode;
			cusTaxOrFeeLanguage.ZXU_ZZF_TaxOrFee = zzf.PK;
			cusTaxOrFeeLanguage.ZXU_Description = localLanguageDescription;
			return cusTaxOrFeeLanguage;
		}

		public RefCusTaxOrFee CreateTaxOrFee(ZString code, ZDecimal value, ZString dataGroupingCode, ZDecimal minimum, ZDecimal maximum, ZString feeType, ZDateTime? startDate = null, ZDateTime? endDate = null, string description = null)
		{
			var effStartDate = startDate ?? ZDateTime.Today.AddDays(-1);
			var effEndDate = endDate ?? ZDateTime.Today.AddDays(1);

			var query = new ZQuery(RefCusTaxOrFeeSchema.ZZF_Code, code);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_ZZZ_NKDataGrouping, dataGroupingCode);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_StartDate, effStartDate);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_EndDate, effEndDate);
			query.AddToFilter(RefCusTaxOrFeeSchema.ZZF_ZX0_NKTaxOrFeeType, feeType);
			var taxOrFee = factory.LoadTop1<RefCusTaxOrFee>(query);

			if (taxOrFee == null)
			{
				taxOrFee = factory.New<RefCusTaxOrFee>();
				taxOrFee.ZZF_Code = code;
				CreateNewOrGetExistingDataGrouping(dataGroupingCode);
				taxOrFee.ZZF_ZZZ_NKDataGrouping = dataGroupingCode;
				taxOrFee.ZZF_StartDate = effStartDate;
				taxOrFee.ZZF_EndDate = effEndDate;
				taxOrFee.ZZF_ZX0_NKTaxOrFeeType = feeType;
			}
			taxOrFee.ZZF_Value = value;
			taxOrFee.ZZF_Minimum = minimum;
			taxOrFee.ZZF_Maximum = maximum;
			taxOrFee.ZZF_Description = description ?? ZString.Format("{0} DESC", code);
			return taxOrFee;
		}

		public RefCusTaxOrFeeType CreateRefCusTaxOrFeeType(ZString type, string description = null)
		{
			var query = new ZQuery(RefCusTaxOrFeeTypeSchema.ZX0_TaxOrFeeType, type);
			var taxOrFeeType = factory.LoadTop1<RefCusTaxOrFeeType>(query);
			if (taxOrFeeType == null)
			{
				taxOrFeeType = factory.New<RefCusTaxOrFeeType>();
				taxOrFeeType.ZX0_TaxOrFeeType = type;
				taxOrFeeType.ZX0_Description = description ?? ZString.Format("{0} DESC", type);
			}
			return taxOrFeeType;
		}

		public CusRefPreferenceView CreatePreferenceView(ZString code, ZString description, ZString countryCode, bool isSystem = true)
		{
			var query = new ZQuery(CusRefPreferenceViewSchema.ZZS_Preference, code);
			query.AddToFilter(CusRefPreferenceViewSchema.ZZS_ZZZ_NKDataGrouping, countryCode);
			query.AddToFilter(CusRefPreferenceViewSchema.ZZS_IsSystem, isSystem);
			var result = factory.LoadTop1<CusRefPreferenceView>(query);
			if (result == null)
			{
				if (isSystem)
				{
					var newItem = CreatePreferenceForCountry(code, description, countryCode);
					newItem.Factory.Save();
					result = factory.Load<CusRefPreferenceView>(newItem.PK);
				}
				else
				{
					var view = factory.New<CusRefPreferenceView>();
					view.ZZS_Preference = code;
					view.ZZS_Description = description;
					view.ZZS_ZZZ_NKDataGrouping = countryCode;
					result = factory.Load<CusRefPreferenceView>(view.PK);
				}
			}
			return result;
		}

		public CusRefPreferenceView CreatePreferenceForCountry(ZString code, ZString description, ZString dataGroupingCode, bool isSystem = true)
		{
			var query = new ZQuery(CusRefPreferenceViewSchema.ZZS_Preference, code);
			query.AddToFilter(CusRefPreferenceViewSchema.ZZS_ZZZ_NKDataGrouping, dataGroupingCode);

			var refCusPreference = factory.LoadTop1<CusRefPreferenceView>(query);
			if (refCusPreference == null)
			{
				CreateNewOrGetExistingDataGrouping(dataGroupingCode);

				if (isSystem)
				{
					var newFactory = new BusinessObjectFactory();
					var newRef = newFactory.New<Internal.RefCusPreference>();
					newRef.ZZS_Preference = code;
					newRef.ZZS_Description = description;
					newRef.ZZS_ZZZ_NKDataGrouping = dataGroupingCode;
					newFactory.Save();
					refCusPreference = factory.Load<CusRefPreferenceView>(newRef.PK);
				}
				else
				{
					var newFactory = new BusinessObjectFactory();
					refCusPreference = newFactory.New<CusRefPreferenceView>();
					refCusPreference.ZZS_Preference = code;
					refCusPreference.ZZS_Description = description;
					refCusPreference.ZZS_ZZZ_NKDataGrouping = dataGroupingCode;
					newFactory.Save();
				}
			}

			return refCusPreference;
		}

		internal Internal.RefCusPreference CreatePreferenceForCountryAndGroupingInternal(ZString code, ZString description, ZString dataGroupingCode, ZString grouping)
		{
			var query = new ZQuery(RefCusPreferenceSchema.ZZS_Preference, code);
			query.AddToFilter(RefCusPreferenceSchema.ZZS_ZZZ_NKDataGrouping, grouping);

			var refCusPreference = factory.LoadTop1<Internal.RefCusPreference>(query);
			if (refCusPreference == null)
			{
				var eunId = CreateNewOrGetExistingDataGrouping(grouping);
				CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunId);

				refCusPreference = factory.New<Internal.RefCusPreference>();
				refCusPreference.ZZS_Preference = code;
				refCusPreference.ZZS_Description = description;
				refCusPreference.ZZS_ZZZ_NKDataGrouping = grouping;
			}

			return refCusPreference;
		}

		public CusRefPreferenceView CreatePreferenceForCountryAndGrouping(ZString code, ZString description, ZString dataGroupingCode, ZString grouping)
		{
			var refPre = CreatePreferenceForCountryAndGroupingInternal(code, description, dataGroupingCode, grouping);
			factory.Save();
			return factory.Load<CusRefPreferenceView>(refPre.PK);
		}

		public RefCusPreferenceLanguage CreatePreferenceLanguage(CusRefPreferenceView preferenceView, ZString language, ZString description)
		{
			var preferenceLanguage = factory.New<RefCusPreferenceLanguage>();
			preferenceLanguage.ZX9_ZZS_Preference = preferenceView.PK;
			preferenceLanguage.ZX9_ZX6_NKLanguage = language;
			preferenceLanguage.ZX9_Description = description;
			return preferenceLanguage;
		}

		public ZZRefCarrierCombined CreateZZRefCarrierCombined(ZString code, ZString description, ZString dataGroupingCode)
		{
			var carrier = factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = code;
			CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			carrier.ZZ4_CountryOrGrouping = dataGroupingCode;
			carrier.ZZ4_Description = description;
			return carrier;
		}

		public RefCarrierCode CreateCarrierCode(ZString code, ZString description, ZString dataGroupingCode)
		{
			var carrier = factory.New<RefCarrierCode>();
			carrier.ZZ4_Code = code;
			carrier.ZZ4_Description = description;
			CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			carrier.ZZ4_ZZZ_NKDataGrouping = dataGroupingCode;
			return carrier;
		}

		public RefCarrierCodeAttribute CreateCarrierCodeAttribute(ZGuid carrierCodePk, ZString attributeName, ZString attributeValue)
		{
			var attribute = factory.New<RefCarrierCodeAttribute>();
			attribute.ZZG_Name = attributeName;
			attribute.ZZG_Value = attributeValue;
			attribute.ZZG_ZZ4_CarrierCode = carrierCodePk;
			return attribute;
		}

		public RefCarrierCodeLanguage CreateCarrierCodeLanguage(RefCarrierCode zz4, string languageCode, string localLanguageDescription)
		{
			var refCarrierCodeLanguage = factory.New<RefCarrierCodeLanguage>();
			refCarrierCodeLanguage.ZCL_ZX6_NKLanguage = languageCode;
			refCarrierCodeLanguage.ZCL_ZZ4_CarrierCode = zz4.PK;
			refCarrierCodeLanguage.ZCL_Description = localLanguageDescription;

			return refCarrierCodeLanguage;
		}

		public RefVesselZZ CreateVesselZZ(ZString code, ZString radioCallSign, ZString type, ZString dataGroupingCode)
		{
			var vessel = factory.New<RefVesselZZ>();
			vessel.ZZO_Code = code;
			vessel.ZZO_VesselType = type;
			vessel.ZZO_RadioCallSign = radioCallSign;
			CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			vessel.ZZO_ZZZ_NKDataGrouping = dataGroupingCode;
			return vessel;
		}

		public RefCarrierVesselPivot CreateCarrierVesselPivot(ZGuid carrierCodePk, ZGuid vesselPk)
		{
			var pivot = factory.New<RefCarrierVesselPivot>();
			pivot.ZZQ_ZZ4 = carrierCodePk;
			pivot.ZZQ_ZZO = vesselPk;
			return pivot;
		}

		public CusRefTariffVersion CreateTariffVersion(ZString code, ZString description, ZDate effectiveDate)
		{
			var version = factory.New<CusRefTariffVersion>();
			version.CRT_Version = code;
			version.CRT_Description = description;
			version.CRT_EffectiveDate = effectiveDate;
			return version;
		}

		public RefCusCodeList CreateNewOrGetExistingCusCodeList(ZString officeCode, ZString dataGroupingCode, ZString description, ZString[] officePurpose)
		{
			var officeQuery = new ZQuery();
			officeQuery.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			officeQuery.AddToFilter(RefCusCodeListSchema.ZZD_Code, officeCode);
			officeQuery.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, dataGroupingCode);
			var cusCodeOffice = factory.LoadTop1<RefCusCodeList>(officeQuery);
			if (cusCodeOffice == null)
			{
				cusCodeOffice = factory.New<RefCusCodeList>();
				cusCodeOffice.ZZD_Code = officeCode;
				cusCodeOffice.ZZD_ZZZ_NKDataGrouping = dataGroupingCode;
				cusCodeOffice.ZZD_ZZK_NKCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
				cusCodeOffice.ZZD_Description = description;
				cusCodeOffice.ZZD_StartDate = ZDateTime.Today.AddDays(-2);
				cusCodeOffice.ZZD_EndDate = ZDateTime.Today.AddDays(2);

				foreach (var purpose in officePurpose)
				{
					var officeAttribute = cusCodeOffice.Attributes.AddNew();
					officeAttribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.ROLE;
					officeAttribute.ZZE_Value = purpose;
				}
			}
			CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			return cusCodeOffice;
		}

		public VATApplicabilityView CreateNewOrGetExistingVATApplicability(TariffView tariff, ZString dataGrouping, ZString taxOrFeeCode, string additionalCode = "", ZDateTime? startDate = null, ZDateTime? endDate = null, string description = null, ZGuid? tradeGroup = null, string category = "")
		{
			var effStartDate = startDate ?? ZDateTime.Today.AddDays(-1);
			var effEndDate = endDate ?? ZDateTime.Today.AddDays(1);

			var query = new ZQuery(VATApplicabilityViewSchema.ZX5_ZZ1_ParentTariffOrNationalCode, tariff.PK);
			query.AddToFilter(VATApplicabilityViewSchema.ZX5_ZZF_NKTaxOrFeeCode, taxOrFeeCode);
			query.AddToFilter(VATApplicabilityViewSchema.ZX5_AdditionalCode, additionalCode);
			query.AddToFilter(VATApplicabilityViewSchema.ZX5_ZZZ_NKDataGrouping, dataGrouping);
			query.AddToFilter(VATApplicabilityViewSchema.ZX5_StartDate, effStartDate);
			query.AddToFilter(VATApplicabilityViewSchema.ZX5_EndDate, effEndDate);
			query.AddToFilter(VATApplicabilityViewSchema.ZX5_VATCategory, category);

			return factory.LoadTop1<VATApplicabilityView>(query) ?? CreateVATApplicabilityView(tariff, dataGrouping, taxOrFeeCode, effStartDate, effEndDate, additionalCode, description, tradeGroup, category);
		}

		public VATApplicabilityView CreateVATApplicabilityView(TariffView tariff, ZString dataGrouping, ZString taxOrFeeCode, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string description = null, ZGuid? tradeGroup = null, string category = "")
		{
			var newFactory = new BusinessObjectFactory();
			var vatApplicability = CreateInternalRefCusVATApplicability(newFactory, tariff, dataGrouping, taxOrFeeCode, startDate, endDate, additionalCode, description, tradeGroup, category);
			newFactory.Save();
			return factory.Load<VATApplicabilityView>(vatApplicability.PK);
		}

		internal static Internal.RefCusVATApplicability CreateInternalRefCusVATApplicability(BusinessObjectFactory factory, TariffView tariff, ZString dataGrouping, ZString taxOrFeeCode, ZDateTime startDate, ZDateTime endDate, string additionalCode = "", string description = null, ZGuid? tradeGroup = null, string category = "")
		{
			var vatApplicability = factory.New<Internal.RefCusVATApplicability>();
			if (tariff.IsTariffNationalCode)
			{
				vatApplicability.ZX5_ZZW_TariffNationalCode = tariff.PK;
			}
			else
			{
				vatApplicability.ZX5_ZZ1_Tariff = tariff.PK;
			}
			vatApplicability.ZX5_ZZZ_NKDataGrouping = dataGrouping;
			vatApplicability.ZX5_ZZF_NKTaxOrFeeCode = taxOrFeeCode;
			vatApplicability.ZX5_AdditionalCode = additionalCode;
			vatApplicability.ZX5_StartDate = startDate;
			vatApplicability.ZX5_EndDate = endDate;
			vatApplicability.ZX5_Description = description ?? taxOrFeeCode + " DESC";
			vatApplicability.ZX5_ZZA_TradeGroup = tradeGroup.GetValueOrDefault();
			vatApplicability.ZX5_VATCategory = category;
			return vatApplicability;
		}

		public ZZRefCusRulingCombined CreateZZRefCusRulingCombined(ZString country, ZString number, ZString type, ZGuid? applies = null, ZDate? startDate = null, ZDate? endDate = null)
		{
			var effStartDate = startDate ?? ZDate.Today.AddDays(-1);
			var effEndDate = endDate ?? ZDate.Today.AddDays(1);

			var cusRuling = factory.New<ZZRefCusRulingCombined>();
			cusRuling.ZZX_RN_NKCountryCode = country;
			cusRuling.ZZX_RulingNumber = number;
			cusRuling.ZZX_Description = number + " DESC";
			cusRuling.ZZX_RulingType = type;
			cusRuling.ZZX_OA_AppliesTo = applies.GetValueOrDefault();
			cusRuling.ZZX_StartDate = effStartDate;
			cusRuling.ZZX_EndDate = effEndDate;
			return cusRuling;
		}

		public ZZRefCusRulingCombined CreateOrGetRefCusRuling(ZString country, ZString number, ZString type, ZDate startDate, ZDate endDate)
		{
			var sql = @"
						 declare @PK uniqueIdentifier 
						 select @PK = ZZX_PK from RefDatabase_RefCusRuling where ZZX_RN_NKCountryCode ='{0}' AND ZZX_RulingNumber='{1}' AND ZZX_RulingType='{2}' AND ZZX_StartDate='{3}'
						 if @PK is null
						 begin 
							set @PK = newid()
							insert into RefDatabase_RefCusRuling (ZZX_PK, ZZX_RN_NKCountryCode, ZZX_RulingNumber, ZZX_Description, ZZX_RulingType, ZZX_StartDate, ZZX_EndDate) 
								select @PK, '{0}', '{1}', '{1} Desc', '{2}', '{3}', '{4}'
						 end
						select @PK
						";
			var cusRulingPKString = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, sql, country, number, type, startDate, endDate)).ExecuteScalar().ToString();

			return factory.Load<ZZRefCusRulingCombined>(new Guid(cusRulingPKString));
		}

		public CusRulingConfigCombined CreateRefCusRulingConfig(ZString category, ZString type, ZDecimal rate, ZString value, ZString cusRulingPK)
		{
			var sql = @"
						 declare @PK uniqueIdentifier = newid()
						 insert into RefDatabase_RefCusRulingConfig (ZZY_PK, ZZY_Category, ZZY_Type, ZZY_Rate, ZZY_Value, ZZY_ZZX_CusRuling) 
						 select @PK, '{0}', '{1}', {2}, '{3}', '{4}'
						 select @PK
						 ";
			var cusRulingConfigPKString = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, sql, category, type, rate, value, cusRulingPK)).ExecuteScalar().ToString();

			return factory.Load<CusRulingConfigCombined>(new Guid(cusRulingConfigPKString));
		}

		public RefHarbourRate CreateHarbourRate(ZString type, ZString port, ZString mode, ZString commodity, ZDate startDate, ZDate endDate, ZString rateFormula, ZString dataGrouping, string portTaxType = null)
		{
			var query = new ZQuery(RefHarbourRateSchema.ZXF_Type, type);
			query.AddToFilter(RefHarbourRateSchema.ZXF_Port, port);
			query.AddToFilter(RefHarbourRateSchema.ZXF_Mode, mode);
			query.AddToFilter(RefHarbourRateSchema.ZXF_Commodity, commodity);
			query.AddToFilter(RefHarbourRateSchema.ZXF_ZZZ_NKDataGrouping, dataGrouping);
			var harbourRate = factory.LoadTop1<RefHarbourRate>(query);
			if (harbourRate == null)
			{
				harbourRate = factory.New<RefHarbourRate>();
				harbourRate.ZXF_Type = type;
				harbourRate.ZXF_Port = port;
				harbourRate.ZXF_Mode = mode;
				harbourRate.ZXF_Commodity = commodity;
				harbourRate.ZXF_StartDate = startDate;
				harbourRate.ZXF_EndDate = endDate;
				harbourRate.ZXF_RateFormula = rateFormula;
				CreateNewOrGetExistingDataGrouping(dataGrouping);
				harbourRate.ZXF_ZZZ_NKDataGrouping = dataGrouping;
				harbourRate.ZXF_PortTaxType = portTaxType;
			}
			return harbourRate;
		}

		public SpecificVATSelectionCriteria CreateVATSelectionCriteria(ZString dataGrouping, ZDateTime effectiveDate, ZString taxOrFee, ISet<ZString> additionalCode, ISet<ZString> tradeGroups = null)
		{
			return new SpecificVATSelectionCriteria(dataGrouping, effectiveDate, taxOrFee, additionalCode, tradeGroups);
		}

		public SpecificRateSelectionCriteria CreateRateSelectionCriteria(ZString countryOfOrigin, ZString dataGrouping, ZString primaryPreference, ZString concessionOrder, ISet<ZString> additionalCode, ZDateTime effectiveDate, ZString rateType, ZString rateCode, ISet<ZString> secondTradeGroup = null, RateDirection direction = RateDirection.Both)
		{
			return new SpecificRateSelectionCriteria(countryOfOrigin, dataGrouping, primaryPreference, concessionOrder, additionalCode, effectiveDate, rateType, rateCode, secondTradeGroup, direction);
		}

		public TariffAdditionalCodeSelectionCriteria CreateTariffAdditionalCodeSelectionCriteria(ZString category, ZDateTime effectiveDate, ZString tradeGroupCountry, ZString dataGrouping)
		{
			return new TariffAdditionalCodeSelectionCriteria(category, effectiveDate, tradeGroupCountry, dataGrouping);
		}

		public RefCusTariffBRCharacteristic CreateRefCusTariffBRCharacteristic(RefCusNomenclatureGroup nomeclatureGroup, ZString characteristicType, ZString code, string type = null, ZDateTime? startDate = null, ZDateTime? endDate = null, bool isImport = true, bool isExport = true,
			List<KeyValuePair<string, string>> attributes = null, List<KeyValuePair<string, string>> values = null)
		{
			return CreateRefCusTariffBRCharacteristic(nomeclatureGroup.PK, true, characteristicType, code, type, startDate, endDate, isImport, isExport, attributes, values);
		}

		public RefCusTariffBRCharacteristic CreateRefCusTariffBRCharacteristic(TariffView tariff, ZString characteristicType, ZString code, string type = null, ZDateTime? startDate = null, ZDateTime? endDate = null, bool isImport = true, bool isExport = true,
			List<KeyValuePair<string, string>> attributes = null, List<KeyValuePair<string, string>> values = null)
		{
			return CreateRefCusTariffBRCharacteristic(tariff.PK, false, characteristicType, code, type, startDate, endDate, isImport, isExport, attributes, values);
		}

		RefCusTariffBRCharacteristic CreateRefCusTariffBRCharacteristic(ZGuid pk, ZBool byNomenchatureGroup, ZString characteristicType, ZString code, string type = null, ZDateTime? startDate = null, ZDateTime? endDate = null, bool isImport = true, bool isExport = true,
			List<KeyValuePair<string, string>> attributes = null, List<KeyValuePair<string, string>> values = null)
		{
			var characteristic = factory.New<RefCusTariffBRCharacteristic>();
			characteristic.ZB1_ZZ1_Tariff = (!byNomenchatureGroup) ? pk : ZGuid.Empty;
			characteristic.ZB1_ZZ5_Nomenclature = (byNomenchatureGroup) ? pk : ZGuid.Empty;
			characteristic.ZB1_CharacteristicType = characteristicType;
			characteristic.ZB1_Code = code;
			characteristic.ZB1_Text = code;
			characteristic.ZB1_Style = type ?? Constants.ProfileQuestion.AnswerDataTypes.String;
			characteristic.ZB1_StartDate = startDate ?? ZDateTime.Today.AddDays(-10);
			characteristic.ZB1_EndDate = endDate ?? ZDateTime.Today.AddDays(10);
			characteristic.ZB1_IsImport = isImport;
			characteristic.ZB1_IsExport = isExport;

			if (attributes != null)
			{
				foreach (var att in attributes)
				{
					var newAtt = characteristic.Attributes.AddNew();
					newAtt.ZB3_Name = att.Key;
					newAtt.ZB3_Code = att.Key;
					newAtt.ZB3_Value = att.Value;
				}
			}

			if (values != null)
			{
				foreach (var value in values)
				{
					var newValue = characteristic.Values.AddNew();
					newValue.ZB2_Value = value.Key;
					newValue.ZB2_Description = value.Value;
				}
			}

			return characteristic;
		}

		public RefCusProfileType CreateRefCusProfileType(ZString profileType, ZString tariffType, ZString dataGrouping, string description = null)
		{
			CreateNewOrGetExistingDataGrouping(dataGrouping);
			return CreateRefCusProfileType(CreateNewOrGetExistingTariffType(dataGrouping, tariffType).PK, dataGrouping, profileType, description);
		}

		public RefCusProfileType CreateRefCusProfileType(ZGuid tariffTypePK, ZString dataGrouping, ZString profileType, string description = null)
		{
			var query = new ZQuery(RefCusProfileTypeSchema.XXX_ProfileType, profileType);
			query.AddToFilter(RefCusProfileTypeSchema.XXX_ZZZ_NKDataGrouping, dataGrouping);
			query.AddToFilter(RefCusProfileTypeSchema.XXX_ZZI_TariffType, tariffTypePK);
			var cusProfileType = factory.LoadTop1<RefCusProfileType>(query);
			if (cusProfileType == null)
			{
				cusProfileType = factory.New<RefCusProfileType>();
				cusProfileType.XXX_ZZZ_NKDataGrouping = dataGrouping;
				cusProfileType.XXX_ZZI_TariffType = tariffTypePK;
				cusProfileType.XXX_ProfileType = profileType;
				cusProfileType.XXX_Description = description ?? profileType;
			}
			return cusProfileType;
		}

		public RefCusProfile CreateRefCusProfile(RefCusProfileType profileType, string tariffCode, string questionCode, ZDateTime startDate, ZDateTime endDate,
			IEnumerable<(string, string)> attributes = null)
		{
			var profile = factory.New<RefCusProfile>();
			profile.XX0_XXX_ProfileType = profileType.PK;
			profile.XX0_ZZZ_NKDataGrouping = profileType.XXX_ZZZ_NKDataGrouping;
			profile.XX0_TariffCode = tariffCode;
			profile.XX0_QuestionCode = questionCode;
			profile.XX0_StartDate = startDate;
			profile.XX0_EndDate = endDate;

			if (attributes != null)
			{
				foreach (var (name, value) in attributes)
				{
					var attribute = profile.Attributes.AddNew();
					attribute.XXY_Name = name;
					attribute.XXY_Value = value;
				}
			}

			return profile;
		}

		public RefCusProfileQuestion CreateRefCusProfileQuestion(RefCusProfileType profileType, string questionName, string questionCode, string text, ZDateTime startDate, ZDateTime endDate,
			string answerDataType = "BOOLEAN", short answerDecimalPlaces = 0, short answerMaxLength = 0, string answerMask = null,
			bool allowMultipleAnswers = false, string note = null, bool isMandatory = false,
			IEnumerable<(string, string)> attributes = null,
			IEnumerable<(string, string)> answers = null)
		{
			var question = factory.New<RefCusProfileQuestion>();
			question.XQ2_XXX_ProfileType = profileType.PK;
			question.XQ2_ZZZ_NKDataGrouping = profileType.XXX_ZZZ_NKDataGrouping;
			question.XQ2_Name = questionName;
			question.XQ2_Code = questionCode;
			question.XQ2_Text = text;
			question.XQ2_StartDate = startDate;
			question.XQ2_EndDate = endDate;
			question.XQ2_AnswerDataType = answerDataType;
			question.XQ2_AnswerDecimalPlaces = answerDecimalPlaces;
			question.XQ2_AnswerMaxLength = answerMaxLength;
			question.XQ2_AnswerMask = answerMask ?? string.Empty;
			question.XQ2_AllowMultipleAnswers = allowMultipleAnswers;
			question.XQ2_Note = note ?? string.Empty;
			question.XQ2_IsAnswerMandatory = isMandatory;

			if (attributes != null)
			{
				foreach (var (name, value) in attributes)
				{
					var attribute = question.Attributes.AddNew();
					attribute.XQ3_Name = name;
					attribute.XQ3_Value = value;
				}
			}

			if (answers != null)
			{
				foreach (var (value, description) in answers)
				{
					var answer = question.Answers.AddNew();
					answer.XQ4_Value = value;
					answer.XQ4_Description = description;
				}
			}

			return question;
		}

		public RefCusProfileQuestionPathway CreateRefCusProfileQuestionPathway(RefCusProfileQuestion questionParent, RefCusProfileQuestion questionChild, string description, ZDateTime startDate, ZDateTime endDate, string condition)
		{
			var questionPathway = factory.New<RefCusProfileQuestionPathway>();
			questionPathway.XQP_XQ2_QuestionParent = questionParent.PK;
			questionPathway.XQP_XQ2_QuestionChild = questionChild.PK;
			questionPathway.XQP_Description = description;
			questionPathway.XQP_StartDate = startDate;
			questionPathway.XQP_EndDate = endDate;
			questionPathway.XQP_ConditionToProceedFormula = condition;
			return questionPathway;
		}

		public void AssertRateSelectionCriteriaInfoResult(RateSelectionCriteriaInfo result, ZString tradeGroupCountry, ZDateTime effectiveDate, ZString rateType, ZString rateCode, ZString orderNumber, ZString additionalCode, ZString preferenceCode, ZString preferenceDescription, ZString tradeGroup, ZString tradeGroupDescription, string secondTradeGroup = "")
		{
			Assertion.AssertEquals("TradeGroupCountry", tradeGroupCountry, result.TradeGroupCountry);
			Assertion.AssertEquals("EffectiveDate", effectiveDate, result.EffectiveDate);
			Assertion.AssertEquals("RateType", rateType, result.RateType);
			Assertion.AssertEquals("RateCode", rateCode, result.RateCode);
			Assertion.AssertEquals("OrderNumber", orderNumber, result.ZZT_OrderNumber);
			Assertion.AssertEquals("AdditionalCode", additionalCode, result.ZZT_AdditionalCode);
			Assertion.AssertEquals("PreferenceCode", preferenceCode, result.ZZS_Preference);
			Assertion.AssertEquals("PreferenceDescription", preferenceDescription, result.ZZS_Description);
			Assertion.AssertEquals("TradeGroup", tradeGroup, result.ZZA_TradeGroup);
			Assertion.AssertEquals("TradeGroupDescription", tradeGroupDescription, result.ZZA_Description);
			Assertion.AssertEquals("SecondTradeGroup", (ZString)secondTradeGroup, result.SecondTradeGroup);
		}

		public void AssertTwoRateSelectionCriteriaInfoSame(RateSelectionCriteriaInfo info1, RateSelectionCriteriaInfo info2)
		{
			Assertion.AssertEquals("TradeGroupCountry", info1.TradeGroupCountry, info2.TradeGroupCountry);
			Assertion.AssertEquals("EffectiveDate", info1.EffectiveDate, info2.EffectiveDate);
			Assertion.AssertEquals("RateType", info1.RateType, info2.RateType);
			Assertion.AssertEquals("RateCode", info1.RateCode, info2.RateCode);
			Assertion.AssertEquals("OrderNumber", info1.ZZT_OrderNumber, info2.ZZT_OrderNumber);
			Assertion.AssertEquals("AdditionalCode", info1.ZZT_AdditionalCode, info2.ZZT_AdditionalCode);
			Assertion.AssertEquals("PreferenceCode", info1.ZZS_Preference, info2.ZZS_Preference);
			Assertion.AssertEquals("PreferenceDescription", info1.ZZS_Description, info2.ZZS_Description);
			Assertion.AssertEquals("TradeGroup", info1.ZZA_TradeGroup, info2.ZZA_TradeGroup);
			Assertion.AssertEquals("TradeGroupDescription", info1.ZZA_Description, info2.ZZA_Description);
			Assertion.AssertEquals("SecondTradeGroup", info1.SecondTradeGroup, info2.SecondTradeGroup);
		}

		public void AssertConditionApplicabilitiesByCriteriaResult(ConditionApplicabilitiesByCriteria result, ZString tradeGroupCountry, ZDateTime effectiveDate, ZString conditionClass, ZString conditionType,
			ZString orderNumber, ZString additionalCode, ZString preferenceCode, ZString preferenceDescription, ZString tradeGroup, ZString tradeGroupDescription, string secondTradeGroup = "")
		{
			Assertion.AssertEquals("TradeGroupCountry", tradeGroupCountry, result.TradeGroupCountry);
			Assertion.AssertEquals("EffectiveDate", effectiveDate, result.EffectiveDate);
			Assertion.AssertEquals("ConditionClass", conditionClass, result.ZX2_ConditionClass);
			Assertion.AssertEquals("ConditionType", conditionType, result.ZX2_ConditionType);
			Assertion.AssertEquals("OrderNumber", orderNumber, result.ZZT_OrderNumber);
			Assertion.AssertEquals("AdditionalCode", additionalCode, result.ZZT_AdditionalCode);
			Assertion.AssertEquals("PreferenceCode", preferenceCode, result.ZZS_Preference);
			Assertion.AssertEquals("PreferenceDescription", preferenceDescription, result.ZZS_Description);
			Assertion.AssertEquals("TradeGroup", tradeGroup, result.ZZA_TradeGroup);
			Assertion.AssertEquals("TradeGroupDescription", tradeGroupDescription, result.ZZA_Description);
			Assertion.AssertEquals("SecondTradeGroup", secondTradeGroup, result.SecondTradeGroup);
		}

		BusinessObject Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper.CreatePreferenceForCountry(ZString code, ZString description, ZString dataGroupingCode, bool isSystem) => CreatePreferenceForCountry(code, description, dataGroupingCode, isSystem);

		protected readonly BusinessObjectFactory factory;

		void DeleteTable(string tableName)
		{
			if (string.IsNullOrWhiteSpace(tableName))
			{
				return;
			}

			var sql = $"DELETE [dbo].[{tableName}]";
			((IDbConnected)factory).Connection.ExecuteNonQuery(sql);
		}
	}
}
