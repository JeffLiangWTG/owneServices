using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public static class Helper
	{
		public static XmlWriterConfiguration GetRefCusCodeTypeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeTypeConfiguration = new EntityTypeConfiguration<RefCusCodeType>(true);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_CodeType, true);
			codeTypeConfiguration.IncludeColumn(x => x.ZZK_Description, false);
			codeTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZZK_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			codeTypeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZK_MaxLength, false, 0);
			writerConfiguration.IncludeEntityTypeConfiguration(codeTypeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListAttributeNameWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var attributeNameConfiguration = new EntityTypeConfiguration<RefCusCodeListAttributeName>(true);
			attributeNameConfiguration.IncludeColumn(x => x.ZXE_Name, true);
			attributeNameConfiguration.IncludeColumn(x => x.ZXE_Description, false);
			attributeNameConfiguration.IncludeColumn(x => x.ZXE_ZZK_NKCodeType, true);
			attributeNameConfiguration.IncludeColumn(x => x.ZXE_AllowDuplicates, false);
			attributeNameConfiguration.IncludeColumn(x => x.ZXE_ColumnCaption, false);
			attributeNameConfiguration.IncludeColumnWithConstantValue(x => x.ZXE_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			writerConfiguration.IncludeEntityTypeConfiguration(attributeNameConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType, bool hasAttributes = false, bool attributeAllowDuplicates = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, new DateTime(1900, 01, 01, 0, 0, 0));
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 0));
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			if (hasAttributes)
			{
				refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);

				var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, attributeAllowDuplicates);
				writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);
			}

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusNomenclatureGroupWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusNomenclatureGroup = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			refCusNomenclatureGroup.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			refCusNomenclatureGroup.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, Constants.DataGroupingCodes.Mercosul);
			refCusNomenclatureGroup.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusNomenclatureGroup.IncludeColumn(x => x.ZZ5_Description, false);
			refCusNomenclatureGroup.IncludeColumnWithDefaultValue(x => x.ZZ5_StartDate, false, new DateTime(2022, 04, 01, 00, 00, 00));
			refCusNomenclatureGroup.IncludeColumnWithDefaultValue(x => x.ZZ5_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			refCusNomenclatureGroup.IncludeColumn(x => x.ZZ5_Value, false);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusNomenclatureGroup);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusTariffTypeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusTariffType = new EntityTypeConfiguration<RefCusTariffType>(true);
			refCusTariffType.IncludeColumn(x => x.ZZI_TariffType, true);
			refCusTariffType.IncludeColumn(x => x.ZZI_Description, false);
			refCusTariffType.IncludeColumnWithConstantValue(x => x.ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffType);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusTariffWriterConfiguration(string tariffType, bool addAttributes = false, bool addDefaultEndDate = false, bool hasCompositeKey = true)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusTariff = new EntityTypeConfiguration<RefCusTariff>(true);
			refCusTariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, tariffType);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusTariff.IncludeColumn(x => x.ZZ1_Description, false);
			if (addDefaultEndDate)
			{
				refCusTariff.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			}
			else
			{
				refCusTariff.IncludeColumn(x => x.ZZ1_EndDate, false);
			}
			refCusTariff.IncludeColumnWithDefaultValue(x => x.ZZ1_StartDate, false, new DateTime(2022, 04, 01, 00, 00, 00));
			refCusTariff.IncludeColumn(x => x.ZZ1_ZZF_NKTaxOrFeeCode, false);
			if (hasCompositeKey)
			{
				refCusTariff.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, false);
			}

			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariff);

			if (addAttributes)
			{
				refCusTariff.IncludeColumn(x => x.RefCusTariffAttributes, false);

				var refCusTariffAttribute = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
				refCusTariffAttribute.IncludeColumn(x => x.ZZ3_Name, true);
				refCusTariffAttribute.IncludeColumn(x => x.ZZ3_Value, false);

				writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffAttribute);
			}

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusTariffUnitOfMeasureWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusTariff = new EntityTypeConfiguration<RefCusTariff>(false);
			refCusTariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.Codes.HSN);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusTariff.IncludeColumn(x => x.RefCusTariffUOMs, false);

			var refCusTariffUOMConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			refCusTariffUOMConfiguration.IncludeColumn(x => x.ZZ8_Type, true);
			refCusTariffUOMConfiguration.IncludeColumn(x => x.ZZ8_UOM, false);
			refCusTariffUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, false, Constants.DataGroupingCodes.Brazil);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariff);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffUOMConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusProfileTypeWriterConfiguration(string tariffType)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusProfileType = new EntityTypeConfiguration<RefCusProfileType>(true);
			refCusProfileType.IncludeColumn(x => x.XXX_ProfileType, true);
			refCusProfileType.IncludeColumnWithConstantValue(x => x.XXX_ZZI_NKTariffType, true, tariffType);
			refCusProfileType.IncludeColumnWithConstantValue(x => x.XXX_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileType.IncludeColumnWithConstantValue(x => x.XXX_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileType.IncludeColumn(x => x.XXX_Description, false);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusProfileType);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusProfileWriterConfiguration(string profileType, string tariffType, bool hasAttributes = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusProfile = new EntityTypeConfiguration<RefCusProfile>(true);
			refCusProfile.IncludeColumn(x => x.XX0_TariffCode, true);
			refCusProfile.IncludeColumnWithConstantValue(x => x.XX0_XXX_NKProfileType, true, profileType);
			refCusProfile.IncludeColumnWithConstantValue(x => x.XX0_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfile.IncludeColumnWithConstantValue(x => x.XX0_XXX_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfile.IncludeColumnWithConstantValue(x => x.XX0_XXX_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfile.IncludeColumnWithConstantValue(x => x.XX0_XXX_ZZI_NKTariffType, true, tariffType);
			refCusProfile.IncludeColumn(x => x.XX0_QuestionCode, true);
			refCusProfile.IncludeColumnWithDefaultValue(x => x.XX0_StartDate, false, new DateTime(1900, 01, 01, 00, 00, 00));
			refCusProfile.IncludeColumnWithDefaultValue(x => x.XX0_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			refCusProfile.IncludeColumnWithDefaultValue(x => x.XX0_AllowMultipleAnswers, false, false);
			refCusProfile.IncludeColumnWithDefaultValue(x => x.XX0_IsAnswerMandatory, false, false);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusProfile);
			if (hasAttributes)
			{
				refCusProfile.IncludeColumn(x => x.RefCusProfileAttributes, false);
				var refCusProfileAttribute = new EntityTypeConfiguration<RefCusProfileAttribute>(true);
				refCusProfileAttribute.IncludeColumn(x => x.XXY_Name, true);
				refCusProfileAttribute.IncludeColumn(x => x.XXY_Value, false);

				writerConfiguration.IncludeEntityTypeConfiguration(refCusProfileAttribute);
			}

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusProfileQuestionWriterConfiguration(string profileType, string tariffType, bool addAttributes = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusProfileQuestion = new EntityTypeConfiguration<RefCusProfileQuestion>(true);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_QuestionCode, true);
			refCusProfileQuestion.IncludeColumnWithConstantValue(x => x.XQ2_XXX_NKProfileType, true, profileType);
			refCusProfileQuestion.IncludeColumnWithConstantValue(x => x.XQ2_XXX_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileQuestion.IncludeColumnWithConstantValue(x => x.XQ2_XXX_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileQuestion.IncludeColumnWithConstantValue(x => x.XQ2_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileQuestion.IncludeColumnWithConstantValue(x => x.XQ2_XXX_ZZI_NKTariffType, true, tariffType);
			refCusProfileQuestion.IncludeColumnWithDefaultValue(x => x.XQ2_StartDate, false, new DateTime(1900, 01, 01, 00, 00, 00));
			refCusProfileQuestion.IncludeColumnWithDefaultValue(x => x.XQ2_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_AnswerDataType, false);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_AnswerDecimalPlaces, false);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_AnswerMask, false);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_AnswerMaxLength, false);
			refCusProfileQuestion.IncludeColumnWithDefaultValue(x => x.XQ2_AllowMultipleAnswers, false, Constants.Booleans.FALSE);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_Name, false);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_Text, false);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_Note, false);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_IsAnswerMandatory, false);
			refCusProfileQuestion.IncludeColumn(x => x.XQ2_Name, false);
			refCusProfileQuestion.IncludeColumn(x => x.RefCusProfileQuestionAnswerLists, false);

			var refCusProfileQuestionAnswerList = new EntityTypeConfiguration<RefCusProfileQuestionAnswerList>(true);
			refCusProfileQuestionAnswerList.IncludeColumn(x => x.XQ4_Value, true);
			refCusProfileQuestionAnswerList.IncludeColumn(x => x.XQ4_Description, false);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusProfileQuestion);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusProfileQuestionAnswerList);

			if (addAttributes)
			{
				refCusProfileQuestion.IncludeColumn(x => x.RefCusProfileQuestionAttributes, false);

				var refCusProfileQuestionAttribute = new EntityTypeConfiguration<RefCusProfileQuestionAttribute>(true);
				refCusProfileQuestionAttribute.IncludeColumn(x => x.XQ3_Name, true);
				refCusProfileQuestionAttribute.IncludeColumn(x => x.XQ3_Value, true);

				writerConfiguration.IncludeEntityTypeConfiguration(refCusProfileQuestionAttribute);
			}

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusProfileQuestionPathwayWriterConfiguration(string profileType, string tariffType)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusProfileQuestionPathway = new EntityTypeConfiguration<RefCusProfileQuestionPathway>(true);
			refCusProfileQuestionPathway.IncludeColumn(x => x.XQP_XQ2_NKQuestionChild, true);
			refCusProfileQuestionPathway.IncludeColumn(x => x.XQP_XQ2_NKQuestionParent, true);
			refCusProfileQuestionPathway.IncludeColumnWithConstantValue(x => x.XQP_XQ2_XXX_NKProfileType, true, profileType);
			refCusProfileQuestionPathway.IncludeColumnWithConstantValue(x => x.XQP_XQ2_XXX_ZZI_NKTariffType, true, tariffType);
			refCusProfileQuestionPathway.IncludeColumnWithConstantValue(x => x.XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileQuestionPathway.IncludeColumnWithConstantValue(x => x.XQP_XQ2_XXX_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileQuestionPathway.IncludeColumnWithConstantValue(x => x.XQP_XQ2_ZZZ_NKDataGroupingChild, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileQuestionPathway.IncludeColumnWithConstantValue(x => x.XQP_XQ2_ZZZ_NKDataGroupingParent, true, Constants.DataGroupingCodes.Brazil);
			refCusProfileQuestionPathway.IncludeColumn(x => x.XQP_XQ2_NKQuestionStartDateChild, true);
			refCusProfileQuestionPathway.IncludeColumn(x => x.XQP_XQ2_NKQuestionStartDateParent, true);
			refCusProfileQuestionPathway.IncludeColumnWithDefaultValue(x => x.XQP_StartDate, false, new DateTime(1900, 01, 01, 00, 00, 00));
			refCusProfileQuestionPathway.IncludeColumn(x => x.XQP_ConditionToProceedFormula, false);
			refCusProfileQuestionPathway.IncludeColumn(x => x.XQP_Description, false);
			refCusProfileQuestionPathway.IncludeColumnWithDefaultValue(x => x.XQP_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			refCusProfileQuestionPathway.IncludeColumnWithDefaultValue(x => x.XQP_StartDate, false, new DateTime(1900, 01, 01, 00, 00, 00));
			refCusProfileQuestionPathway.IncludeColumnWithDefaultValue(x => x.XQP_AllowMultipleAnswers, false, false);
			refCusProfileQuestionPathway.IncludeColumnWithDefaultValue(x => x.XQP_IsAnswerMandatory, false, false);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusProfileQuestionPathway);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetTariffBRCharacteristicConfiguration(string characteristicType, bool attributes = false, string constantStyle = null,
			bool defaultBooleanValues = false, bool defaultIsImport = false, bool defaultIsExport = false, bool defaultIsMandatory = false, bool defaultIsConditioningAttribute = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusTariff = new EntityTypeConfiguration<RefCusTariff>(false);
			refCusTariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.Codes.HSN);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusTariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusTariff.IncludeColumn(x => x.RefCusTariffBRCharacteristics, false);

			var refCusTariffBRCharacteristic = new EntityTypeConfiguration<RefCusTariffBRCharacteristic>(true);
			refCusTariffBRCharacteristic.IncludeColumnWithConstantValue(x => x.ZB1_CharacteristicType, true, characteristicType);
			if (string.IsNullOrEmpty(constantStyle))
			{
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_Style, false);
			}
			else
			{
				refCusTariffBRCharacteristic.IncludeColumnWithConstantValue(x => x.ZB1_Style, false, constantStyle);
			}
			refCusTariffBRCharacteristic.IncludeColumnWithConstantValue(x => x.ZB1_MaxLength, false, "0");
			refCusTariffBRCharacteristic.IncludeColumnWithConstantValue(x => x.ZB1_DecimalPlaces, false, "0");
			refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_Code, true);
			refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_Text, false);
			refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_StartDate, false, new DateTime(1900, 01, 01, 00, 00, 00));
			refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			if (defaultBooleanValues)
			{
				refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_IsImport, false, defaultIsImport);
				refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_IsExport, false, defaultIsExport);
				refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_IsMandatory, false, defaultIsMandatory);
				refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_IsConditioningAttribute, false, defaultIsConditioningAttribute);
			}
			else
			{
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_IsImport, false);
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_IsExport, false);
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_IsMandatory, false);
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_IsConditioningAttribute, false);
			}
			refCusTariffBRCharacteristic.IncludeColumn(x => x.RefCusTariffBRCharacteristicValues, false);

			if (attributes)
			{
				refCusTariffBRCharacteristic.IncludeColumn(x => x.RefCusTariffBRCharacteristicAttributes, false);
			}

			var refCusTariffBRCharacteristicValue = new EntityTypeConfiguration<RefCusTariffBRCharacteristicValue>(true);
			refCusTariffBRCharacteristicValue.IncludeColumn(x => x.ZB2_Value, true);
			refCusTariffBRCharacteristicValue.IncludeColumn(x => x.ZB2_Description, false);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariff);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffBRCharacteristic);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffBRCharacteristicValue);

			if (attributes)
			{
				var refCusTariffBRCharacteristicAttribute = new EntityTypeConfiguration<RefCusTariffBRCharacteristicAttribute>(true);
				refCusTariffBRCharacteristicAttribute.IncludeColumn(x => x.ZB3_Name, true);
				refCusTariffBRCharacteristicAttribute.IncludeColumn(x => x.ZB3_Value, false);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffBRCharacteristicAttribute);
			}

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetNomenclatureBRCharacteristicConfiguration(string characteristicType, bool attributes = false, string constantStyle = null,
			bool defaultBooleanValues = false, bool defaultIsImport = false, bool defaultIsExport = false, bool defaultIsMandatory = false, bool defaultIsConditioningAttribute = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusNomenclatureGroup = new EntityTypeConfiguration<RefCusNomenclatureGroup>(false);
			refCusNomenclatureGroup.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			refCusNomenclatureGroup.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, Constants.DataGroupingCodes.Mercosul);
			refCusNomenclatureGroup.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusNomenclatureGroup.IncludeColumn(x => x.RefCusTariffBRCharacteristics, false);
			refCusNomenclatureGroup.IncludeColumn(x => x.ZZ5_Description, false);
			refCusNomenclatureGroup.IncludeColumn(x => x.ZZ5_Value, false);

			var refCusTariffBRCharacteristic = new EntityTypeConfiguration<RefCusTariffBRCharacteristic>(true);
			refCusTariffBRCharacteristic.IncludeColumnWithConstantValue(x => x.ZB1_CharacteristicType, true, characteristicType);
			if (string.IsNullOrEmpty(constantStyle))
			{
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_Style, false);
			}
			else
			{
				refCusTariffBRCharacteristic.IncludeColumnWithConstantValue(x => x.ZB1_Style, false, constantStyle);
			}
			refCusTariffBRCharacteristic.IncludeColumnWithConstantValue(x => x.ZB1_MaxLength, false, "0");
			refCusTariffBRCharacteristic.IncludeColumnWithConstantValue(x => x.ZB1_DecimalPlaces, false, "0");
			refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_Code, true);
			refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_Text, false);
			refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_StartDate, false, new DateTime(1900, 01, 01, 00, 00, 00));
			refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			if (defaultBooleanValues)
			{
				refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_IsImport, false, defaultIsImport);
				refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_IsExport, false, defaultIsExport);
				refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_IsMandatory, false, defaultIsMandatory);
				refCusTariffBRCharacteristic.IncludeColumnWithDefaultValue(x => x.ZB1_IsConditioningAttribute, false, defaultIsConditioningAttribute);
			}
			else
			{
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_IsImport, false);
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_IsExport, false);
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_IsMandatory, false);
				refCusTariffBRCharacteristic.IncludeColumn(x => x.ZB1_IsConditioningAttribute, false);
			}
			refCusTariffBRCharacteristic.IncludeColumn(x => x.RefCusTariffBRCharacteristicValues, false);

			if (attributes)
			{
				refCusTariffBRCharacteristic.IncludeColumn(x => x.RefCusTariffBRCharacteristicAttributes, false);
			}

			var refCusTariffBRCharacteristicValue = new EntityTypeConfiguration<RefCusTariffBRCharacteristicValue>(true);
			refCusTariffBRCharacteristicValue.IncludeColumn(x => x.ZB2_Value, true);
			refCusTariffBRCharacteristicValue.IncludeColumn(x => x.ZB2_Description, false);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusNomenclatureGroup);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffBRCharacteristic);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffBRCharacteristicValue);

			if (attributes)
			{
				var refCusTariffBRCharacteristicAttribute = new EntityTypeConfiguration<RefCusTariffBRCharacteristicAttribute>(true);
				refCusTariffBRCharacteristicAttribute.IncludeColumn(x => x.ZB3_Name, true);
				refCusTariffBRCharacteristicAttribute.IncludeColumn(x => x.ZB3_Value, false);
				writerConfiguration.IncludeEntityTypeConfiguration(refCusTariffBRCharacteristicAttribute);
			}

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusProcedureWriterConfiguration(string shipmentType,
			string constantPreviousProcedureCode = null, string constantConcession = null, bool constantCalculateDuty = false, bool constantStartDate = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusProcedureList = new EntityTypeConfiguration<RefCusProcedure>(true);
			refCusProcedureList.IncludeColumn(x => x.ZZ6_Description, false);
			refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_ShipmentType, true, shipmentType);
			refCusProcedureList.IncludeColumnWithDefaultValue(x => x.ZZ6_Category, true, "");
			refCusProcedureList.IncludeColumn(x => x.ZZ6_ProcedureCode, true);

			if (constantPreviousProcedureCode != null)
			{
				refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_PreviousProcedureCode, true, constantPreviousProcedureCode);
			}
			else
			{
				refCusProcedureList.IncludeColumnWithDefaultValue(x => x.ZZ6_PreviousProcedureCode, true, "");
			}
			if (constantConcession != null)
			{
				refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_Concession, true, constantConcession);
			}
			else
			{
				refCusProcedureList.IncludeColumnWithDefaultValue(x => x.ZZ6_Concession, true, "");
			}
			if (constantCalculateDuty)
			{
				refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_CalculateDuty, false, constantCalculateDuty);
			}
			else
			{
				refCusProcedureList.IncludeColumnWithDefaultValue(x => x.ZZ6_CalculateDuty, false, false);
			}
			refCusProcedureList.IncludeColumnWithDefaultValue(x => x.ZZ6_Group, false, "");

			if (constantStartDate)
			{
				refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_StartDate, false, new DateTime(1900, 01, 01, 00, 00, 00));
			}
			else
			{
				refCusProcedureList.IncludeColumn(x => x.ZZ6_StartDate, false);
			}
			refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_EndDate, false, new DateTime(2079, 06, 06, 00, 00, 00));
			refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusProcedureList);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusProcedureAttributesWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusProcedureList = new EntityTypeConfiguration<RefCusProcedure>(false);
			refCusProcedureList.IncludeColumn(x => x.ZZ6_ProcedureCode, true);
			refCusProcedureList.IncludeColumn(x => x.ZZ6_Description, false);
			refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_ShipmentType, true, Constants.ShipmentTypes.Export);
			refCusProcedureList.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			refCusProcedureList.IncludeColumn(x => x.RefCusProcedureAttributes, false);

			var refCusProcedureAttributeList = new EntityTypeConfiguration<RefCusProcedureAttribute>(true);
			refCusProcedureAttributeList.IncludeColumnWithConstantValue(x => x.ZXB_Name, true, Constants.SpecialSituation.Name);
			refCusProcedureAttributeList.IncludeColumn(x => x.ZXB_Value, true);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusProcedureList);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusProcedureAttributeList);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(string codeType)
		{
			Argument.NotNullOrEmpty(codeType, nameof(codeType));

			var writerConfiguration = new XmlWriterConfiguration();

			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, codeType);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_EndDate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.DataGroupingCodes.Brazil);

			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetHSNRefCusRateWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfig.IncludeColumn(x => x.RefCusRates);
			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.Codes.HSN);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			var rateConfig = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfig.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom);
			rateConfig.IncludeColumn(x => x.ZZ2_RateFormula);
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_StartDate, false, new DateTime(2017, 01, 01));
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, new DateTime(2079,06,06,23,59,00));
			rateConfig.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rateConfig.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfig);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusRateWriterConfiguration(DateTime? startDate = null, DateTime? endDate = null, string rateType = null, string rateCode = null, string preference = null)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(false);
			tariffConfig.IncludeColumn(x => x.RefCusRates);
			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.Codes.HSN);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			var rateConfig = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfig.IncludeColumn(x => x.RefCusApplicabilities);
			rateConfig.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom);
			rateConfig.IncludeColumn(x => x.ZZ2_RateFormula);
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_StartDate, false, startDate);
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, endDate ?? new DateTime(2079, 06, 06, 23, 59, 00));
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			if (rateType == null)
			{
				rateConfig.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			}
			else
			{
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, rateType);
			}

			if (rateCode == null)
			{
				rateConfig.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			}
			else
			{
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_NKRateCode, true, rateCode);
			}

			if (preference == null)
			{
				rateConfig.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
				rateConfig.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true);
			}
			else
			{
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_NKPreference, true, preference);
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			}

			if (startDate != null)
			{
				rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_StartDate, false, startDate);
			}
			else
			{
				rateConfig.IncludeColumn(x => x.ZZ2_StartDate, false);
			}

			var applicabilitiesConfig = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilitiesConfig.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_NKTradeGroup, true, Constants.Groups.ALL);
			applicabilitiesConfig.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			applicabilitiesConfig.IncludeColumnWithDefaultValue(x => x.ZZT_StartDate, false, startDate);
			applicabilitiesConfig.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, endDate ?? new DateTime(2079, 06, 06, 23, 59, 00));

			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilitiesConfig);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetChildTariffConfiguration(string tariffType, string tradeGroup,
			string tradeGroupDataGrouping = Constants.DataGroupingCodes.Brazil,
			bool defaultZZ1_EndDate = false,
			bool defaultZZ1_StartDate = false,
			string defaultZZ2_RateFormula = null,
			bool hasAttributes = true,
			string defaultZZ2_ZY1_ZZR_NKRateType = Constants.TariffRateTypes.DTY,
			string constantPreferenceDataGrouping = Constants.DataGroupingCodes.Brazil,
			string constantNKPreference = null)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfig.IncludeColumn(x => x.RefCusRates);
			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			if (defaultZZ1_StartDate)
			{
				tariffConfig.IncludeColumnWithDefaultValue(x => x.ZZ1_StartDate, false, new DateTime(2017, 01, 01, 00, 00, 00));
			}
			else
			{
				tariffConfig.IncludeColumn(x => x.ZZ1_StartDate);
			}
			if (defaultZZ1_EndDate)
			{
				tariffConfig.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			}
			tariffConfig.IncludeColumn(x => x.ZZ1_Description);
			if (!string.IsNullOrEmpty(tariffType))
			{
				tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, tariffType);
			}
			else
			{
				tariffConfig.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			}
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			tariffConfig.IncludeColumn(x => x.RefCusTariffRelationships, false);
			var tariffRelationshipConfig = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			tariffRelationshipConfig.IncludeColumn(x => x.ZZH_TariffCode, true);
			tariffRelationshipConfig.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, Constants.TariffTypes.Codes.HSN);
			tariffRelationshipConfig.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			var rateConfig = new EntityTypeConfiguration<RefCusRate>(true);
			if (!string.IsNullOrEmpty(defaultZZ2_RateFormula))
			{
				rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_RateFormula, false, $"VFD * {decimal.Parse(defaultZZ2_RateFormula, CultureInfo.CurrentCulture) / 100:0.00}");
				rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_RateFormulaDerivedFrom, false, defaultZZ2_RateFormula);
			}
			else
			{
				rateConfig.IncludeColumn(x => x.ZZ2_RateFormula, true);
				rateConfig.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom);
			}
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_StartDate, false, new DateTime(2017, 01, 01, 00, 00, 00));
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			rateConfig.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			if (!string.IsNullOrEmpty(defaultZZ2_ZY1_ZZR_NKRateType))
			{
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, defaultZZ2_ZY1_ZZR_NKRateType);
			}
			else
			{
				rateConfig.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			}
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);
			if (constantNKPreference == null)
			{
				rateConfig.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			}
			else
			{
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_NKPreference, true, constantNKPreference);
			}
			if (constantPreferenceDataGrouping == null)
			{
				rateConfig.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true);
			}
			else
			{
				rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, constantPreferenceDataGrouping);
			}
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Brazil);

			rateConfig.IncludeColumn(x => x.RefCusApplicabilities, false);
			var applicabilityConfig = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfig.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			if (!string.IsNullOrEmpty(tradeGroup))
			{
				applicabilityConfig.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_NKTradeGroup, true, tradeGroup);
			}
			else
			{
				applicabilityConfig.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			}
			applicabilityConfig.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, tradeGroupDataGrouping);
			applicabilityConfig.IncludeColumnWithConstantValue(x => x.ZZT_StartDate, false, new DateTime(2017, 01, 01, 00, 00, 00));
			applicabilityConfig.IncludeColumnWithConstantValue(x => x.ZZT_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));

			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffRelationshipConfig);
			if (hasAttributes)
			{
				tariffConfig.IncludeColumn(x => x.RefCusTariffAttributes, false);
				var tariffAttributeConfig = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
				tariffAttributeConfig.IncludeColumn(x => x.ZZ3_Name, true);
				tariffAttributeConfig.IncludeColumn(x => x.ZZ3_Value, false);

				writerConfiguration.IncludeEntityTypeConfiguration(tariffAttributeConfig);
			}

			writerConfiguration.IncludeEntityTypeConfiguration(rateConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfig);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusTradeGroupWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tradeGroupConfig = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroupConfig.IncludeColumn(x => x.RefCusTradeGroupCountries);
			tradeGroupConfig.IncludeColumnWithDefaultValue(x => x.ZZA_TradeGroup, true, Constants.RefCusTradeGroup.SACU.Code);
			tradeGroupConfig.IncludeColumnWithDefaultValue(x => x.ZZA_Description, false, Constants.RefCusTradeGroup.SACU.Description);
			tradeGroupConfig.IncludeColumnWithConstantValue(x => x.ZZA_StartDate, false, new DateTime(2016, 04, 01, 00, 00, 00));
			tradeGroupConfig.IncludeColumnWithDefaultValue(x => x.ZZA_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			tradeGroupConfig.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.Mercosul);

			var countriesConfig = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			countriesConfig.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			countriesConfig.IncludeColumn(x => x.ZZB_Description, false);
			countriesConfig.IncludeColumnWithConstantValue(x => x.ZZB_StartDate, false, new DateTime(2008, 05, 01, 23, 59, 00));
			countriesConfig.IncludeColumnWithDefaultValue(x => x.ZZB_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));

			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(countriesConfig);

			return writerConfiguration;
		}

		static XmlWriter GenerateXmlWriter<T>(string dataSource, DateTime publicationDateTime, XmlWriterConfiguration xmlWriterConfig, IEnumerable<T> dataList, UpdateType? updateType = UpdateType.Full)
		{
			Argument.NotNull(dataList, nameof(dataList));
			Argument.NotNull(xmlWriterConfig, nameof(xmlWriterConfig));

			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType.Value);
			foreach (var code in dataList)
			{
				writer.PopulateData(code);
			}

			return writer;
		}

		public static void ExportToXMLFile<T>(string outputFile, string dataSource, DateTime publicationDateTime, XmlWriterConfiguration xmlWriterConfig, IEnumerable<T> dataList, UpdateType? updateType = UpdateType.Full)
		{
			Argument.NotNull(dataList, nameof(dataList));
			var writer = GenerateXmlWriter(dataSource, publicationDateTime, xmlWriterConfig, dataList, updateType);
			if (writer != null)
			{
				var directoryName = Path.GetDirectoryName(outputFile);
				Directory.CreateDirectory(directoryName);
				writer.SaveXml(outputFile);
			}
		}

		public static void ExportToStream<T>(Stream outputFile, string dataSource, DateTime publicationDateTime, XmlWriterConfiguration xmlWriterConfig, IEnumerable<T> dataList, UpdateType? updateType = UpdateType.Full)
		{
			Argument.NotNull(dataList, nameof(dataList));
			var writer = GenerateXmlWriter(dataSource, publicationDateTime, xmlWriterConfig, dataList, updateType);
			writer.SaveXml(outputFile);
		}
	}
}
