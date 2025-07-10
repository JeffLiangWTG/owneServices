using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public class UpgradeScriptProvider : IUpgradeScriptProvider
	{
		// PLEASE ALWAYS CREATE AN ASPECT REVIEW TASK WITH RER CAPABILITY FOR REFERENCE TEAM TO REVIEW IF YOU MODIFY THIS CLASS, THANK YOU
		public int LatestVersion => 579;

		public int RequiredVersion => 1;
#if DEBUG
		public
#endif
		static IEnumerable<UpgradeWrapper> Upgrades
		{
			get
			{
				yield return new UpgradeWrapper(2, GetVersion2Script(), "Create Table RefDataGrouping");
				yield return new UpgradeWrapper(3, GetVersion3Script(), "Create Table RefCusMapType");
				yield return new UpgradeWrapper(4, GetVersion4Script(), "Create Table RefCusMap");
				yield return new UpgradeWrapper(5, GetVersion5Script(), "Create Table RefExchangeRateZZ");
				yield return new UpgradeWrapper(6, GetVersion6Script(), "Create Table RefVesselZZ");
				yield return new UpgradeWrapper(7, GetVersion7Script(), "Create Table RefCusNomenclatureGroupType");
				yield return new UpgradeWrapper(8, GetVersion8Script(), "Create Table RefLanguageType");
				yield return new UpgradeWrapper(9, GetVersion9Script(), "Create Table RefCusTariffAdditionalCodeCategory");
				yield return new UpgradeWrapper(10, GetVersion10Script(), "Create Table RefAccTaxRate");
				yield return new UpgradeWrapper(11, GetVersion11Script(), "Create Table RefHarbourRate");
				yield return new UpgradeWrapper(12, GetVersion12Script(), "Create Table RefCusAUNexdocECMCode");
				yield return new UpgradeWrapper(13, GetVersion13Script(), "Create Table RefDocOrgCusCode");
				yield return new UpgradeWrapper(14, GetVersion14Script(), "Create Table RefCusCodeType");
				yield return new UpgradeWrapper(15, GetVersion15Script(), "Create Table RefCusCodeTypeLanguage");
				yield return new UpgradeWrapper(16, GetVersion16Script(), "Create Table RefCusCodeListAttributeName");
				yield return new UpgradeWrapper(17, GetVersion17Script(), "Create Table RefCusCodeListAttributeNameLanguage");
				yield return new UpgradeWrapper(18, GetVersion18Script(), "Create Table RefCusTradeGroup");
				yield return new UpgradeWrapper(19, GetVersion19Script(), "Create Table RefCusTradeGroupCountry");
				yield return new UpgradeWrapper(20, GetVersion20Script(), "Create Table RefCusConditionType");
				yield return new UpgradeWrapper(21, GetVersion21Script(), "Create Table RefCusConditionTypeLanguage");
				yield return new UpgradeWrapper(22, GetVersion22Script(), "Create Table RefCusConditionValueType");
				yield return new UpgradeWrapper(23, GetVersion23Script(), "Create Table RefCusConditionValueTypeLanguage");
				yield return new UpgradeWrapper(24, GetVersion24Script(), "Create Table UNDGSubstanceADR");
				yield return new UpgradeWrapper(25, GetVersion25Script(), "Create Table UNDGAttributeZZ");
				yield return new UpgradeWrapper(26, GetVersion26Script(), "Create Table UNDGSubstanceRID");
				yield return new UpgradeWrapper(27, GetVersion27Script(), "Create Table UNDGSubstanceADN");
				yield return new UpgradeWrapper(28, GetVersion28Script(), "Create Table RefSysConfigType");
				yield return new UpgradeWrapper(29, GetVersion29Script(), "Create Table RefSysConfig");
				yield return new UpgradeWrapper(30, GetVersion30Script(), "Create Table RefCusCodeList");
				yield return new UpgradeWrapper(31, GetVersion31Script(), "Create Table RefCusCodeListAttribute");
				yield return new UpgradeWrapper(32, GetVersion32Script(), "Create Table RefCusCodeListLanguage");
				yield return new UpgradeWrapper(33, GetVersion33Script(), "Create Table RefCusCodeOrAttributeTransportMode");
				yield return new UpgradeWrapper(34, GetVersion34Script(), "Create Table RefCusProcedure");
				yield return new UpgradeWrapper(35, GetVersion35Script(), "Create Table RefCusProcedureAttribute");
				yield return new UpgradeWrapper(36, GetVersion36Script(), "Create Table RefCusProcedureLanguage");
				yield return new UpgradeWrapper(37, GetVersion37Script(), "Create Table RefCusTaxOrFeeType");
				yield return new UpgradeWrapper(38, GetVersion38Script(), "Create Table RefCusTaxOrFee");
				yield return new UpgradeWrapper(39, GetVersion39Script(), "Create Table RefCusTaxOrFeeLanguage");
				yield return new UpgradeWrapper(40, GetVersion40Script(), "Create Table RefCusRuling");
				yield return new UpgradeWrapper(41, GetVersion41Script(), "Create Table RefCusRulingConfig");
				yield return new UpgradeWrapper(42, GetVersion42Script(), "Create Table RefCusTariffType");
				yield return new UpgradeWrapper(43, GetVersion43Script(), "Create Table RefCusPreference");
				yield return new UpgradeWrapper(44, GetVersion44Script(), "Create Table RefCusPreferenceLanguage");
				yield return new UpgradeWrapper(45, GetVersion45Script(), "Create Table RefCusNomenclatureGroup");
				yield return new UpgradeWrapper(46, GetVersion46Script(), "Create Table RefCusNomenclatureGroupNote");
				yield return new UpgradeWrapper(47, GetVersion47Script(), "Create Table RefCusNomenclatureLanguage");
				yield return new UpgradeWrapper(48, GetVersion48Script(), "Create Table RefCarrierCode");
				yield return new UpgradeWrapper(49, GetVersion49Script(), "Create Table RefCarrierCodeAttribute");
				yield return new UpgradeWrapper(50, GetVersion50Script(), "Create Table RefCarrierVesselPivot");
				yield return new UpgradeWrapper(51, GetVersion51Script(), "Create Table RefCusRateType");
				yield return new UpgradeWrapper(52, GetVersion52Script(), "Create Table RefCusRateCode");
				yield return new UpgradeWrapper(53, GetVersion53Script(), "Create Table RefCusRateCodeLanguage");
				yield return new UpgradeWrapper(54, GetVersion54Script(), "Create Table RefCusRateTypeLanguage");
				yield return new UpgradeWrapper(55, GetVersion55Script(), "Create Table RefCusTariff");
				yield return new UpgradeWrapper(56, GetVersion56Script(), "Create Table RefCusTariffNationalCode");
				yield return new UpgradeWrapper(57, GetVersion57Script(), "Create Table RefCusTariffUOM");
				yield return new UpgradeWrapper(58, GetVersion58Script(), "Create Table RefCusTariffLanguage");
				yield return new UpgradeWrapper(59, GetVersion59Script(), "Create Table RefCusTariffRelationship");
				yield return new UpgradeWrapper(60, GetVersion60Script(), "Create Table RefCusTariffAttribute");
				yield return new UpgradeWrapper(61, GetVersion61Script(), "Create Table RefCusVATApplicability");
				yield return new UpgradeWrapper(62, GetVersion62Script(), "Create Table RefCusRate");
				yield return new UpgradeWrapper(63, GetVersion63Script(), "Create Table RefCusRateUOM");
				yield return new UpgradeWrapper(64, GetVersion64Script(), "Create Table RefCusCondition");
				yield return new UpgradeWrapper(65, GetVersion65Script(), "Create Table RefCusConditionValue");
				yield return new UpgradeWrapper(66, GetVersion66Script(), "Create Table RefCusTariffAdditionalCode");
				yield return new UpgradeWrapper(67, GetVersion67Script(), "Create Table RefCusTariffAdditionalCodeLanguage");
				yield return new UpgradeWrapper(68, GetVersion68Script(), "Create Table RefCusApplicability");
				yield return new UpgradeWrapper(69, GetVersion69Script(), "Create Table RefCusExcludedTradeGroup");
				yield return new UpgradeWrapper(70, GetVersion70Script(), "Create Table RefCusTradeGroupLanguage");
				yield return new UpgradeWrapper(83, GetVersion83Script(), "Create Trigger on Table RefCusCodeListAttribute");
				yield return new UpgradeWrapper(84, GetVersion84Script(), "Create Trigger on Table RefCusTariffAdditionalCode");
				yield return new UpgradeWrapper(85, GetVersion85Script(), "Create Trigger on Table RefCusCodeList");
				yield return new UpgradeWrapper(86, GetVersion86Script(), "Create Table RefDbVersionControl");
				yield return new UpgradeWrapper(195, GetVersion195Script(), "Create Trigger on Table RefCusCodeListAttributeName");
				yield return new UpgradeWrapper(196, GetVersion196Script(), "Add column ZY1_InternalUse to Table RefCusRateCode");    // Mapped to ZZ v196
				yield return new UpgradeWrapper(197, GetVersion197Script(), "Alter Table UNDGSubstanceADR Alter Columns");    // Mapped to ZZ v197
				yield return new UpgradeWrapper(198, GetVersion198Script(), "Create Table UNDGSubstanceJTT");    // Mapped to ZZ v198
				yield return new UpgradeWrapper(199, GetVersion199Script(), "Alter Table UNDGSubstanceADR Alter Column ADR_ADRTankSpecProv");    // Mapped to ZZ v200
				yield return new UpgradeWrapper(200, GetVersion200Script(), "Add and Drop Columns on Table RefCusProcedure");    // Mapped to ZZ v202
				yield return new UpgradeWrapper(201, GetVersion201Script(), "Add Check Constraints to Table RefCusProcedure");    // Mapped to ZZ v202
				yield return new UpgradeWrapper(202, GetVersion202Script(), "Create Table UNDGSubstanceCFR");    // Mapped to ZZ v203
				yield return new UpgradeWrapper(203, GetVersion203Script(), "Alter Table UNDGSubstanceCFR Alter Column CFR_PrimaryClass");    // Mapped to ZZ v204
				yield return new UpgradeWrapper(204, GetVersion204Script(), "Create Indexes on Table RefVesselZZ");    // Mapped to ZZ v206
				yield return new UpgradeWrapper(205, GetVersion205Script(), "Add Column and Constraints to Table RefCusVATApplicability");    // Mapped to ZZ v205
				yield return new UpgradeWrapper(210, GetVersion210Script(), "Add columns to Table UNDGSubstanceCFR");    // Mapped to ZZ v207
				yield return new UpgradeWrapper(211, GetVersion211Script(), "Drop Columns and Add Constraints to Table UNDGSubstanceCFR");    // Mapped to ZZ v207
				yield return new UpgradeWrapper(212, GetVersion212Script(), "Alter Table RefCusTaxOrFee Alter Columns");    // Mapped to ZZ v208
				yield return new UpgradeWrapper(213, GetVersion213Script(), "Add Constranints and Indexes to Table RefCusTaxOrFee");    // Mapped to ZZ v208
				yield return new UpgradeWrapper(214, GetVersion214Script(), "Create Table RefAirlineUNDGRule");    // Mapped to ZZ v210
				yield return new UpgradeWrapper(215, GetVersion215Script(), "Drop Triggers TG_RefCusCodeListAttributeName_INS_UPD and TG_RefCusCodeList_INS_UPD");    // Mapped to ZZ v212
				yield return new UpgradeWrapper(216, GetVersion216Script(), "Drop Constraint and Delete Records from Table RefCusCodeType");    // Mapped to ZZ v212
				yield return new UpgradeWrapper(217, GetVersion217Script(), "Add Constraint to Table RefCusCodeType");    // Mapped to ZZ v212
				yield return new UpgradeWrapper(218, GetVersion218Script(), "Create Trigger TG_RefCusCodeList_INS_UPD");    // Mapped to ZZ v212
				yield return new UpgradeWrapper(219, GetVersion219Script(), "Create Trigger TG_RefCusCodeListAttributeName_INS_UPD");    // Mapped to ZZ v212

				yield return new UpgradeWrapper(300, GetVersion300Script(), "Drop old Function,Procedures and Views");
				yield return new UpgradeWrapper(301, GetVersion301Script(), "Create View TariffView_V1");
				yield return new UpgradeWrapper(302, GetVersion302Script(), "Create View RateView_V1");
				yield return new UpgradeWrapper(303, GetVersion303Script(), "Create View TariffAttributeView_V1");
				yield return new UpgradeWrapper(304, GetVersion304Script(), "Create View TariffUOMView_V1");
				yield return new UpgradeWrapper(305, GetVersion305Script(), "Create View TariffRelationshipView");
				yield return new UpgradeWrapper(306, GetVersion306Script(), "Create View VATApplicabilityView_V1");
				yield return new UpgradeWrapper(307, GetVersion307Script(), "Create View TariffAdditionalCodeView_V1");
				yield return new UpgradeWrapper(308, GetVersion308Script(), "Create Procedure GetApplicableRatesWithoutDataGrouping_V1");
				yield return new UpgradeWrapper(309, GetVersion309Script(), "Create Procedure GetApplicableRates_V1");
				yield return new UpgradeWrapper(310, GetVersion310Script(), "Create Procedure GetApplicableConditions_V1");
				yield return new UpgradeWrapper(311, GetVersion311Script(), "Create Procedure GetApplicableConditionsBySingleAdditionalCode_V1");
				yield return new UpgradeWrapper(312, GetVersion312Script(), "Create Function GetRatesBySingleCriteriaSet_V1");
				yield return new UpgradeWrapper(313, GetVersion313Script(), "Create TableViews");
				yield return new UpgradeWrapper(314, GetVersion314Script(), "Create Table RefCusConditionLanguage and TableView RefCusConditionLanguageTableView_V1");    // Mapped to ZZ v214
				yield return new UpgradeWrapper(315, GetVersion315Script(), "Alter Table RefSysConfig Drop ZRC_ZRT_ConfigCode and Add New Column ZRC_ZRT_NKConfigCode");    // Mapped to ZZ v216
				yield return new UpgradeWrapper(316, GetVersion316Script(), "Add Column ZRC_BinaryValue to RefSysConfig");    // Mapped to ZZ v217
				yield return new UpgradeWrapper(317, GetVersion317Script(), "Alter Table RefSysConfig Add Constraint Constraint_ZRC_SingleFieldValue ");    // Mapped to ZZ v217
				yield return new UpgradeWrapper(318, GetVersion318Script(), "Create Table RefAirlineCommodityCode and RefAirlineCommodityCodeTableView_V1");    // Mapped to ZZ v218
				yield return new UpgradeWrapper(319, GetVersion319Script(), "Create Table RefStlScript and RefStlScriptTableView_V1");    // Mapped to ZZ v219 v222
				yield return new UpgradeWrapper(320, GetVersion320Script(), "Drop Column DAZ_IsSystem and Recreate TableView UNDGAttributeZZTableView_V1");
				yield return new UpgradeWrapper(321, GetVersion321Script(), "Create Table RefCusTariffTypeLanguage and RefCusTariffTypeLanguageTableView_V1");
				yield return new UpgradeWrapper(322, GetVersion322Script(), "Correct Default Constraints' Names");
				yield return new UpgradeWrapper(323, GetVersion323Script(), "Create Tables and TableViews RefCusTariffBRCharacteristic,RefCusTariffBRCharacteristicValue,RefCusTariffBRCharacteristicAttribute");
				yield return new UpgradeWrapper(324, GetVersion324Script(), "Alter Table RefDocOrgCusCode Recreate Constraint CK_RefDocOrgCusCode_DOC_DocumentType");
				yield return new UpgradeWrapper(325, GetVersion325Script(), "Create Table RefCusConfiguration and Table View RefCusConfigurationTableView_V1");
				yield return new UpgradeWrapper(326, GetVersion326Script(), "Add Column ZZT_ZZA_SecondTradeGroup and Recreate RefCusApplicability Table Views");    //Mapped to ZZ v228
				yield return new UpgradeWrapper(327, GetVersion327Script(), "Create Index on Table RefCusApplicability");    //Mapped to ZZ v228 - Index on new column only
				yield return new UpgradeWrapper(328, GetVersion328Script(), "Add Column STL_DateType and Create Table View RefStlScriptTableView_V2");
				yield return new UpgradeWrapper(329, GetVersion329Script(), "Add Constraint CK_RefStlScript_STL_DateType to Table RefStlScript");
				yield return new UpgradeWrapper(330, GetVersion330Script(), "Recreate TableView RefSysConfigTableView_V1");
				yield return new UpgradeWrapper(331, GetVersion331Script(), "Recreate RefStlScriptTableView_V1,RefStlScriptTableView_V2 and Create Table View RefStlScriptTableView_V3");
				yield return new UpgradeWrapper(332, GetVersion332Script(), "Recreate Constraint CK_RefExchangeRateZZ_ZZN_ExRateType");
				yield return new UpgradeWrapper(333, GetVersion333Script(), "Recreate unique index on Table RefStlScript");
				yield return new UpgradeWrapper(334, GetVersion334Script(), "Bump Version");
				yield return new UpgradeWrapper(335, GetVersion335Script(), "Add Foreign Key FK_RefCusConditionLanguage_RefCusCondition");
				yield return new UpgradeWrapper(336, GetVersion336Script(), "Create Table RefAirlineProductCode and RefAirlineProductCodeCommodityCodePivot");
				yield return new UpgradeWrapper(337, GetVersion337Script(), "Alter Table RefAirlineProductCode Recreate Constraint CK_RefAirlineProductCode_RAR_CodeNotEmpty");
				yield return new UpgradeWrapper(338, GetVersion338Script(), "Alter Table RefCusTariffNationalCode, RefCusVATApplicability and RefCusTariff Alter Columns ZZF_NKTaxOrFeeCode to Varchar(4)");
				yield return new UpgradeWrapper(339, GetVersion339Script(), "Create Table RefCusQuota");
				yield return new UpgradeWrapper(340, GetVersion340Script(), "Add index IX_RefDataGrouping_ZZZ_ZZZ_Grouping");
				yield return new UpgradeWrapper(341, GetVersion341Script(), "Add Not NULL to column ZXX_Description");
				yield return new UpgradeWrapper(342, GetVersion342Script(), "ALTER COLUMN ZZW_NationalCode to VARCHAR(10)"); //Mapped to ZZ V224
				yield return new UpgradeWrapper(343, GetVersion343Script(), "Recreate CK_RefCusTariffUOM_ZZ8_Type to include ZZ8_Type='CU5'"); //Mapped to ZZ V230
				yield return new UpgradeWrapper(344, GetVersion344Script(), "Add Not NULL to column ZZT_AdditionalCode and ZZT_OrderNumber");
				yield return new UpgradeWrapper(345, GetVersion345Script(), "Add Defualt vaule constraints to Table RefCusTariff and RefCusRateType and etc");
				yield return new UpgradeWrapper(346, GetVersion346Script(), "Create Table RefCarrierCodeLanguage");
				yield return new UpgradeWrapper(347, GetVersion347Script(), "synchronizing default value of columns");
				yield return new UpgradeWrapper(348, GetVersion348Script(), "Add RefCusTariffTableView_V2 & RefCusTariffNationalCodeTableView_V2 & RefCusVATApplicabilityTableView_V2");
				yield return new UpgradeWrapper(349, GetVersion349Script(), "synchronizing default value of columns");
				yield return new UpgradeWrapper(350, GetVersion350Script(), "Create Table RefStlFieldMapping");
				yield return new UpgradeWrapper(351, GetVersion351Script(), "Add RefCusTariffNationalCodeTableView_V3 and Update RefCusTariffNationalCodeTableView_V2");
				yield return new UpgradeWrapper(352, GetVersion352Script(), "Add new fields to RefStlFieldMapping reference database table");
				yield return new UpgradeWrapper(353, GetVersion353Script(), "Add Column ZY1_ZZZ_NKDataGrouping to RefCusRateCode");
				yield return new UpgradeWrapper(354, GetVersion354Script(), "Add RefCusConditionType Constraint and RefCusRateCode Index IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode");
				yield return new UpgradeWrapper(355, GetVersion355Script(), "Create Tables and TableViews RefCusTariffBRCharacteristic,RefCusTariffBRCharacteristicValue,RefCusTariffBRCharacteristicAttribute");
				yield return new UpgradeWrapper(356, GetVersion356Script(), "Add columns to RefExchangeRate index");
				yield return new UpgradeWrapper(357, GetVersion357Script(), "Select 1");
				yield return new UpgradeWrapper(358, GetVersion358Script(), "Add RefCusConditionTypeTableView_V2 and Update RefCusTariffNationalCodeTableView_V1");
				yield return new UpgradeWrapper(359, GetVersion359Script(), "Fix RefExchangeRate unique index");
				yield return new UpgradeWrapper(360, GetVersion360Script(), "Add RAC_SpecialHandlingCodes column to RefAirlineCommodityCode");
				yield return new UpgradeWrapper(361, GetVersion361Script(), "Rename non-standard default constraint names");
				yield return new UpgradeWrapper(362, GetVersion362Script(), "Create Tables and TableViews RefUNLOCO");
				yield return new UpgradeWrapper(363, GetVersion363Script(), "Create Tables and TableView RefUNLOCOUtcOffset");
				yield return new UpgradeWrapper(364, GetVersion364Script(), "Add new field ZB1_ZZ5_Nomenclature to table RefCusTariffBRCharacteristic");
				yield return new UpgradeWrapper(365, GetVersion365Script(), "Add Constraints to field ZB1_ZZ5_Nomenclature && ZB1_ZZ1_Tariff");
				yield return new UpgradeWrapper(366, GetVersion366Script(), "Create Tables and TableView RefCusTariffAttributeName");
				yield return new UpgradeWrapper(367, GetVersion367Script(), "Add new field ZB1_ZZ5_Nomenclature to table RefCusTariffBRCharacteristic(Server side)");
				yield return new UpgradeWrapper(368, GetVersion368Script(), "Add new rules for constains CK_RefCusTariffBRCharacteristic_ZB1_Style and CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType on RefCusTariffBRCharacteristic");
				yield return new UpgradeWrapper(369, GetVersion369Script(), "Add new field ZXF_PortTaxType to table RefHarbourRate and Create Table View RefHarbourRateTableView_V2");
				yield return new UpgradeWrapper(370, GetVersion370Script(), "Add new constraint CK_RefHarbourRate_ZXF_PortTaxType to table RefHarbourRate");
				yield return new UpgradeWrapper(371, GetVersion371Script(), "Create Table CMRPreferenceRulePeriodCharacteristic, CMRPreferenceRulePeriodCountry, CMRPreferenceRulePeriodSnapshot, CMRPreferenceRulePeriodTariffGroup, CMRPreferenceSchemePeriodCountry, CMRPreferenceSchemePeriodSnapshot, CMRPreferenceSchemeRule and CMRPreferenceSchemeRuleMessageAdvice");
				yield return new UpgradeWrapper(372, GetVersion372Script(), "Create Table CMRStatisticalClassificationPeriodCharacteristic");
				yield return new UpgradeWrapper(373, GetVersion373Script(), "Create Table CMRStatisticalClassificationPeriodMessageAdvice");
				yield return new UpgradeWrapper(374, GetVersion374Script(), "Create Table CMRStatisticalClassificationPeriodSnapshot");
				yield return new UpgradeWrapper(375, GetVersion375Script(), "Add ZXF_PortTaxType to RefHarbourRateUpdaterInfo");
				yield return new UpgradeWrapper(376, GetVersion376Script(), "Create Table CMRTariffClassificationCharacteristic");
				yield return new UpgradeWrapper(377, GetVersion377Script(), "Create Table CMRTariffClassificationConcordance");
				yield return new UpgradeWrapper(378, GetVersion378Script(), "Create Table CMRTariffClassificationMessageAdvice");
				yield return new UpgradeWrapper(379, GetVersion379Script(), "Create Table CMRTariffClassificationSnapshot");
				yield return new UpgradeWrapper(380, GetVersion380Script(), "Create Table CMRTreatmentSnapshot");
				yield return new UpgradeWrapper(381, GetVersion381Script(), "Create Table CMRTreatmentSnapshotCountry");
				yield return new UpgradeWrapper(382, GetVersion382Script(), "Create Table CMRTreatmentSnapshotTariffGroup");
				yield return new UpgradeWrapper(383, GetVersion383Script(), "Add ZZR_IsExport to RefCusRateType");
				yield return new UpgradeWrapper(384, GetVersion384Script(), "Fix RefUNLOCOUtcOffset unique index");
				yield return new UpgradeWrapper(385, GetVersion385Script(), "Create Table CMRInstrument");
				yield return new UpgradeWrapper(386, GetVersion386Script(), "Create Table CMRInstrumentCategory");
				yield return new UpgradeWrapper(387, GetVersion387Script(), "Create Table CMRInstrumentCategoryCharacteristic");
				yield return new UpgradeWrapper(388, GetVersion388Script(), "Create Table CMRInstrumentCategoryCountry");
				yield return new UpgradeWrapper(389, GetVersion389Script(), "Create Table CMRInstrumentCategoryTariffGroup");
				yield return new UpgradeWrapper(390, GetVersion390Script(), "Create Table CMRInstrumentCharacteristic");
				yield return new UpgradeWrapper(391, GetVersion391Script(), "Create Table CMRInstrumentCountry");
				yield return new UpgradeWrapper(392, GetVersion392Script(), "Create Table CMRInstrumentMessageAdvice");
				yield return new UpgradeWrapper(393, GetVersion393Script(), "Create Table CMRInstrumentTariffGroup");
				yield return new UpgradeWrapper(394, GetVersion394Script(), "Add DataGrouping to RefCusVATApplicability Start and End date unique indexes");
				yield return new UpgradeWrapper(395, GetVersion395Script(), "Add missing ZZR_RX_NKFormulaCurrency to RefCusRateType");
				yield return new UpgradeWrapper(396, GetVersion396Script(), "Create Table CMRTreatmentRatePeriodAdditionalDutyCalculation");
				yield return new UpgradeWrapper(397, GetVersion397Script(), "Create Table CMRTreatmentRatePeriodCharacteristic");
				yield return new UpgradeWrapper(398, GetVersion398Script(), "Create Table CMRTreatmentRatePeriodMessageAdvice");
				yield return new UpgradeWrapper(399, GetVersion399Script(), "Create Table CMRTreatmentRatePeriodSnapshot");
				yield return new UpgradeWrapper(400, GetVersion400Script(), "Create Table CMRCommunityProtectionProfile");
				yield return new UpgradeWrapper(401, GetVersion401Script(), "Create Table CMRCommunityProtectionRisk");
				yield return new UpgradeWrapper(402, GetVersion402Script(), "Create Table CMRCommunityProtectionRiskMessageAdvice");
				yield return new UpgradeWrapper(403, GetVersion403Script(), "Create View RefCusCodeListAttributeTransportModeView_V1 and RefCusCodeListTransportModeView_V1");
				yield return new UpgradeWrapper(404, GetVersion404Script(), "Add new field ZZ6_IntoVATWarehouse & ZZ6_OutOfVATWarehouse to table RefCusProcedure and Create Table View RefCusProcedureTableView_V2");
				yield return new UpgradeWrapper(405, GetVersion405Script(), "Add STL_CollectionStartDateUtc to RefStlScript");
				yield return new UpgradeWrapper(406, GetVersion406Script(), "Create Table CMRTariffRatePeriodSnapshot");
				yield return new UpgradeWrapper(407, GetVersion407Script(), "Create Table CMRTariffRatePeriodCharacteristic");
				yield return new UpgradeWrapper(408, GetVersion408Script(), "Create Table CMRTariffRatePeriodMessageAdvice");
				yield return new UpgradeWrapper(409, GetVersion409Script(), "Create Table CMRTariffRatePeriodAdditionalDutyCalculation");
				yield return new UpgradeWrapper(410, GetVersion410Script(), "Create Table CMRPermitRequirement");
				yield return new UpgradeWrapper(411, GetVersion411Script(), "Create Table CMRPermitRequirementExclusions");
				yield return new UpgradeWrapper(412, GetVersion412Script(), "Add new rules for constains CK_RefCusTariffBRCharacteristic_ZB1_Style and CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType on RefCusTariffBRCharacteristic");
				yield return new UpgradeWrapper(413, GetVersion413Script(), "Set LOCK_ESCALATION Disable for tariff DataSet tables.");
				yield return new UpgradeWrapper(414, GetVersion414Script(), "Recreate RefStlScriptTableView_V1-4 and Create Table View RefStlScriptTableView_V5.");
				yield return new UpgradeWrapper(415, GetVersion415Script(), "Add STL_CollectionStartDateUtc to RefStlScript");
				yield return new UpgradeWrapper(416, GetVersion416Script(), "Create Table RefCusConditionCode");
				yield return new UpgradeWrapper(417, GetVersion417Script(), "Create Table RefCusConditionCodeLanguage");
				yield return new UpgradeWrapper(418, GetVersion418Script(), "Add ZX1_AdditionalComment, ZX1_ZY7_NKConditionCode to RefCusCondition");
				yield return new UpgradeWrapper(419, GetVersion419Script(), "Add ZXJ_AdditionalComment to RefCusConditionLanguage");
				yield return new UpgradeWrapper(420, GetVersion420Script(), "Create Table RefMessagingBussPackageInfo");
				yield return new UpgradeWrapper(421, GetVersion421Script(), "Create Table RefMessagingBussPackageVersion");
				yield return new UpgradeWrapper(422, GetVersion422Script(), "Create Table RefMessagingBussCarrierInfo");
				yield return new UpgradeWrapper(423, GetVersion423Script(), "Update Constraint to Table UNDGSubstanceCFR");
				yield return new UpgradeWrapper(424, GetVersion424Script(), "Add View TariffUOMView_V2 with ZZ8_ZZZ_NKDataGrouping");
				yield return new UpgradeWrapper(425, GetVersion425Script(), "Add DataGrouping to RefCusVATApplicability Start and End date unique indexes(Server side)");
				yield return new UpgradeWrapper(426, GetVersion426Script(), "Replace CMRPreferenceScheme Tables to using Foreign Keys");
				yield return new UpgradeWrapper(427, GetVersion427Script(), "Update RefStlScript Constraints(Server side)");
				yield return new UpgradeWrapper(428, GetVersion428Script(), "Create Table RefClient");
				yield return new UpgradeWrapper(429, GetVersion429Script(), "Create Table and TableView RefUNLOCORelatedPort");
				yield return new UpgradeWrapper(430, GetVersion430Script(), "Update CMRInstrument Tables to use Foreign Keys");
				yield return new UpgradeWrapper(431, GetVersion431Script(), "Create Table RefClient(Server side)");
				yield return new UpgradeWrapper(432, GetVersion432Script(), "Add columns to Table UNDGSubstanceCFR");
				yield return new UpgradeWrapper(433, GetVersion433Script(), "Update RefCusProcedure and RefCusProcedureAttribute(Server side)");
				yield return new UpgradeWrapper(434, GetVersion434Script(), "Update CMRCommunityProtection Tables to use Foreign Keys");
				yield return new UpgradeWrapper(435, GetVersion435Script(), "Update UNDGSubstanceCFR Table to add new secondary values and units");
				yield return new UpgradeWrapper(436, GetVersion436Script(), "Add CLUSTERED INDEX IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_PK to RefCusTariff");
				yield return new UpgradeWrapper(437, GetVersion437Script(), "Add CLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_PK to RefCusRate");
				yield return new UpgradeWrapper(438, GetVersion438Script(), "Add CLUSTERED INDEX IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_PK to RefCusCondition");
				yield return new UpgradeWrapper(439, GetVersion439Script(), "Add CLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_PK to RefCusTariffAdditionalCode");
				yield return new UpgradeWrapper(440, GetVersion440Script(), "Add CLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_PK to RefCusTariffNationalCode");
				yield return new UpgradeWrapper(441, GetVersion441Script(), "Add CLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_PK to RefCusTariffUOM");
				yield return new UpgradeWrapper(442, GetVersion442Script(), "Add CLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK to RefCusVATApplicability");
				yield return new UpgradeWrapper(443, GetVersion443Script(), "Make NKDataGrouping the first column in indexes for RefCusRate");
				yield return new UpgradeWrapper(444, GetVersion444Script(), "Make NKDataGrouping the first column in indexes for RefCusCondition");
				yield return new UpgradeWrapper(445, GetVersion445Script(), "Make NKDataGrouping the first column in indexes for RefCusTariffUOM");
				yield return new UpgradeWrapper(446, GetVersion446Script(), "Rollback schema change of version 443");
				yield return new UpgradeWrapper(447, GetVersion447Script(), "Rollback schema change of version 444");
				yield return new UpgradeWrapper(448, GetVersion448Script(), "Rollback schema change of version 445");
				yield return new UpgradeWrapper(449, GetVersion449Script(), "Add computed column to RefCusVATApplicability and add VatApplicabilityView_V2");
				yield return new UpgradeWrapper(450, GetVersion450Script(), "Add RefCusTariffAttributeName on server side");
				yield return new UpgradeWrapper(451, GetVersion451Script(), "Update DB Indexes on RefCusApplicability");
				yield return new UpgradeWrapper(452, GetVersion452Script(), "Add RefCusConditionCode and RefCusConditionCodeLanguage on server side");
				yield return new UpgradeWrapper(453, GetVersion453Script(), "Update CMRTariffRatePeriodSnapshot Tables to use Foreign Keys");
				yield return new UpgradeWrapper(454, GetVersion454Script(), "Update CMRTreatmentSnapshotCountry Table to using Foreign Keys");
				yield return new UpgradeWrapper(455, GetVersion455Script(), "Update CMRTreatmentSnapshotTariffGroup Table to using Foreign Keys");
				yield return new UpgradeWrapper(456, GetVersion456Script(), "Update CMRTreatmentRatePeriodCharacteristic Table to using Foreign Keys");
				yield return new UpgradeWrapper(457, GetVersion457Script(), "Update CMRTreatmentRatePeriodAdditionalDutyCalculation Table to using Foreign Keys");
				yield return new UpgradeWrapper(458, GetVersion458Script(), "Update CMRStatisticalClassificationPeriodCharacteristic Table to using Foreign Keys");
				yield return new UpgradeWrapper(459, GetVersion459Script(), "Update View TariffView (_V2) with corrected end/start/published dates");
				yield return new UpgradeWrapper(460, GetVersion460Script(), "Add Column ZX5_DataSetId to Table RefCusVATApplicability");
				yield return new UpgradeWrapper(461, GetVersion461Script(), "Populate ZX5_DataSetId in RefCusVATApplicability");
				yield return new UpgradeWrapper(462, GetVersion462Script(), "Add CLUSTERED INDEX IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK to RefCusVATApplicability");
				yield return new UpgradeWrapper(463, GetVersion463Script(), "Add RefMessagingBussPackageInfo, RefMessagingBussCarrierInfo, RefMessagingBussPackageVersion(Server side)");
				yield return new UpgradeWrapper(464, GetVersion464Script(), "Alter Column ZX2_ConditionType FROM VARCHAR(5) to VARCHAR(6), Add a new TableView RefCusConditionTypeTableView_V3, Update RefCusConditionTypeTableView_V1 && RefCusConditionTypeTableView_V2");
				yield return new UpgradeWrapper(465, GetVersion465Script(), "Add UNIQUE CLUSTERED INDEX IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption to RefCusCodeListAttributeName");
				yield return new UpgradeWrapper(466, GetVersion466Script(), "Create Tables and TableViews RefCusProfile,RefCusProfileQuestion,RefCusProfileType,RefCusProfileAttribute");
				yield return new UpgradeWrapper(467, GetVersion467Script(), "Update Constraint to Table UNDGSubstanceCFR");
				yield return new UpgradeWrapper(468, GetVersion468Script(), "Alter column CFR_Variation to have 150 length instead of 80");
				yield return new UpgradeWrapper(469, GetVersion469Script(), "Server side: Add UNIQUE CLUSTERED INDEX IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption to RefCusCodeListAttributeName");
				yield return new UpgradeWrapper(470, GetVersion470Script(), "Add NONCLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode to RefCusTariffNationalCode");
				yield return new UpgradeWrapper(471, GetVersion471Script(), "Alter Column CFR_Variation to VARCHAR(150) on server side");
				yield return new UpgradeWrapper(472, GetVersion472Script(), "Alter Column ZX2_ConditionType FROM VARCHAR(5) to VARCHAR(6) on server side");
				yield return new UpgradeWrapper(473, GetVersion473Script(), "Alter Table RefExchangeRateZZ Alter Column ZZN_AsPublished From VARCHAR(10) to VARCHAR(35)");
				yield return new UpgradeWrapper(474, GetVersion474Script(), "Add Column DOC_Direction to Table RefDocOrgCusCode");
				yield return new UpgradeWrapper(475, GetVersion475Script(), "Add Column DOC_Direction, Server Side");
				yield return new UpgradeWrapper(476, GetVersion476Script(), "Fix RefCusProfile,RefCusProfileType,RefCusProfileAttribute Constraints, fix IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate_XX0_EndDate and create Tables and TableViews RefCusProfileQuestion,RefCusProfileQuestionAnswerList,RefCusProfileQuestionAttribute");
				yield return new UpgradeWrapper(477, GetVersion477Script(), "Add Constraints to RefExchangeRateZZ");
				yield return new UpgradeWrapper(478, GetVersion478Script(), "Alter Table RefAccTaxRate Alter Column ZAT_RN_NKCountry to CHAR(2)");
				yield return new UpgradeWrapper(479, GetVersion479Script(), "Alter Table RefCusConditionTypeLanguage Alter Column ZXW_Description to NOT NULL");
				yield return new UpgradeWrapper(480, GetVersion480Script(), "Alter Table RefCusTariffLanguage Alter Column ZX7_Description to NVARCHAR(MAX)");
				yield return new UpgradeWrapper(481, GetVersion481Script(), "Rename primary key of RefCusExcludedTradeGroup to PK_RefCusExcludedTradeGroup");
				yield return new UpgradeWrapper(482, GetVersion482Script(), "Recreate views RefCusTariffTableView_V1, RefCusTariffNationalCodeTableView_V1 and RefCusVATApplicabilityTableView_V1");
				yield return new UpgradeWrapper(483, GetVersion483Script(), "Alter column JTT_PSN to have 300 length instead of 260");
				yield return new UpgradeWrapper(484, GetVersion484Script(), "Update RefCusTariffUOM table and add RefCusTariffTableView_V2");
				yield return new UpgradeWrapper(485, GetVersion485Script(), "Update RefCusTariffUOM table add constraint as it failed if mutualised directly with 474");
				yield return new UpgradeWrapper(486, GetVersion486Script(), "Create Tables and TableViews RefCusProfileQuestionLanguage,RefCusProfileQuestionAnswerListLanguage,RefCusProfileQuestionPathway");
				yield return new UpgradeWrapper(487, GetVersion487Script(), "Server side: Add NONCLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZ1_Tariff_ZZW_ZZZ_NKDataGrouping_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode to RefCusTariffNationalCode");
				yield return new UpgradeWrapper(488, GetVersion488Script(), "Remove CMRInstrumentTariffGroup, CMRInstrument, CMRCommunityProtectionProfile, CMRCommunityProtectionRisk, CMRCommunityProtectionRiskMessageAdvice, CMRLodgementQuestion");
				yield return new UpgradeWrapper(489, GetVersion489Script(), "Remove CMRStatisticalClassificationPeriodCharacteristic, CMRStatisticalClassificationPeriodSnapshot, CMRPermitRequirement, CMRPermitRequirementExclusions, CMRPreferenceSchemePeriodCountry, CMRPreferenceSchemeRule, CMRPreferenceSchemePeriodSnapshot, CMRPreferenceRulePeriodTariffGroup, CMRPreferenceRulePeriodCharacteristic");
				yield return new UpgradeWrapper(490, GetVersion490Script(), "Remove CMRTreatmentRatePeriodAdditionalDutyCalculation, CMRTreatmentRatePeriodCharacteristic, CMRTreatmentRatePeriodSnapshot, CMRTariffRatePeriodAdditionalDutyCalculation, CMRTariffRatePeriodCharacteristic, CMRTariffRatePeriodSnapshot, CMRTreatmentSnapshotCountry, CMRTreatmentSnapshotTariffGroup, CMRTreatmentSnapshot");
				yield return new UpgradeWrapper(491, GetVersion491Script(), "Remove CMRInstrumentCategoryCharacteristic, CMRInstrumentCategory, CMRInstrumentCategoryCountry, CMRInstrumentCategoryTariffGroup, CMRInstrumentMessageAdvice, CMRInstrumentCharacteristic, CMRInstrumentCountry, CMRTreatmentRatePeriodMessageAdvice");
				yield return new UpgradeWrapper(492, GetVersion492Script(), "Remove CMRTariffClassificationSnapshot, CMRTariffClassificationCharacteristic, CMRTariffRatePeriodMessageAdvice, CMRTariffClassificationMessageAdvice, CMRTariffClassificationConcordance, CMRPreferenceSchemeRuleMessageAdvice, CMRPreferenceRulePeriodSnapshot, CMRPreferenceRulePeriodCountry, CMRStatisticalClassificationPeriodMessageAdvice");
				yield return new UpgradeWrapper(493, GetVersion493Script(), "Alter column JTT_PSN to have length 300 instead of 260 on server side");
				yield return new UpgradeWrapper(494, GetVersion494Script(), "Create Table and TableViews RefAccessorial");
				yield return new UpgradeWrapper(495, GetVersion495Script(), "Remove Unique Constraint from RefClient");
				yield return new UpgradeWrapper(496, GetVersion496Script(), "Update UNDGSubstanceJTTTableView_V1 JTT_PSN");
				yield return new UpgradeWrapper(497, GetVersion497Script(), "Remove UNIQUE from index IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort on RefUNLOCORelatedPort table");
				yield return new UpgradeWrapper(498, GetVersion498Script(), "Alter Table RefCusProfile, alter column XX0_XX2_NKQuestionCode to XX0_XQ2_QuestionCode");
				yield return new UpgradeWrapper(499, GetVersion499Script(), "Add Column ZY2_ZY2_TariffAdditionalCode what is a self-referencing FK column.");
				yield return new UpgradeWrapper(500, GetVersion500Script(), "add RefCusProfileType (Server side)");
				yield return new UpgradeWrapper(501, GetVersion501Script(), "Add Column ZX1_Severity to Table RefCusCondition");
				yield return new UpgradeWrapper(502, GetVersion502Script(), "Add Constraint CK_RefCusCondition_ZX1_Severity to Table RefCusCondition");
				yield return new UpgradeWrapper(503, GetVersion503Script(), "Alter Table RefCarrierCodeAttribute Alter Column ZZG_Value to NVARCHAR(MAX)");
				yield return new UpgradeWrapper(504, GetVersion504Script(), "Remove RefAccessorial table columns's constraints, indexes and view.");
				yield return new UpgradeWrapper(505, GetVersion505Script(), "Rename RefAccessorial table columns and add view.");
				yield return new UpgradeWrapper(506, GetVersion506Script(), "Add RefAccessorial table columns's constraints and indexes.");
				yield return new UpgradeWrapper(507, GetVersion507Script(), "Add column ZXE_IsDateRangeUsed to RefCusCodeListAttributeName");
				yield return new UpgradeWrapper(508, GetVersion508Script(), "Add columns ZZE_StartDate/ZZE_EndDate to RefCusCodeListAttribute");
				yield return new UpgradeWrapper(509, GetVersion509Script(), "Add check constrainit to ZZE_StartDate/ZZE_EndDate on RefCusCodeListAttribute");
				yield return new UpgradeWrapper(510, GetVersion510Script(), "Add trigger TG_RefCusCodeListAttributeName_INS_UPD_Dates");
				yield return new UpgradeWrapper(511, GetVersion511Script(), "Add trigger TG_RefCusCodeListAttribute_INS_UPD_Dates");
				yield return new UpgradeWrapper(512, GetVersion512Script(), "Alter Table RefCusProfile, alter column XX0_XQ2_QuestionCode to XX0_QuestionCode");
				yield return new UpgradeWrapper(513, GetVersion513Script(), "Alter RefExchangeRateZZ ZZN_AsPublished column to have 35 length instead of 10. Added BUY and SEL to Check constraint CK_RefExchangeRateZZ_ZZN_ExRateType");
				yield return new UpgradeWrapper(514, GetVersion514Script(), "Add RefCusProfile and RefCusProfileAttribute on server side");
				yield return new UpgradeWrapper(515, GetVersion515Script(), "Create unique index for RefCusProfileQuestion and RefCusProfileQuestionPathway");
				yield return new UpgradeWrapper(516, GetVersion516Script(), "Create Table and TableViews RefAccElectronicProcessingFee");
				yield return new UpgradeWrapper(517, GetVersion517Script(), "1.Alter field ZZ8_ZZA_SecondTradeGroup type + 2. Add foreign key + 3.Update View version");
				yield return new UpgradeWrapper(518, GetVersion518Script(), "1.Alter CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate + 2.Alter Index *Tariff + 3.Alter Index *NationalCode");
				yield return new UpgradeWrapper(519, GetVersion519Script(), "Synchronizing RefCusCodeList and RefCusCodeListAttribute schema with RefDatabase");
				yield return new UpgradeWrapper(520, GetVersion520Script(), "Add Columns EPF_CountryCode and EPF_JobDirection to Table RefAccElectronicProcessingFee");
				yield return new UpgradeWrapper(521, GetVersion521Script(), "Alter Table RefCarrierCodeAttribute Alter Column ZZG_Value to NVARCHAR(100)");
				yield return new UpgradeWrapper(522, GetVersion522Script(), "Add Column ZX1_Severity in RefCusCondition (Server Side)");
				yield return new UpgradeWrapper(523, GetVersion523Script(), "Alter table RefAccElectronicProcessingFee on server side");
				yield return new UpgradeWrapper(524, GetVersion524Script(), "Create Table UNDGVersion");
				yield return new UpgradeWrapper(525, GetVersion525Script(), "Add Indexes on ForeignKey Columns for Tariff DataSet");
				yield return new UpgradeWrapper(526, GetVersion526Script(), "Recreate trigger TG_RefCusCodeListAttribute_INS_UPD");
				yield return new UpgradeWrapper(527, GetVersion527Script(), "roll back Recreate trigger TG_RefCusCodeListAttribute_INS_UPD");
				yield return new UpgradeWrapper(528, GetVersion528Script(), "Remove Tariff SPs & FNs from RefDatabase");
				yield return new UpgradeWrapper(529, GetVersion529Script(), "Add Start-EndDate and ZZ8_ZZA_SecondTradeGroup to RefCusTariffUOM");
				yield return new UpgradeWrapper(530, GetVersion530Script(), "Remove column ZY2_ZY2_TariffAdditionalCode and add columns ZY2_ParentAdditionalCode and ZY2_ZY3_NKParentCategory");
				yield return new UpgradeWrapper(531, GetVersion531Script(), "Add Trigger TG_RefCusTariffAdditionalCode_INS_UPD_Parents");
				yield return new UpgradeWrapper(532, GetVersion532Script(), "Create Table RefVesselArrival");
				yield return new UpgradeWrapper(533, GetVersion533Script(), "Alter RefExchangeRateZZ indexes");
				yield return new UpgradeWrapper(534, GetVersion534Script(), "Create Table RefGlbReleaseNote");
				yield return new UpgradeWrapper(535, GetVersion535Script(), "Add Columns AllowMultipleAnswers and IsAnswerMandatory to Tables RefCusProfile and RefCusProfileQuestionPathway");
				yield return new UpgradeWrapper(536, GetVersion536Script(), "Add column ZZT_ZZH_TariffRelationship");
				yield return new UpgradeWrapper(537, GetVersion537Script(), "Add contraint for ZZT_ZZH_TariffRelationship");
				yield return new UpgradeWrapper(538, GetVersion538Script(), "Alter Table RefCusTariffType Alter Column ZZI_Description to NVARCHAR(100)");
				yield return new UpgradeWrapper(539, GetVersion539Script(), "Add columns ZY2_ParentAdditionalCode and ZY2_ZY3_NKParentCategory to RefCusTariffAdditionalCode on server side");
				yield return new UpgradeWrapper(540, GetVersion540Script(), "Create Table RefGlbReleaseNote(Server Side)");
				yield return new UpgradeWrapper(541, GetVersion541Script(), "Create Table RefMessagingBussAttributeInfo");
				yield return new UpgradeWrapper(542, GetVersion542Script(), "Create Table RefMessagingBussCarrierInfoAttribute");
				yield return new UpgradeWrapper(543, GetVersion543Script(), "Create Table RefMessagingBussPackageInfoAttribute");
				yield return new UpgradeWrapper(544, GetVersion544Script(), "Create View RefGlbReleaseNoteTableView_2");
				yield return new UpgradeWrapper(545, GetVersion545Script(), "Alter Table RefCusTariffType Alter Column ZZI_Description to NVARCHAR(100) on server side");
				yield return new UpgradeWrapper(546, GetVersion546Script(), "Alter RefCusProfileType make column XXX_ZZI_TariffType nullable");
				yield return new UpgradeWrapper(547, GetVersion547Script(), "Alter RefCusProfile rename column XX0_TariffCode to XX0_AppliesToCode");
				yield return new UpgradeWrapper(548, GetVersion548Script(), "Add View TariffAdditionalCodeView_V2 with ZY2_ParentAdditionalCode and ZY2_ZY3_NKParentCategory");
				yield return new UpgradeWrapper(549, GetVersion549Script(), "Re-create TariffAdditionalCodeTableView_V2");
				yield return new UpgradeWrapper(550, GetVersion550Script(), "Add column AllowMultipleAnswers/IsAnswerMandatory to tables RefCusProfile and RefCusProfileQuestionPathway on Server Side");
				yield return new UpgradeWrapper(551, GetVersion551Script(), "Populate NULL value with '' in RefStlScript Table");
				yield return new UpgradeWrapper(552, GetVersion552Script(), "Alter Columns from NULL to NOT NULL in in RefStlScript Table");
				yield return new UpgradeWrapper(553, GetVersion553Script(), "Create Table UNDGVersion(Server Side)");
				yield return new UpgradeWrapper(554, GetVersion554Script(), "Re-create trigger TG_RefCusCodeListAttribute_INS_UPD");
				yield return new UpgradeWrapper(555, GetVersion555Script(), "Add Column MinCW1Version and MaxCW1Version to all UNDG standard tables and views");
				yield return new UpgradeWrapper(556, GetVersion556Script(), "Alter column STL_AdditionalRefs from VARCHAR(1000) to VARCHAR(MAX)");
				yield return new UpgradeWrapper(557, GetVersion557Script(), "Create UNIQUE IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory of RefCusTariffAdditionalCode");
				yield return new UpgradeWrapper(558, GetVersion558Script(), "Create View TariffUOMView_V3");
				yield return new UpgradeWrapper(559, GetVersion559Script(), "Alter column ZXE_ZZK_NKCodeType/ZXE_ZZK_NKCodeTypeForValueList from VARCHAR(5) to VARCHAR(10)");
				yield return new UpgradeWrapper(560, GetVersion560Script(), "Alter column ZZD_ZZK_NKCodeType from VARCHAR(5) to VARCHAR(10)");
				yield return new UpgradeWrapper(561, GetVersion561Script(), "Alter column ZZK_CodeType from VARCHAR(5) to VARCHAR(10)");
				yield return new UpgradeWrapper(562, GetVersion562Script(), "Create Table RefVesselArrival on Server Side");
				yield return new UpgradeWrapper(563, GetVersion563Script(), "Create Table and TableView for UNDGSubstanceTDG");
				yield return new UpgradeWrapper(564, GetVersion564Script(), "The Anchor Version: No need to bump ContractVersion for RDU Schema Change after this Version");
				yield return new UpgradeWrapper(565, GetVersion565Script(), "Recreate TableView RefCusCodeListTableView_V1 and RefCusCodeTypeTableView_V1");
				yield return new UpgradeWrapper(566, GetVersion566Script(), "Create Tables and TableViews RefCusCodeTypeAttribute,RefCusCodeTypeAttributeName");
				yield return new UpgradeWrapper(567, GetVersion567Script(), "Rollback 565 change");
				yield return new UpgradeWrapper(568, GetVersion568Script(), "Add Column ZY2_StartDate/ZY2_EndDate.");
				yield return new UpgradeWrapper(569, GetVersion569Script(), "Add check constrainit to ZY2_StartDate/ZY2_EndDate on RefCusTariffAdditionalCode Column ZY2_StartDate/ZY2_EndDate.");
				yield return new UpgradeWrapper(570, GetVersion570Script(), "Add ZXE_ZZK_NKCodeTypeComputed columns and related index");
				yield return new UpgradeWrapper(571, GetVersion571Script(), "Add ZZD_ZZK_NKCodeTypeComputed columns and related index");
				yield return new UpgradeWrapper(572, GetVersion572Script(), "Add ZZK_CodeTypeComputed columns and related index");
				yield return new UpgradeWrapper(573, GetVersion573Script(), "Rename column XQ2_Code of RefCusProfileQuestion to XQ2_QuestionCode");
				yield return new UpgradeWrapper(574, GetVersion574Script(), "Add related objects of RefCusProfileQuestion.XQ2_QuestionCode");
				yield return new UpgradeWrapper(575, GetVersion575Script(), "Add check constraint to STL_CollectionStartDateUtc on RefStlScript");
				yield return new UpgradeWrapper(576, GetVersion576Script(), "Add View TariffAdditionalCodeView_V3 with ZY2_StartDate and ZY2_EndDate");
				yield return new UpgradeWrapper(577, GetVersion577Script(), "Add TariffRelationship to RefCusApplicability");
				yield return new UpgradeWrapper(578, GetVersion578Script(), "Recreate TableView RefCusProfileTypeTableView_V1 and create RefCusProfileTypeTableView_V2");
				yield return new UpgradeWrapper(579, GetVersion579Script(), "Re-Publish RefExchangeRateZZ/RefCusQuota/RefCusTaxOrFee");
			}
		}

		public UpgradeWrapper GetUpgradeWrapperByVersion(int version)
		{
			return Upgrades.FirstOrDefault(x => x.UpgradeVersion == version);
		}

		public IEnumerable<int> GetAvailableVersionsAfterVersion(int version)
		{
			return Upgrades.Where(x => x.UpgradeVersion > version).OrderBy(x => x.UpgradeVersion).Select(x => x.UpgradeVersion);
		}

		static string GetCreateTableWithIndexSql(ITableScript script)
		{
			return SharedDbSchemaChange.GetCreateTableIfNotExistsScript(script.TableName, script.CreateTableScript).Replace("GO\r\n", string.Empty);
		}

		#region Version 2 Upgrade Script
		static string GetVersion2Script()
		{
			return GetCreateTableWithIndexSql(new RefDataGrouping());
		}

		#endregion

		#region Version 3 Upgrade Script
		static string GetVersion3Script()
		{
			return GetCreateTableWithIndexSql(new RefCusMapType());
		}

		#endregion

		#region Version 4 Upgrade Script
		static string GetVersion4Script()
		{
			return GetCreateTableWithIndexSql(new RefCusMap());
		}

		#endregion

		#region Version 5 Upgrade Script
		static string GetVersion5Script()
		{
			return GetCreateTableWithIndexSql(new RefExchangeRateZZ());
		}

		#endregion

		#region Version 6 Upgrade Script
		static string GetVersion6Script()
		{
			return GetCreateTableWithIndexSql(new RefVesselZZ());
		}

		#endregion

		#region Version 7 Upgrade Script
		static string GetVersion7Script()
		{
			return GetCreateTableWithIndexSql(new RefCusNomenclatureGroupType());
		}

		#endregion

		#region Version 8 Upgrade Script
		static string GetVersion8Script()
		{
			return GetCreateTableWithIndexSql(new RefLanguageType());
		}

		#endregion

		#region Version 9 Upgrade Script
		static string GetVersion9Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffAdditionalCodeCategory());
		}

		#endregion

		#region Version 10 Upgrade Script
		static string GetVersion10Script()
		{
			return GetCreateTableWithIndexSql(new RefAccTaxRate());
		}

		#endregion

		#region Version 11 Upgrade Script
		static string GetVersion11Script()
		{
			return GetCreateTableWithIndexSql(new RefHarbourRate());
		}

		#endregion

		#region Version 12 Upgrade Script
		static string GetVersion12Script()
		{
			return GetCreateTableWithIndexSql(new RefCusAUNexdocECMCode());
		}

		#endregion

		#region Version 13 Upgrade Script
		static string GetVersion13Script()
		{
			return GetCreateTableWithIndexSql(new RefDocOrgCusCode());
		}

		#endregion

		#region Version 14 Upgrade Script
		static string GetVersion14Script()
		{
			return GetCreateTableWithIndexSql(new RefCusCodeType());
		}

		#endregion

		#region Version 15 Upgrade Script
		static string GetVersion15Script()
		{
			return GetCreateTableWithIndexSql(new RefCusCodeTypeLanguage());
		}

		#endregion

		#region Version 16 Upgrade Script
		static string GetVersion16Script()
		{
			return GetCreateTableWithIndexSql(new RefCusCodeListAttributeName());
		}

		#endregion

		#region Version 17 Upgrade Script
		static string GetVersion17Script()
		{
			return GetCreateTableWithIndexSql(new RefCusCodeListAttributeNameLanguage());
		}

		#endregion

		#region Version 18 Upgrade Script
		static string GetVersion18Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTradeGroup());
		}

		#endregion

		#region Version 19 Upgrade Script
		static string GetVersion19Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTradeGroupCountry());
		}

		#endregion

		#region Version 20 Upgrade Script
		static string GetVersion20Script()
		{
			return GetCreateTableWithIndexSql(new RefCusConditionType());
		}

		#endregion

		#region Version 21 Upgrade Script
		static string GetVersion21Script()
		{
			return GetCreateTableWithIndexSql(new RefCusConditionTypeLanguage());
		}

		#endregion

		#region Version 22 Upgrade Script
		static string GetVersion22Script()
		{
			return GetCreateTableWithIndexSql(new RefCusConditionValueType());
		}

		#endregion

		#region Version 23 Upgrade Script
		static string GetVersion23Script()
		{
			return GetCreateTableWithIndexSql(new RefCusConditionValueTypeLanguage());
		}

		#endregion

		#region Version 24 Upgrade Script
		static string GetVersion24Script()
		{
			return GetCreateTableWithIndexSql(new UNDGSubstanceADR());
		}

		#endregion

		#region Version 25 Upgrade Script
		static string GetVersion25Script()
		{
			return GetCreateTableWithIndexSql(new UNDGAttributeZZ());
		}

		#endregion

		#region Version 26 Upgrade Script
		static string GetVersion26Script()
		{
			return GetCreateTableWithIndexSql(new UNDGSubstanceRID());
		}

		#endregion

		#region Version 27 Upgrade Script
		static string GetVersion27Script()
		{
			return GetCreateTableWithIndexSql(new UNDGSubstanceADN());
		}

		#endregion

		#region Version 28 Upgrade Script
		static string GetVersion28Script()
		{
			return GetCreateTableWithIndexSql(new RefSysConfigType());
		}

		#endregion

		#region Version 29 Upgrade Script
		static string GetVersion29Script()
		{
			return GetCreateTableWithIndexSql(new RefSysConfig());
		}

		#endregion

		#region Version 30 Upgrade Script
		static string GetVersion30Script()
		{
			return GetCreateTableWithIndexSql(new RefCusCodeList());
		}

		#endregion

		#region Version 31 Upgrade Script
		static string GetVersion31Script()
		{
			return GetCreateTableWithIndexSql(new RefCusCodeListAttribute());
		}

		#endregion

		#region Version 32 Upgrade Script
		static string GetVersion32Script()
		{
			return GetCreateTableWithIndexSql(new RefCusCodeListLanguage());
		}

		#endregion

		#region Version 33 Upgrade Script
		static string GetVersion33Script()
		{
			return GetCreateTableWithIndexSql(new RefCusCodeOrAttributeTransportMode());
		}

		#endregion

		#region Version 34 Upgrade Script
		static string GetVersion34Script()
		{
			return GetCreateTableWithIndexSql(new RefCusProcedure());
		}

		#endregion

		#region Version 35 Upgrade Script
		static string GetVersion35Script()
		{
			return GetCreateTableWithIndexSql(new RefCusProcedureAttribute());
		}

		#endregion

		#region Version 36 Upgrade Script
		static string GetVersion36Script()
		{
			return GetCreateTableWithIndexSql(new RefCusProcedureLanguage());
		}

		#endregion

		#region Version 37 Upgrade Script
		static string GetVersion37Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTaxOrFeeType());
		}

		#endregion

		#region Version 38 Upgrade Script
		static string GetVersion38Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTaxOrFee());
		}

		#endregion

		#region Version 39 Upgrade Script
		static string GetVersion39Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTaxOrFeeLanguage());
		}

		#endregion

		#region Version 40 Upgrade Script
		static string GetVersion40Script()
		{
			return GetCreateTableWithIndexSql(new RefCusRuling());
		}

		#endregion

		#region Version 41 Upgrade Script
		static string GetVersion41Script()
		{
			return GetCreateTableWithIndexSql(new RefCusRulingConfig());
		}

		#endregion

		#region Version 42 Upgrade Script
		static string GetVersion42Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffType());
		}

		#endregion

		#region Version 43 Upgrade Script
		static string GetVersion43Script()
		{
			return GetCreateTableWithIndexSql(new RefCusPreference());
		}

		#endregion

		#region Version 44 Upgrade Script
		static string GetVersion44Script()
		{
			return GetCreateTableWithIndexSql(new RefCusPreferenceLanguage());
		}

		#endregion

		#region Version 45 Upgrade Script
		static string GetVersion45Script()
		{
			return GetCreateTableWithIndexSql(new RefCusNomenclatureGroup());
		}

		#endregion

		#region Version 46 Upgrade Script
		static string GetVersion46Script()
		{
			return GetCreateTableWithIndexSql(new RefCusNomenclatureGroupNote());
		}

		#endregion

		#region Version 47 Upgrade Script
		static string GetVersion47Script()
		{
			return GetCreateTableWithIndexSql(new RefCusNomenclatureLanguage());
		}

		#endregion

		#region Version 48 Upgrade Script
		static string GetVersion48Script()
		{
			return GetCreateTableWithIndexSql(new RefCarrierCode());
		}

		#endregion

		#region Version 49 Upgrade Script
		static string GetVersion49Script()
		{
			return GetCreateTableWithIndexSql(new RefCarrierCodeAttribute());
		}

		#endregion

		#region Version 50 Upgrade Script
		static string GetVersion50Script()
		{
			return GetCreateTableWithIndexSql(new RefCarrierVesselPivot());
		}

		#endregion

		#region Version 51 Upgrade Script
		static string GetVersion51Script()
		{
			return GetCreateTableWithIndexSql(new RefCusRateType());
		}

		#endregion

		#region Version 52 Upgrade Script
		static string GetVersion52Script()
		{
			return GetCreateTableWithIndexSql(new RefCusRateCode());
		}

		#endregion

		#region Version 53 Upgrade Script
		static string GetVersion53Script()
		{
			return GetCreateTableWithIndexSql(new RefCusRateCodeLanguage());
		}

		#endregion

		#region Version 54 Upgrade Script
		static string GetVersion54Script()
		{
			return GetCreateTableWithIndexSql(new RefCusRateTypeLanguage());
		}

		#endregion

		#region Version 55 Upgrade Script
		static string GetVersion55Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariff());
		}

		#endregion

		#region Version 56 Upgrade Script
		static string GetVersion56Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffNationalCode());
		}

		#endregion

		#region Version 57 Upgrade Script
		static string GetVersion57Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffUOM());
		}

		#endregion

		#region Version 58 Upgrade Script
		static string GetVersion58Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffLanguage());
		}

		#endregion

		#region Version 59 Upgrade Script
		static string GetVersion59Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffRelationship());
		}

		#endregion

		#region Version 60 Upgrade Script
		static string GetVersion60Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffAttribute());
		}

		#endregion

		#region Version 61 Upgrade Script
		static string GetVersion61Script()
		{
			return GetCreateTableWithIndexSql(new RefCusVATApplicability());
		}

		#endregion

		#region Version 62 Upgrade Script
		static string GetVersion62Script()
		{
			return GetCreateTableWithIndexSql(new RefCusRate());
		}

		#endregion

		#region Version 63 Upgrade Script
		static string GetVersion63Script()
		{
			return GetCreateTableWithIndexSql(new RefCusRateUOM());
		}

		#endregion

		#region Version 64 Upgrade Script
		static string GetVersion64Script()
		{
			// the FK_RefCusCondition_RefCusConditionCode is added back in GetVersion417Script()
			return RemoveContainingLine(GetCreateTableWithIndexSql(new RefCusCondition()), "[FK_RefCusCondition_RefCusConditionCode]");
		}

		#endregion

		#region Version 65 Upgrade Script
		static string GetVersion65Script()
		{
			return GetCreateTableWithIndexSql(new RefCusConditionValue());
		}

		#endregion

		#region Version 66 Upgrade Script
		static string GetVersion66Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffAdditionalCode());
		}

		#endregion

		#region Version 67 Upgrade Script
		static string GetVersion67Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTariffAdditionalCodeLanguage());
		}

		#endregion

		#region Version 68 Upgrade Script
		static string GetVersion68Script()
		{
			return GetCreateTableWithIndexSql(new RefCusApplicability());
		}

		#endregion

		#region Version 69 Upgrade Script
		static string GetVersion69Script()
		{
			return GetCreateTableWithIndexSql(new RefCusExcludedTradeGroup());
		}

		#endregion

		#region Version 70 Upgrade Script
		static string GetVersion70Script()
		{
			return GetCreateTableWithIndexSql(new RefCusTradeGroupLanguage());
		}

		#endregion

		#region Version 83 Upgrade Script
		static string GetVersion83Script()
		{
			return new Trigger("TG_RefCusCodeListAttribute_INS_UPD").GetCreateSqlScriptByVersion();
		}

		#endregion

		#region Version 84 Upgrade Script
		static string GetVersion84Script()
		{
			return new Trigger("TG_RefCusTariffAdditionalCode_INS_UPD").GetCreateSqlScriptByVersion();
		}

		#endregion

		#region Version 85 Upgrade Script
		static string GetVersion85Script()
		{
			return new Trigger("TG_RefCusCodeList_INS_UPD").GetCreateSqlScriptByVersion();
		}

		#endregion

		#region Version 86 Upgrade Script
		static string GetVersion86Script()
		{
			return GetCreateTableWithIndexSql(new RefDbVersionControl());
		}

		#endregion

		#region Version 195 Upgrade Script
		static string GetVersion195Script()
		{
			return new Trigger("TG_RefCusCodeListAttributeName_INS_UPD").GetCreateSqlScriptByVersion();
		}

		#endregion

		#region Version 196 Upgrade Script
		static string GetVersion196Script()
		{
			var refCusRateCode = new RefCusRateCode();
			return SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusRateCode.TableName, "ZY1_InternalUse", "BIT NOT NULL CONSTRAINT DF_RefCusRateCode_ZY1_InternalUse DEFAULT 0");
		}

		#endregion

		#region Version 197 Upgrade Script
		static string GetVersion197Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine("ALTER TABLE UNDGSubstanceADR ALTER COLUMN ADR_PSN VARCHAR(260) NOT NULL");
			sb.AppendLine("ALTER TABLE UNDGSubstanceADR ALTER COLUMN ADR_SpecialProvisions VARCHAR(40) NOT NULL");
			sb.AppendLine("ALTER TABLE UNDGSubstanceADR ALTER COLUMN ADR_BulkTankIns VARCHAR(80) NOT NULL");
			sb.AppendLine("ALTER TABLE UNDGSubstanceADR ALTER COLUMN ADR_TransportCategory VARCHAR(15) NOT NULL");
			sb.AppendLine("ALTER TABLE UNDGSubstanceADR ALTER COLUMN ADR_BulkSpecialProv VARCHAR(20) NOT NULL");
			return sb.ToString();
		}

		#endregion

		#region Version 198 Upgrade Script
		static string GetVersion198Script()
		{
			return GetCreateTableWithIndexSql(new UNDGSubstanceJTT());
		}

		#endregion

		#region Version 199 Upgrade Script
		static string GetVersion199Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine("ALTER TABLE UNDGSubstanceADR ALTER COLUMN ADR_ADRTankSpecProv VARCHAR(80) NOT NULL");
			return sb.ToString();
		}

		#endregion

		#region Version 200 Upgrade Script
		static string GetVersion200Script()
		{
			var refCusProcedure = new RefCusProcedure();
			var refCusProcedureTableName = refCusProcedure.TableName;
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProcedureTableName, "DF_RefCusProcedure_ZZ6_IntoTemporaryProcedure"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProcedureTableName, "DF_RefCusProcedure_ZZ6_OutOfTemporaryProcedure"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_IntoWarehouse"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_OutOfWarehouse"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_IntoTemporaryProcedure"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_OutOfTemporaryProcedure"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_OutOfInwardProcessing"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_IntoInwardProcessing"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_IntoOutwardProcessing"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_OutofOutwardProcessing"));

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(
				refCusProcedureTableName,
				"ZZ6_IntoTemporaryImport", "CHAR(1) NOT NULL CONSTRAINT DF_RefCusProcedure_ZZ6_IntoTemporaryImport DEFAULT ('N')"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(
				refCusProcedureTableName,
				"ZZ6_OutOfTemporaryImport", "CHAR(1) NOT NULL CONSTRAINT DF_RefCusProcedure_ZZ6_OutOfTemporaryImport DEFAULT ('N')"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(
				refCusProcedureTableName,
				"ZZ6_IntoTemporaryExport", "CHAR(1) NOT NULL CONSTRAINT DF_RefCusProcedure_ZZ6_IntoTemporaryExport DEFAULT ('N')"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(
				refCusProcedureTableName,
				"ZZ6_OutOfTemporaryExport", "CHAR(1) NOT NULL CONSTRAINT DF_RefCusProcedure_ZZ6_OutOfTemporaryExport DEFAULT ('N')"));

			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProcedureTableName, "ZZ6_IntoTemporaryProcedure"));
			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProcedureTableName, "ZZ6_OutOfTemporaryProcedure"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProcedureTableName, "ZZ6_IsTransit", "CHAR(1) NOT NULL CONSTRAINT[DF_RefCusProcedure_ZZ6_IsTransit] DEFAULT('N')"));

			return sb.ToString();
		}

		#endregion

		#region Version 201 Upgrade Script
		static string GetVersion201Script()
		{
			var refCusProcedure = new RefCusProcedure();
			var refCusProcedureTableName = refCusProcedure.TableName;
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_IntoWarehouse", "((ZZ6_IntoWarehouse='Y' AND ZZ6_IntoOutwardProcessing<>'Y' AND ZZ6_IntoInwardProcessing<>'Y' AND (ZZ6_IntoTemporaryImport<>'Y' AND ZZ6_IntoTemporaryExport<>'Y')) OR (ZZ6_IntoWarehouse='N' OR ZZ6_IntoWarehouse='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_OutOfWarehouse", "((ZZ6_OutOfWarehouse='Y' AND ZZ6_OutofOutwardProcessing<>'Y' AND (ZZ6_OutOfTemporaryImport<>'Y' AND ZZ6_OutOfTemporaryExport<>'Y') AND ZZ6_OutOfInwardProcessing<>'Y') OR (ZZ6_OutOfWarehouse='N' OR ZZ6_OutOfWarehouse='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_IntoTemporaryImport", "((ZZ6_IntoTemporaryImport='Y' AND ZZ6_IntoTemporaryExport<>'Y' AND ZZ6_IntoWarehouse<>'Y' AND ZZ6_IntoInwardProcessing<>'Y' AND ZZ6_IntoOutwardProcessing<>'Y') OR (ZZ6_IntoTemporaryImport='N' OR ZZ6_IntoTemporaryImport='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_IntoTemporaryExport", "((ZZ6_IntoTemporaryExport='Y' AND ZZ6_IntoTemporaryImport<>'Y' AND ZZ6_IntoWarehouse<>'Y' AND ZZ6_IntoInwardProcessing<>'Y' AND ZZ6_IntoOutwardProcessing<>'Y') OR (ZZ6_IntoTemporaryExport='N' OR ZZ6_IntoTemporaryExport='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_OutOfTemporaryImport", "((ZZ6_OutOfTemporaryImport='Y' AND ZZ6_OutOfTemporaryExport<>'Y' AND ZZ6_OutOfWarehouse<>'Y' AND ZZ6_OutOfInwardProcessing<>'Y' AND ZZ6_OutofOutwardProcessing<>'Y') OR (ZZ6_OutOfTemporaryImport='N' OR ZZ6_OutOfTemporaryImport='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_OutOfTemporaryExport", "((ZZ6_OutOfTemporaryExport='Y' AND ZZ6_OutOfTemporaryImport<>'Y' AND ZZ6_OutOfWarehouse<>'Y' AND ZZ6_OutOfInwardProcessing<>'Y' AND ZZ6_OutofOutwardProcessing<>'Y') OR (ZZ6_OutOfTemporaryExport='N' OR ZZ6_OutOfTemporaryExport='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_IntoInwardProcessing", "((ZZ6_IntoInwardProcessing='Y' AND ZZ6_IntoWarehouse<>'Y' AND (ZZ6_IntoTemporaryImport<>'Y' AND ZZ6_IntoTemporaryExport<>'Y') AND ZZ6_IntoOutwardProcessing<>'Y') OR (ZZ6_IntoInwardProcessing='N' OR ZZ6_IntoInwardProcessing='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_OutOfInwardProcessing", "((ZZ6_OutOfInwardProcessing='Y' AND ZZ6_OutOfWarehouse<>'Y' AND (ZZ6_OutOfTemporaryImport<>'Y' AND ZZ6_OutOfTemporaryExport<>'Y') AND ZZ6_OutofOutwardProcessing<>'Y') OR (ZZ6_OutOfInwardProcessing='N' OR ZZ6_OutOfInwardProcessing='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_IntoOutwardProcessing", "((ZZ6_IntoOutwardProcessing='Y' AND ZZ6_IntoWarehouse<>'Y' AND ZZ6_IntoInwardProcessing<>'Y' AND (ZZ6_IntoTemporaryImport<>'Y' AND ZZ6_IntoTemporaryExport<>'Y')) OR (ZZ6_IntoOutwardProcessing='N' OR ZZ6_IntoOutwardProcessing='I'))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refCusProcedureTableName,
				"CK_RefCusProcedure_ZZ6_OutofOutwardProcessing", "((ZZ6_OutofOutwardProcessing='Y' AND ZZ6_OutOfWarehouse<>'Y' AND (ZZ6_OutOfTemporaryImport<>'Y' AND ZZ6_OutOfTemporaryExport<>'Y') AND ZZ6_OutOfInwardProcessing<>'Y') OR (ZZ6_OutofOutwardProcessing='N' OR ZZ6_OutofOutwardProcessing='I'))"));

			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusProcedureTableName, "CK_RefCusProcedure_ZZ6_IsTransit", "(ZZ6_IsTransit IN ('Y', 'N', 'I'))"));
			return sb.ToString();
		}

		#endregion

		#region Version 202 Upgrade Script
		static string GetVersion202Script()
		{
			return GetCreateTableWithIndexSql(new UNDGSubstanceCFR());
		}

		#endregion

		#region Version 203 Upgrade Script

		static string GetVersion203Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine("ALTER TABLE UNDGSubstanceCFR ALTER COLUMN CFR_PrimaryClass VARCHAR(4) NOT NULL");
			return sb.ToString();
		}

		#endregion

		#region Version 204 Upgrade Script
		static string GetVersion204Script()
		{
			var refVesselZZ = new RefVesselZZ();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refVesselZZ.TableName, "IX_RefVesselZZ_ZZO_Code_ZZO_ZZZ_NKDataGrouping_ZZO_RadioCallSign", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefVesselZZ_ZZO_Code_ZZO_ZZZ_NKDataGrouping_ZZO_RadioCallSign ON RefVesselZZ(ZZO_Code ASC, ZZO_ZZZ_NKDataGrouping ASC, ZZO_RadioCallSign ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refVesselZZ.TableName, "IX_RefVesselZZ_ZZO_RadioCallSign_ZZO_ZZZ_NKDataGrouping", "CREATE NONCLUSTERED INDEX IX_RefVesselZZ_ZZO_RadioCallSign_ZZO_ZZZ_NKDataGrouping ON RefVesselZZ(ZZO_RadioCallSign ASC, ZZO_ZZZ_NKDataGrouping ASC)"));
			return sb.ToString();
		}

		#endregion

		#region Version 205 Upgrade Script
		static string GetVersion205Script()
		{
			var refCusVATApplicability = new RefCusVATApplicability();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusVATApplicability.TableName, "ZX5_VATCategory", "VARCHAR(4) NOT NULL CONSTRAINT [DF_RefCusVATApplicability_ZX5_VATCategory] DEFAULT ('')"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusVATApplicability.TableName, "CK_RefCusVATApplicability_ZX5_VATCategory", "ZX5_VATCategory = '' OR ZX5_VATCategory LIKE '[A-Z][0-9][0-9][0-9]'"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusVATApplicability.TableName, "CK_RefCusVATApplicability_ZX5_VATCategory_ZX5_ZZF_NKTaxOrFeeCode_ZX5_ZZZ_NKDataGrouping", "ZX5_VATCategory='' OR (ZX5_VATCategory != '' AND ZX5_ZZZ_NKDataGrouping = 'FR')"));
			return sb.ToString();
		}

		#endregion

		#region Version 210 Upgrade Script

		static string GetVersion210Script()
		{
			var tableName = "UNDGSubstanceCFR";
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(tableName, "CFR_LQMaxAmt", "DECIMAL (9,3) NOT NULL DEFAULT 0"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(tableName, "CFR_LQMaxAmtUQ", "VARCHAR(2) NOT NULL DEFAULT ''"));

			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript("UNDGSubstanceCFR", "CFR_IsPAXAirRailForbidden"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript("UNDGSubstanceCFR", "CFR_IsCargoAirRailForbidden"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", tableName, "DF_UNDGSubstanceCFR_CFR_IsPAXAirRailForbidden"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", tableName, "DF_UNDGSubstanceCFR_CFR_IsCargoAirRailForbidden"));

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(tableName, "CFR_PAXAirRailLimitType", "VARCHAR(3) NOT NULL DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(tableName, "CFR_CargoAirRailLimitType", "VARCHAR(3) NOT NULL DEFAULT ''"));

			return sb.ToString();
		}

		#endregion

		#region Version 211 Upgrade Script

		static string GetVersion211Script()
		{
			var tableName = "UNDGSubstanceCFR";
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(tableName, "CFR_IsPAXAirRailForbidden"));
			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(tableName, "CFR_IsCargoAirRailForbidden"));

			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(tableName, "CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType", "(CFR_PAXAirRailLimitType in ('FOB', 'NLM', ''))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(tableName, "CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType", "(CFR_CargoAirRailLimitType in ('FOB', 'NLM', ''))"));

			return sb.ToString();
		}

		#endregion

		#region Version 212 Upgrade Script

		static string GetVersion212Script()
		{
			var refCusTaxOrFee = new RefCusTaxOrFee();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate"));

			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusTaxOrFee.TableName, "ZZF_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusTaxOrFee.TableName, "ZZF_EndDate"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTaxOrFee.TableName, "CK_RefCusTaxOrFee_ZZF_StartDate_ZZF_EndDate"));

			sb.AppendLine("ALTER TABLE RefCusTaxOrFee ALTER COLUMN ZZF_StartDate DATETIME NOT NULL");
			sb.AppendLine("ALTER TABLE RefCusTaxOrFee ALTER COLUMN ZZF_EndDate DATETIME NOT NULL");

			return sb.ToString();
		}

		#endregion

		#region Version 213 Upgrade Script

		static string GetVersion213Script()
		{
			var refCusTaxOrFee = new RefCusTaxOrFee();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusTaxOrFee.TableName, "DF_RefCusTaxOrFee_ZZF_StartDate", "GetUtcDate()", "ZZF_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusTaxOrFee.TableName, "DF_RefCusTaxOrFee_ZZF_EndDate", "'2079-06-06 23:59'", "ZZF_EndDate"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTaxOrFee.TableName, "CK_RefCusTaxOrFee_ZZF_StartDate_ZZF_EndDate", "(ZZF_StartDate<=ZZF_EndDate)"));

			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_StartDate ON RefCusTaxOrFee(ZZF_ZZZ_NKDataGrouping ASC, ZZF_Code ASC, ZZF_StartDate)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTaxOrFee.TableName, "IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTaxOrFee_ZZF_ZZZ_NKDataGrouping_ZZF_Code_ZZF_EndDate ON RefCusTaxOrFee (ZZF_ZZZ_NKDataGrouping ASC, ZZF_Code ASC, ZZF_EndDate ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 214 Upgrade Script

		static string GetVersion214Script()
		{
			return GetCreateTableWithIndexSql(new RefAirlineUNDGRule());
		}

		#endregion

		#region Version 215 Upgrade Script

		static string GetVersion215Script()
		{
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("TR", "TG_RefCusCodeListAttributeName_INS_UPD", "TRIGGER"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("TR", "TG_RefCusCodeList_INS_UPD", "TRIGGER"));

			return sb.ToString();
		}

		#endregion

		#region Version 216 Upgrade Script

		static string GetVersion216Script()
		{
			var sb = new StringBuilder();
			var refCusCodeType = new RefCusCodeType();

			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusCodeType.TableName, "DF_RefCusCodeType_ZZK_ZZZ_NKDataGrouping"));

			sb.AppendLine(@"
DELETE FROM RefCusCodeTypeLanguage WHERE ZXI_ZZK_CodeType IN (SELECT ZZK_PK FROM RefCusCodeType WHERE ZZK_ZZZ_NKDataGrouping = '');
DELETE FROM RefCusCodeType WHERE ZZK_ZZZ_NKDataGrouping = '';");

			return sb.ToString();
		}

		#endregion

		#region Version 217 Upgrade Script

		static string GetVersion217Script()
		{
			var refCusCodeType = new RefCusCodeType();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusCodeType.TableName, "CK_RefCusCodeType_ZZK_ZZZ_NKDataGrouping", "(ZZK_ZZZ_NKDataGrouping <> '')"));

			return sb.ToString();
		}

		#endregion

		#region Version 218 Upgrade Script

		static string GetVersion218Script()
		{
			return new Trigger("TG_RefCusCodeList_INS_UPD").GetCreateSqlScriptByVersion();
		}

		#endregion

		#region Version 219 Upgrade Script

		static string GetVersion219Script()
		{
			return new Trigger("TG_RefCusCodeListAttributeName_INS_UPD").GetCreateSqlScriptByVersion();
		}

		#endregion

		#region Version 300 Upgrade Script
		static string GetVersion300Script()
		{
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("IF", "GetRatesBySingleCriteriaSet", "FUNCTION"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("P", "GetApplicableConditions", "PROCEDURE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("P", "GetApplicableConditionsBySingleAdditionalCode", "PROCEDURE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("P", "GetApplicableRates", "PROCEDURE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("P", "GetApplicableRatesWithoutDataGrouping", "PROCEDURE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RateView", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffAdditionalCodeView", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffAttributeView", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffRelationshipView", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffUOMView", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffView", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "VATApplicabilityView", "VIEW"));
			return sb.ToString();
		}

		#endregion

		#region Version 301 Upgrade Script
		static string GetVersion301Script()
		{
			return new View("TariffView").GetCreateSqlScriptByVersion(1);
		}

		#endregion

		#region Version 302 Upgrade Script
		static string GetVersion302Script()
		{
			return new View("RateView").GetCreateSqlScriptByVersion(1);
		}

		#endregion

		#region Version 303 Upgrade Script
		static string GetVersion303Script()
		{
			return new View("TariffAttributeView").GetCreateSqlScriptByVersion(1);
		}

		#endregion

		#region Version 304 Upgrade Script
		static string GetVersion304Script()
		{
			return new View("TariffUOMView").GetCreateSqlScriptByVersion(1);
		}

		#endregion

		#region Version 305 Upgrade Script
		static string GetVersion305Script()
		{
			return new View("TariffRelationshipView").GetCreateSqlScriptByVersion(1);
		}

		#endregion

		#region Version 306 Upgrade Script
		static string GetVersion306Script()
		{
			return new View("VATApplicabilityView").GetCreateSqlScriptByVersion(1);
		}

		#endregion

		#region Version 307 Upgrade Script
		static string GetVersion307Script()
		{
			return new View("TariffAdditionalCodeView").GetCreateSqlScriptByVersion(1);
		}

		#endregion

		#region Version 308 Upgrade Script
		static string GetVersion308Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 309 Upgrade Script
		static string GetVersion309Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 310 Upgrade Script
		static string GetVersion310Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 311 Upgrade Script
		static string GetVersion311Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 312 Upgrade Script
		static string GetVersion312Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 313 Upgrade Script
		static string GetVersion313Script()
		{
			var type = typeof(ITableScript);
			var types = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name == "CargoWise.RefDbRepo.RemoteDbManager").SelectMany(x => x.GetTypes())
				.Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).Except(TableExclusionListForVersion313Upgrader)
				.ToList();
			var sb = new StringBuilder();
			var createTableViewScript = "";
			foreach (var table in types)
			{
				var dict = ((ITableScript)Activator.CreateInstance(table)).TableViewScriptDictionary;
				if (dict.TryGetValue(1, out createTableViewScript))
				{
					sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(createTableViewScript, "V", FormattableString.Invariant($"{table.Name}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
				}
			}
			return sb.ToString();
		}

		#endregion

		#region Version 314 Upgrade Script
		static string GetVersion314Script()
		{
			var refCusConditionLanguage = new RefCusConditionLanguage();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefCusConditionLanguage()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionLanguage.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusConditionLanguage.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(new RefLanguageType().TableName, "Constraint_ZX6_Language", "(LEN(ZX6_Language)=2 OR LEN(ZX6_Language)=3)"));
			return sb.ToString();
		}

		#endregion

		#region Version 315 Upgrade Script
		static string GetVersion315Script()
		{
			var refSysConfig = new RefSysConfig();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refSysConfig.TableName, "IX_RefSysConfig_ZRC_ZRT_ConfigCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refSysConfig.TableName, "FK_RefSysConfig_RefSysConfigType"));

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refSysConfig.TableName, "ZRC_ZRT_NKConfigCode", "VARCHAR(10) NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refSysConfig.TableName, "FK_RefSysConfig_RefSysConfigType", "ZRC_ZRT_NKConfigCode", "RefSysConfigType (ZRT_ConfigCode)"));

			sb.AppendLine(@"IF EXISTS(
SELECT * FROM sys.tables tab
INNER JOIN sys.columns col ON tab.object_id = col.object_id
WHERE tab.name = 'RefSysConfig' AND col.name = 'ZRC_ZRT_ConfigCode'
)
EXEC dbo.sp_executesql @statement = N'UPDATE RefSysConfig
SET ZRC_ZRT_NKConfigCode = ZRT_ConfigCode
FROM RefSysConfig
INNER JOIN RefSysConfigType ON ZRC_ZRT_ConfigCode = ZRT_PK'");

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefSysConfigTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refSysConfig.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refSysConfig.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refSysConfig.TableName, "ZRC_ZRT_ConfigCode"));
			sb.AppendLine("ALTER TABLE RefSysConfig ALTER COLUMN ZRC_ZRT_NKConfigCode VARCHAR(10) NOT NULL");
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refSysConfig.TableName, "IX_RefSysConfig_ZRC_ZRT_NKConfigCode", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefSysConfig_ZRC_ZRT_NKConfigCode ON RefSysConfig (ZRC_ZRT_NKConfigCode ASC)"));
			return sb.ToString();
		}

		#endregion

		#region Version 316 Upgrade Script
		static string GetVersion316Script()
		{
			var refSysConfig = new RefSysConfig();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refSysConfig.TableName, "Constraint_ZRC_SingleFieldValue"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refSysConfig.TableName, "ZRC_BinaryValue", "VARBINARY(MAX) NULL"));
			return sb.ToString();
		}

		#endregion

		#region Version 317 Upgrade Script
		static string GetVersion317Script()
		{
			var refSysConfig = new RefSysConfig();
			var sb = new StringBuilder();
			var constraint = @"([ZRC_StringValue]<>'' AND [ZRC_BitValue]=(0) AND [ZRC_DecimalValue]=(0) AND [ZRC_BinaryValue] IS NULL OR [ZRC_BitValue]<>(0) AND [ZRC_StringValue]='' AND [ZRC_DecimalValue]=(0) AND [ZRC_BinaryValue] IS NULL OR [ZRC_DecimalValue]<>(0) AND [ZRC_StringValue]='' AND [ZRC_BitValue]=(0) AND [ZRC_BinaryValue] IS NULL OR [ZRC_BinaryValue] IS NOT NULL AND [ZRC_StringValue]='' AND [ZRC_BitValue]=(0) AND [ZRC_DecimalValue]=(0))";
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refSysConfig.TableName, "Constraint_ZRC_SingleFieldValue", constraint));
			return sb.ToString();
		}

		#endregion

		#region Version 318 Upgrade Script
		static string GetVersion318Script()
		{
			var refAirlineCommodityCode = new RefAirlineCommodityCode();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefAirlineCommodityCode()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refAirlineCommodityCode.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refAirlineCommodityCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 319 Upgrade Script
		static string GetVersion319Script()
		{
			var refStlScript = new RefStlScript();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefStlScript()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 320 Upgrade Script
		static string GetVersion320Script()
		{
			var uNDGAttributeZZ = new UNDGAttributeZZ();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGAttributeZZTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(uNDGAttributeZZ.TableName, "DAZ_IsSystem"));
			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(uNDGAttributeZZ.TableName, "DAZ_IsSystem"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(uNDGAttributeZZ.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{uNDGAttributeZZ.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 321 Upgrade Script
		static string GetVersion321Script()
		{
			var refCusTariffTypeLanguage = new RefCusTariffTypeLanguage();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefCusTariffTypeLanguage()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffTypeLanguage.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusTariffTypeLanguage.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 322 Upgrade Script
		static string GetVersion322Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine("BEGIN");
			sb.AppendLine(@"declare @scripts nvarchar(max);
set @scripts=
(select RenameScript + char(10) from (
select CONCAT(
		'exec sp_rename ',
		'@objname = ''['+d.name+']''',
		', @newname = ''DF_',t.name,'_',c.name,   '''',
		', @objtype = ''OBJECT'' ;'
	) RenameScript
from sys.tables t
join sys.default_constraints d on d.parent_object_id = t.object_id
join sys.columns c on c.object_id = t.object_id
and c.column_id = d.parent_column_id
where d.name like 'DF[_][_]%' and d.type='D'
) A for xml path('')
);

EXEC sp_executesql @scripts
");
			sb.AppendLine("END");
			return sb.ToString();
		}

		#endregion

		#region Version 323 Upgrade Script
		static string GetVersion323Script()
		{
			var sb = new StringBuilder();
			AddTableAndTableView(sb, new RefCusTariffBRCharacteristic());
			AddTableAndTableView(sb, new RefCusTariffBRCharacteristicValue());
			AddTableAndTableView(sb, new RefCusTariffBRCharacteristicAttribute());

			return sb.ToString();
		}

		#endregion

		#region Version 324 Upgrade Script
		static string GetVersion324Script()
		{
			var refDocOrgCusCode = new RefDocOrgCusCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(
				refDocOrgCusCode.TableName,
				"CK_RefDocOrgCusCode_DOC_DocumentType"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refDocOrgCusCode.TableName,
				"CK_RefDocOrgCusCode_DOC_DocumentType",
				"([DOC_DocumentType]='HAW' OR [DOC_DocumentType]='AWB' OR [DOC_DocumentType]='ESI' OR [DOC_DocumentType]='HBL')"));
			return sb.ToString();
		}

		#endregion

		#region Version 325 Upgrade Script
		static string GetVersion325Script()
		{
			var refCusConfiguration = new RefCusConfiguration();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefCusConfiguration()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConfiguration.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusConfiguration.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 326 Upgrade Script
		static string GetVersion326Script()
		{
			var refCusApplicability = new RefCusApplicability();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZX1_Conditions_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZZ2_Rate"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_AdditionalCode_ZZT_OrderNumber_ZZT_ZX1_Conditions"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusApplicability.TableName, "ZZT_ZZA_SecondTradeGroup", "UNIQUEIDENTIFIER NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup ON RefCusApplicability(ZZT_ZX1_Conditions, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZ2_Rate, ZZT_ZZA_SecondTradeGroup) INCLUDE(ZZT_EndDate)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup ON RefCusApplicability(ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZX1_Conditions, ZZT_ZZA_SecondTradeGroup)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_ZZA_SecondTradeGroup",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_ZZA_SecondTradeGroup ON RefCusApplicability(ZZT_ZY2_AdditionalCode, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_ZZA_SecondTradeGroup) WHERE ZZT_ZY2_AdditionalCode IS NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusApplicabilityTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusApplicabilityTableView_V2", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusApplicability.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusApplicability.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusApplicability.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusApplicability.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusApplicability.TableName, "FK_RefCusApplicability_RefCusTradeGroup2", "ZZT_ZZA_SecondTradeGroup", "RefCusTradeGroup(ZZA_PK)"));
			return sb.ToString();
		}

		#endregion

		#region Version 327 Upgrade Script
		static string GetVersion327Script()
		{
			var refCusApplicability = new RefCusApplicability();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZA_SecondTradeGroup", "CREATE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZZA_SecondTradeGroup ON RefCusApplicability(ZZT_ZZA_SecondTradeGroup) WHERE ZZT_ZZA_SecondTradeGroup IS NOT NULL "));

			return sb.ToString();
		}

		#endregion

		#region Version 328 Upgrade Script
		static string GetVersion328Script()
		{
			var refStlScript = new RefStlScript();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refStlScript.TableName, "STL_DateType", "CHAR(3) NOT NULL CONSTRAINT [DF_RefStlScript_STL_DateType] DEFAULT 'DTE'"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 329 Upgrade Script
		static string GetVersion329Script()
		{
			var refStlScript = new RefStlScript();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refStlScript.TableName, "CK_RefStlScript_STL_DateType", "[STL_DateType]='SDT' OR [STL_DateType]='DTE' OR [STL_DateType]='DTO'"));

			return sb.ToString();
		}

		#endregion

		#region Version 330 Upgrade Script
		static string GetVersion330Script()
		{
			var sb = new StringBuilder();
			var refSysConfig = new RefSysConfig();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefSysConfigTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refSysConfig.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refSysConfig.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 331 Upgrade Script
		static string GetVersion331Script()
		{
			var sb = new StringBuilder();
			var refStlScript = new RefStlScript();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V2", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity", "[STL_DataGranularity]='TRN' OR [STL_DataGranularity]='MAH' OR [STL_DataGranularity]='MCO' OR [STL_DataGranularity]='DAY'"));
			return sb.ToString();
		}

		#endregion

		#region Version 332 Upgrade Script
		static string GetVersion332Script()
		{
			var refExchange = new RefExchangeRateZZ();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(
				refExchange.TableName,
				"CK_RefExchangeRateZZ_ZZN_ExRateType"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refExchange.TableName,
				"CK_RefExchangeRateZZ_ZZN_ExRateType",
				"([ZZN_ExRateType]='CUE' OR [ZZN_ExRateType]='CUS' OR [ZZN_ExRateType]='CUD' OR [ZZN_ExRateType]='IAT')"));
			return sb.ToString();
		}

		#endregion

		#region Version 333 Upgrade Script
		static string GetVersion333Script()
		{
			var refStlScript = new RefStlScript();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version", "CREATE UNIQUE NONCLUSTERED INDEX [IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version] ON [RefStlScript] ([STL_FeatureCode], [STL_ActiveOn], [STL_MinCW1Version], [STL_MaxCW1Version])"));

			return sb.ToString();
		}

		#endregion

		#region Version 334 Upgrade Script
		static string GetVersion334Script()
		{
			return "SELECT 1 FROM RefStlScript WHERE 1 = 0";
		}

		#endregion

		#region Version 335 Upgrade Script
		static string GetVersion335Script()
		{
			var language = new RefCusConditionLanguage();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(language.TableName, "FK_RefCusConditionLanguage_RefCusCondition", "ZXJ_ZX1_Condition", "RefCusCondition (ZX1_PK)"));
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(
				language.TableName,
				"Constraint_ZXJ_ZX6_NKLanguageNotEmpty"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				language.TableName,
				"CK_ZXJ_ZX6_NKLanguageNotEmpty",
				"([ZXJ_ZX6_NKLanguage] <> '')"));
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(
				language.TableName,
				"Constraint_ZXJ_Comment_OR_ZXJ_SourceNotEmpty"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				language.TableName,
				"CK_ZXJ_Comment_OR_ZXJ_SourceNotEmpty",
				"([ZXJ_Comment] <> '' OR [ZXJ_Source] <> '')"));
			return sb.ToString();
		}

		#endregion

		#region Version 336 Upgrade Script
		static string GetVersion336Script()
		{
			var refAirlineProductCode = new RefAirlineProductCode();
			var refAirlineProductCodeCommodityCodePivot = new RefAirlineProductCodeCommodityCodePivot();

			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefAirlineProductCode()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refAirlineProductCode.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refAirlineProductCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(GetCreateTableWithIndexSql(new RefAirlineProductCodeCommodityCodePivot()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refAirlineProductCodeCommodityCodePivot.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refAirlineProductCodeCommodityCodePivot.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 337 Upgrade Script
		static string GetVersion337Script()
		{
			var refAirlineProductCode = new RefAirlineProductCode();

			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refAirlineProductCode.TableName, "CK_RefAirlineProductCodeg_RAR_CodeNotEmpty"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refAirlineProductCode.TableName, "CK_RefAirlineProductCode_RAR_CodeNotEmpty", "[RAR_Code] <> ''"));

			return sb.ToString();
		}

		#endregion

		#region Version 338 Upgrade Script
		static string GetVersion338Script()
		{
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "VATApplicabilityView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA"));

			sb.AppendLine("ALTER TABLE RefCusTariffNationalCode ALTER COLUMN ZZW_ZZF_NKTaxOrFeeCode VARCHAR(4) NOT NULL");
			sb.AppendLine("ALTER TABLE RefCusVATApplicability ALTER COLUMN ZX5_ZZF_NKTaxOrFeeCode VARCHAR(4) NOT NULL");
			sb.AppendLine("ALTER TABLE RefCusTariff ALTER COLUMN ZZ1_ZZF_NKTaxOrFeeCode VARCHAR(4) NOT NULL");

			sb.AppendLine(new View("TariffView").GetCreateSqlScriptByVersion(1));
			sb.AppendLine(new View("VATApplicabilityView").GetCreateSqlScriptByVersion(1));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode",
				"CREATE UNIQUE INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode ON  RefCusTariffNationalCode (ZZW_ZZZ_NKDataGrouping, ZZW_ZZ1_Tariff, ZZW_ZZF_NKTaxOrFeeCode, ZZW_NationalCode)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate",
				"CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate ON RefCusVATApplicability(ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_StartDate ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 339 Upgrade Script
		static string GetVersion339Script()
		{
			var refCusQuota = new RefCusQuota();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefCusQuota()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusQuota.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusQuota.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 340 Upgrade Script
		static string GetVersion340Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefDataGrouping", "IX_RefDataGrouping_ZZZ_ZZZ_Grouping", "CREATE NONCLUSTERED INDEX IX_RefDataGrouping_ZZZ_ZZZ_Grouping ON RefDataGrouping (ZZZ_ZZZ_Grouping)"));
			return sb.ToString();
		}

		#endregion

		#region Version 341 Upgrade Script
		static string GetVersion341Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine("ALTER TABLE RefCusConditionValueTypeLanguage ALTER COLUMN ZXX_Description NVARCHAR(500) NOT NULL");
			return sb.ToString();
		}
		#endregion

		#region Version 342 Upgrade Script
		static string GetVersion342Script()
		{
			var refCusTariffNationalCode = new RefCusTariffNationalCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffNationalCode.TableName, "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));
			sb.AppendLine("ALTER TABLE RefCusTariffNationalCode ALTER COLUMN ZZW_NationalCode VARCHAR(10) NOT NULL");
			sb.AppendLine(new View("TariffView").GetCreateSqlScriptByVersion(1));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffNationalCode.TableName, "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode",
				"CREATE UNIQUE INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode ON  RefCusTariffNationalCode (ZZW_ZZZ_NKDataGrouping, ZZW_ZZ1_Tariff, ZZW_ZZF_NKTaxOrFeeCode, ZZW_NationalCode)"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusTariffNationalCode.TableName, "DF_RefCusTariffNationalCode_ZZW_Description", "''", "ZZW_Description"));

			return sb.ToString();
		}

		#endregion

		#region Version 343 Upgrade Script
		static string GetVersion343Script()
		{
			var sb = new StringBuilder();
			var refCusTariffUOM = new RefCusTariffUOM();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffUOM.TableName, "CK_RefCusTariffUOM_ZZ8_Type"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffUOM.TableName, "CK_RefCusTariffUOM_ZZ8_Type", "ZZ8_Type IN ('CU1','RU1','AD1','CU2','CU3','CU4','CU5')"));

			return sb.ToString();
		}

		#endregion

		#region Version 344 Upgrade Script
		static string GetVersion344Script()
		{
			var sb = new StringBuilder();
			var refCusApplicability = new RefCusApplicability();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_AdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_OrderNumber"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("IF", "GetRatesBySingleCriteriaSet_V1", "FUNCTION"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusApplicability.TableName, "CK_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_AdditionalCode_ZZT_OrderNumber"));

			sb.AppendLine("ALTER TABLE RefCusApplicability ALTER COLUMN ZZT_AdditionalCode NVARCHAR(15) NOT NULL");
			sb.AppendLine("ALTER TABLE RefCusApplicability ALTER COLUMN ZZT_OrderNumber NVARCHAR(15) NOT NULL");
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusApplicability.TableName, "DF_RefCusApplicability_ZZT_AdditionalCode", "''", "ZZT_AdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusApplicability.TableName, "DF_RefCusApplicability_ZZT_OrderNumber", "''", "ZZT_OrderNumber"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_AdditionalCode", "CREATE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_AdditionalCode ON RefCusApplicability(ZZT_AdditionalCode) WHERE ZZT_AdditionalCode <> ''"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup ON RefCusApplicability (ZZT_ZX1_Conditions, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZ2_Rate, ZZT_ZZA_SecondTradeGroup) INCLUDE ([ZZT_EndDate])"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup ON RefCusApplicability (ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZX1_Conditions, ZZT_ZZA_SecondTradeGroup)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_OrderNumber", "CREATE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_OrderNumber ON RefCusApplicability(ZZT_OrderNumber) WHERE ZZT_OrderNumber <> ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusApplicability.TableName, "CK_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_AdditionalCode_ZZT_OrderNumber", "(ZZT_ZY2_AdditionalCode is null or (ZZT_ZY2_AdditionalCode is not null and ZZT_AdditionalCode = '' and ZZT_OrderNumber = ''))"));
			return sb.ToString();
		}

		#endregion

		#region Version 345 Upgrade Script
		static string GetVersion345Script()
		{
			var sb = new StringBuilder();

			var refCusTariff = new RefCusTariff();
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusTariff.TableName, "DF_RefCusTariff_ZZ1_ZZF_NKTaxOrFeeCode", "''", "ZZ1_ZZF_NKTaxOrFeeCode"));

			var refCusRateType = new RefCusRateType();
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusRateType.TableName, "DF_RefCusRateType_ZZR_CustomsValueFormula", "''", "ZZR_CustomsValueFormula"));

			var refCusTradeGroupLanguage = new RefCusTradeGroupLanguage();
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusTradeGroupLanguage.TableName, "DF_RefCusTradeGroupLanguage_ZXD_Description", "''", "ZXD_Description"));

			var refCusTariffAdditionalCodeLanguage = new RefCusTariffAdditionalCodeLanguage();
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusTariffAdditionalCodeLanguage.TableName, "DF_RefCusTariffAdditionalCodeLanguage_ZY4_Description", "''", "ZY4_Description"));

			var refCusTariffAdditionalCodeCategory = new RefCusTariffAdditionalCodeCategory();
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusTariffAdditionalCodeCategory.TableName, "DF_RefCusTariffAdditionalCodeCategory_ZY3_Category", "''", "ZY3_Category"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusTariffAdditionalCodeCategory.TableName, "DF_RefCusTariffAdditionalCodeCategory_ZY3_Description", "''", "ZY3_Description"));

			var refCusRateCodeLanguage = new RefCusRateCodeLanguage();
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusRateCodeLanguage.TableName, "DF_RefCusRateCodeLanguage_ZXC_Description", "''", "ZXC_Description"));

			return sb.ToString();
		}

		#endregion

		#region Version 346 Upgrade Script
		static string GetVersion346Script()
		{
			var refCarrierCodeLanguage = new RefCarrierCodeLanguage();

			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefCarrierCodeLanguage()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCarrierCodeLanguage.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCarrierCodeLanguage.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 347 Upgrade Script

		static string GetVersion347Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 348 Upgrade Script

		static string GetVersion348Script()
		{
			var sb = new StringBuilder();
			var refCusTariff = new RefCusTariff();
			var refCusTariffNationalCode = new RefCusTariffNationalCode();
			var refCusVATApplicability = new RefCusVATApplicability();
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariff.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusTariff.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffNationalCode.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusTariffNationalCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusVATApplicability.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusVATApplicability.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 349 Upgrade Script

		static string GetVersion349Script()  //all default value removed in this method was added in upgrade 347, which was set to do nothing already
		{
			var sb = new StringBuilder();

			foreach (var dv in GetColumnsToAddDefaultValue())
			{
				foreach (KeyValuePair<string, string> column in dv.Columns)
				{
					sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints",
						dv.TableScript.TableName, $"DF_{dv.TableScript.TableName}_{column.Key}"));
				}
			}

			return sb.ToString();
		}

#if DEBUG
		public
#endif
		static (ITableScript TableScript, Dictionary<string, string> Columns)[] GetColumnsToAddDefaultValue()
		{
			return new (ITableScript, Dictionary<string, string>)[]
			{
				(new RefAccTaxRate(), new Dictionary<string, string> { { "ZAT_ReferenceRateType", "''" }, { "ZAT_RN_NKCountry", "''" } }),
				(new RefAirlineUNDGRule(), new Dictionary<string, string> { { "RMD_StartDate", "'1 January 1'" } }),
				(new RefCarrierCode(), new Dictionary<string, string> { { "ZZ4_Code", "''" }, { "ZZ4_Description", "''" }, { "ZZ4_ZZZ_NKDataGrouping", "''" } }),
				(new RefCarrierCodeAttribute(), new Dictionary<string, string> { { "ZZG_Name", "''" }, { "ZZG_Value", "''" } }),
				(new RefCusCodeList(), new Dictionary<string, string> { { "ZZD_Code", "''" }, { "ZZD_Description", "''" }, { "ZZD_ZZK_NKCodeType", "''" }, { "ZZD_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusCodeListAttribute(), new Dictionary<string, string> { { "ZZE_Value", "''" }, { "ZZE_ZXE_NKName", "''" } }),
				(new RefCusCodeListAttributeName(), new Dictionary<string, string> { { "ZXE_Description", "''" }, { "ZXE_Name", "''" }, { "ZXE_ZZK_NKCodeType", "''" }, { "ZXE_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusCodeListAttributeNameLanguage(), new Dictionary<string, string> { { "ZXH_Description", "''" }, { "ZXH_ZX6_NKLanguage", "''" } }),
				(new RefCusCodeListLanguage(), new Dictionary<string, string> { { "ZXA_ZX6_NKLanguage", "''" } }),
				(new RefCusCodeOrAttributeTransportMode(), new Dictionary<string, string> { { "ZZU_TransportMode", "''" } }),
				(new RefCusCodeType(), new Dictionary<string, string> { { "ZZK_CodeType", "''" }, { "ZZK_Description", "''" }, { "ZZK_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusCodeTypeLanguage(), new Dictionary<string, string> { { "ZXI_Description", "''" }, { "ZXI_ZX6_NKLanguage", "''" } }),
				(new RefCusCondition(), new Dictionary<string, string> { { "ZX1_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusConditionType(), new Dictionary<string, string> { { "ZX2_ConditionClass", "''" }, { "ZX2_ConditionType", "''" }, { "ZX2_Description", "''" }, { "ZX2_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusConditionValue(), new Dictionary<string, string> { { "ZX3_Value", "''" } }),
				(new RefCusConditionValueType(), new Dictionary<string, string> { { "ZX4_Description", "''" }, { "ZX4_ValueType", "''" }, { "ZX4_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusConfiguration(), new Dictionary<string, string> { { "ZZJ_RN_NKCustomsCountry", "''" }, { "ZZJ_TariffDataSource", "''" }, { "ZZJ_ZZZ_NKDefaultDataGrouping", "''" } }),
				(new RefCusMap(), new Dictionary<string, string> { { "ZZM_CustomsValue", "''" }, { "ZZM_CW1orCommercialValue", "''" }, { "ZZM_ZZP_NKMapType", "''" }, { "ZZM_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusMapType(), new Dictionary<string, string> { { "ZZP_Description", "''" }, { "ZZP_MapType", "''" } }),
				(new RefCusNomenclatureGroup(), new Dictionary<string, string> { { "ZZ5_CompositeKey", "''" }, { "ZZ5_Description", "''" }, { "ZZ5_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusNomenclatureGroupNote(), new Dictionary<string, string> { { "ZZL_Note", "''" }, { "ZZL_NoteType", "''" }, { "ZZL_ZX6_NKLanguage", "''" }, { "ZZL_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusNomenclatureGroupType(), new Dictionary<string, string> { { "ZZ9_Description", "''" }, { "ZZ9_GroupType", "''" } }),
				(new RefCusNomenclatureLanguage(), new Dictionary<string, string> { { "ZX8_ZX6_NKLanguage", "''" } }),
				(new RefCusPreference(), new Dictionary<string, string> { { "ZZS_Description", "''" }, { "ZZS_Preference", "''" }, { "ZZS_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusPreferenceLanguage(), new Dictionary<string, string> { { "ZX9_ZX6_NKLanguage", "''" } }),
				(new RefCusProcedure(), new Dictionary<string, string> { { "ZZ6_Description", "''" }, { "ZZ6_ProcedureCode", "''" }, { "ZZ6_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusRate(), new Dictionary<string, string> { { "ZZ2_RateFormula", "''" } }),
				(new RefCusRateCode(), new Dictionary<string, string> { { "ZY1_Description", "''" }, { "ZY1_RateCode", "''" } }),
				(new RefCusRateCodeLanguage(), new Dictionary<string, string> { { "ZXC_Description", "''" }, { "ZXC_ZX6_NKLanguage", "''" } }),
				(new RefCusRateType(), new Dictionary<string, string> { { "ZZR_CustomsValueFormula", "''" }, { "ZZR_Description", "''" }, { "ZZR_RateType", "''" }, { "ZZR_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusRateTypeLanguage(), new Dictionary<string, string> { { "ZXT_Description", "''" }, { "ZXT_ZX6_NKLanguage", "''" } }),
				(new RefCusRateUOM(), new Dictionary<string, string> { { "ZXG_UOM", "''" } }),
				(new RefCusRuling(), new Dictionary<string, string> { { "ZZX_Description", "''" }, { "ZZX_RN_NKCountryCode", "''" }, { "ZZX_RulingNumber", "''" }, { "ZZX_RulingType", "''" } }),
				(new RefCusRulingConfig(), new Dictionary<string, string> { { "ZZY_Category", "''" }, { "ZZY_Type", "''" } }),
				(new RefCusTariff(), new Dictionary<string, string> { { "ZZ1_Description", "''" }, { "ZZ1_TariffCode", "''" }, { "ZZ1_ZZF_NKTaxOrFeeCode", "''" }, { "ZZ1_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusTariffAdditionalCode(), new Dictionary<string, string> { { "ZY2_ZY3_NKCategory", "''" }, { "ZY2_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusTariffAdditionalCodeCategory(), new Dictionary<string, string> { { "ZY3_Category", "''" }, { "ZY3_Description", "''" }, { "ZY3_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusTariffAdditionalCodeLanguage(), new Dictionary<string, string> { { "ZY4_Description", "''" }, { "ZY4_ZX6_NKLanguage", "''" } }),
				(new RefCusTariffAttribute(), new Dictionary<string, string> { { "ZZ3_Name", "''" }, { "ZZ3_Value", "''" } }),
				(new RefCusTariffLanguage(), new Dictionary<string, string> { { "ZX7_ZX6_NKLanguage", "''" } }),
				(new RefCusTariffNationalCode(), new Dictionary<string, string> { { "ZZW_Description", "''" }, { "ZZW_ZZF_NKTaxOrFeeCode", "''" }, { "ZZW_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusTariffType(), new Dictionary<string, string> { { "ZZI_Description", "''" }, { "ZZI_TariffType", "''" }, { "ZZI_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusTariffTypeLanguage(), new Dictionary<string, string> { { "ZXK_Description", "''" }, { "ZXK_ZX6_NKLanguage", "''" } }),
				(new RefCusTariffUOM(), new Dictionary<string, string> { { "ZZ8_Type", "''" }, { "ZZ8_UOM", "''" } }),
				(new RefCusTaxOrFee(), new Dictionary<string, string> { { "ZZF_Code", "''" }, { "ZZF_Description", "''" }, { "ZZF_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusTaxOrFeeType(), new Dictionary<string, string> { { "ZX0_Description", "''" }, { "ZX0_TaxOrFeeType", "''" } }),
				(new RefCusTradeGroup(), new Dictionary<string, string> { { "ZZA_Description", "''" }, { "ZZA_TradeGroup", "''" }, { "ZZA_ZZZ_NKDataGrouping", "''" } }),
				(new RefCusTradeGroupCountry(), new Dictionary<string, string> { { "ZZB_RN_NKTradeGroupCountryCode", "''" } }),
				(new RefCusTradeGroupLanguage(), new Dictionary<string, string> { { "ZXD_Description", "''" }, { "ZXD_ZX6_NKLanguage", "''" } }),
				(new RefCusVATApplicability(), new Dictionary<string, string> { { "ZX5_ZZZ_NKDataGrouping", "''" } }),
				(new RefDataGrouping(), new Dictionary<string, string> { { "ZZZ_DataGrouping", "''" }, { "ZZZ_Description", "''" } }),
				(new RefDbVersionControl(), new Dictionary<string, string> { { "RVC_DataSet", "''" } }),
				(new RefExchangeRateZZ(), new Dictionary<string, string> { { "ZZN_ExRateType", "''" }, { "ZZN_Rate", "0" }, { "ZZN_RN_NKCountry", "''" }, { "ZZN_RX_NKExCurrency", "''" } }),
				(new RefHarbourRate(), new Dictionary<string, string> { { "ZXF_Mode", "''" }, { "ZXF_ZZZ_NKDataGrouping", "''" } }),
				(new RefSysConfig(), new Dictionary<string, string> { { "ZRC_ZRT_NKConfigCode", "''" } }),
				(new RefVesselZZ(), new Dictionary<string, string> { { "ZZO_Code", "''" }, { "ZZO_ZZZ_NKDataGrouping", "''" } })
			};
		}

		#endregion

		#region Version 350 Upgrade Script
		static string GetVersion350Script()
		{
			var refStlFieldMapping = new RefStlFieldMapping();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefStlFieldMapping()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlFieldMapping.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refStlFieldMapping.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 351 Upgrade Script

		static string GetVersion351Script()
		{
			var sb = new StringBuilder();
			var refCusTariffNationalCode = new RefCusTariffNationalCode();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", FormattableString.Invariant($"{refCusTariffNationalCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2"), "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffNationalCode.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusTariffNationalCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffNationalCode.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refCusTariffNationalCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			return sb.ToString();
		}

		#endregion

		#region Version 352 Upgrade Script
		static string GetVersion352Script()
		{
			var refStlFieldMapping = new RefStlFieldMapping();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefStlFieldMapping()));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refStlFieldMapping.TableName, "SFM_Reference5", "VARCHAR(255) NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refStlFieldMapping.TableName, "SFM_Category", "VARCHAR(255) NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refStlFieldMapping.TableName, "SFM_PriceItemCode", "VARCHAR(255) NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refStlFieldMapping.TableName, "SFM_ServiceOccuredUTC", "VARCHAR(255) NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refStlFieldMapping.TableName, "SFM_ClientStaffCode", "VARCHAR(255) NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlFieldMapping.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refStlFieldMapping.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			return sb.ToString();
		}

		#endregion

		#region Version 353 Upgrade Script
		static string GetVersion353Script()
		{
			var refCusRateCode = new RefCusRateCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusRateCode.TableName, "ZY1_ZZZ_NKDataGrouping", "VARCHAR(3) NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusRateCode.TableName, "FK_RefCusRateCode_RefDataGrouping", "ZY1_ZZZ_NKDataGrouping", "RefDataGrouping (ZZZ_DataGrouping)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusRateCode.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusRateCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 354 Upgrade Script
		static string GetVersion354Script()
		{
			var refCusConditionType = new RefCusConditionType();
			var refCusRateCode = new RefCusRateCode();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusRateCode.TableName, "IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode", "CREATE NONCLUSTERED INDEX IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode on RefCusRateCode(ZY1_ZZZ_NKDataGrouping, ZY1_RateCode) WHERE ZY1_ZZZ_NKDataGrouping <> ''"));

			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusConditionType.TableName, "CK_RefCusConditionType_ZX2_ConditionClass"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusConditionType.TableName, "CK_RefCusConditionType_ZX2_ConditionClass", "([ZX2_ConditionClass]='CLASS' OR [ZX2_ConditionClass]='RATE' OR [ZX2_ConditionClass]='CTRL' OR [ZX2_ConditionClass]='VAT' OR [ZX2_ConditionClass]='RISK')"));
			return sb.ToString();
		}

		#endregion

		#region Version 355 Upgrade Script
		static string GetVersion355Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 356 Upgrade Script
		static string GetVersion356Script()
		{
			var refExchangeRateZZ = new RefExchangeRateZZ();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refExchangeRateZZ.TableName,
				"IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refExchangeRateZZ.TableName,
				"IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate ON RefExchangeRateZZ(ZZN_RN_NKCountry ASC, ZZN_ExRateType ASC, ZZN_RX_NKExCurrency ASC, ZZN_StartDate DESC, ZZN_EndDate ASC) INCLUDE (ZZN_Rate)"));
			return sb.ToString();
		}

		#endregion

		#region Version 357 Upgrade Script
		static string GetVersion357Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 358 Upgrade Script

		static string GetVersion358Script()
		{
			var sb = new StringBuilder();
			var refCusConditionType = new RefCusConditionType();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", FormattableString.Invariant($"{refCusConditionType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"), "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionType.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusConditionType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionType.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusConditionType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			return sb.ToString();
		}

		#endregion

		#region Version 359 Upgrade Script
		static string GetVersion359Script()
		{
			var refExchangeRateZZ = new RefExchangeRateZZ();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refExchangeRateZZ.TableName,
				"IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refExchangeRateZZ.TableName,
				"IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate ON RefExchangeRateZZ(ZZN_RN_NKCountry ASC, ZZN_ExRateType ASC, ZZN_RX_NKExCurrency ASC, ZZN_StartDate ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refExchangeRateZZ.TableName,
				"IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate",
				"CREATE NONCLUSTERED INDEX IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate ON RefExchangeRateZZ(ZZN_RN_NKCountry ASC, ZZN_ExRateType ASC, ZZN_RX_NKExCurrency ASC, ZZN_StartDate DESC, ZZN_EndDate ASC) INCLUDE (ZZN_Rate)"));
			return sb.ToString();
		}

		#endregion

		#region Version 360 Upgrade Script
		static string GetVersion360Script()
		{
			var refAirlineCommodityCode = new RefAirlineCommodityCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refAirlineCommodityCode.TableName, "RAC_SpecialHandlingCodes", "nvarchar(MAX) NOT NULL CONSTRAINT [DF_RefAirlineCommodityCode_RAC_SpecialHandlingCodes] DEFAULT ('')"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refAirlineCommodityCode.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refAirlineCommodityCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 361 Upgrade Script
		static string GetVersion361Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine("BEGIN");
			sb.AppendLine(@"declare @scripts nvarchar(max);
set @scripts=
(select RenameScript + char(10) from (
select concat(
		'exec sp_rename ',
		'@objname = ''['+dc.name+']''',
		', @newname = ''DF_',t.name,'_',c.name,   '''',
		', @objtype = ''OBJECT'' ;'
	) RenameScript
from sys.default_constraints dc
join sys.tables t on dc.parent_object_id=t.object_id
join sys.columns c on dc.parent_column_id=c.column_id and c.object_id=t.object_id
where t.name not like '%History' and t.is_ms_shipped=0 and dc.name not like concat('DF[_]',t.name,'[_]',c.name)
) A for xml path('')
);

EXEC sp_executesql @scripts
");
			sb.AppendLine("END");
			return sb.ToString();
		}
		#endregion

		#region Version 362 Upgrade Script
		static string GetVersion362Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefUNLOCO()));

			return sb.ToString();
		}

		#endregion

		#region Version 363 Upgrade Script
		static string GetVersion363Script()
		{
			var refUNLOCOUtcOffset = new RefUNLOCOUtcOffset();

			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefUNLOCOUtcOffset()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refUNLOCOUtcOffset.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refUNLOCOUtcOffset.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 364 Upgrade Script
		static string GetVersion364Script()
		{
			var refCusTariffBRCharacteristic = new RefCusTariffBRCharacteristic();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "ZB1_ZZ5_Nomenclature", "UNIQUEIDENTIFIER NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "FK_RefCusTariffBRCharacteristic_RefCusNomenclatureGroup", "ZB1_ZZ5_Nomenclature", "RefCusNomenclatureGroup (ZZ5_PK)"));
			return sb.ToString();
		}

		#endregion

		#region Version 365 Upgrade Script
		static string GetVersion365Script()
		{
			var refCusTariffBRCharacteristic = new RefCusTariffBRCharacteristic();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ1_Tariff"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusTariffBRCharacteristic.TableName, "ZB1_ZZ1_Tariff", "UNIQUEIDENTIFIER NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_ZZ5_Nomenclature_ZB1_ZZ1_Tariff", "(ZB1_ZZ1_Tariff IS NULL AND ZB1_ZZ5_Nomenclature IS NOT NULL) OR (ZB1_ZZ1_Tariff IS NOT NULL AND ZB1_ZZ5_Nomenclature IS NULL)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ1_Tariff", "CREATE NONCLUSTERED INDEX IX_ZB1_ZZ1_Tariff ON RefCusTariffBRCharacteristic (ZB1_ZZ1_Tariff ASC) where ZB1_ZZ1_Tariff IS NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "IX_ZB1_ZZ5_Nomenclature", "CREATE NONCLUSTERED INDEX IX_ZB1_ZZ5_Nomenclature ON RefCusTariffBRCharacteristic (ZB1_ZZ5_Nomenclature ASC) where ZB1_ZZ5_Nomenclature IS NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffBRCharacteristic.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusTariffBRCharacteristic.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			return sb.ToString();
		}

		#endregion

		#region Version 366 Upgrade Script

		static string GetVersion366Script()
		{
			var refCusTariffAttributeName = new RefCusTariffAttributeName();

			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(new RefCusTariffAttributeName()));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffAttributeName.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusTariffAttributeName.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 367 Upgrade Script
		static string GetVersion367Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 368 Upgrade Script
		static string GetVersion368Script()
		{
			var refCusTariffBRCharacteristic = new RefCusTariffBRCharacteristic();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_Style"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_CharacteristicType", "(ZB1_CharacteristicType='NVE' OR ZB1_CharacteristicType='NCM' OR ZB1_CharacteristicType='NCMTE' OR ZB1_CharacteristicType='LPC' OR ZB1_CharacteristicType='LPCT')"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffBRCharacteristic.TableName, "CK_RefCusTariffBRCharacteristic_ZB1_Style", "(ZB1_Style='BOOLEAN' OR ZB1_Style='STRING' OR ZB1_Style='NUMBER' OR ZB1_Style='LIST' OR ZB1_Style='COMPOSED' OR ZB1_Style='DATE')"));
			return sb.ToString();
		}

		#endregion

		#region Version 369 Upgrade Script

		static string GetVersion369Script()
		{
			var refHarbourRate = new RefHarbourRate();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refHarbourRate.TableName, "ZXF_PortTaxType", "VARCHAR(3) NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_PortTaxType] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refHarbourRate.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refHarbourRate.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			return sb.ToString();
		}

		#endregion

		#region Version 370 Upgrade Script

		static string GetVersion370Script()
		{
			var refHarbourRate = new RefHarbourRate();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refHarbourRate.TableName, "CK_RefHarbourRate_ZXF_PortTaxType", "LEN(ZXF_PortTaxType)=0 OR LEN(ZXF_PortTaxType)=3"));
			return sb.ToString();
		}

		#endregion

		#region Version 371 Upgrade Script
		static string GetVersion371Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 372 Upgrade Script
		static string GetVersion372Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 373 Upgrade Script
		static string GetVersion373Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 374 Upgrade Script
		static string GetVersion374Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 375 Upgrade Script

		static string GetVersion375Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 376 Upgrade Script
		static string GetVersion376Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 377 Upgrade Script
		static string GetVersion377Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 378 Upgrade Script
		static string GetVersion378Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 379 Upgrade Script
		static string GetVersion379Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 380 Upgrade Script
		static string GetVersion380Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 381 Upgrade Script
		static string GetVersion381Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 382 Upgrade Script
		static string GetVersion382Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 383 Upgrade Script

		static string GetVersion383Script()
		{
			var refCusRateType = new RefCusRateType();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusRateType.TableName, "ZZR_IsExport", "BIT NOT NULL CONSTRAINT DF_RefCusRateType_ZZR_IsExport DEFAULT 0"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusRateType.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusRateType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 384 Upgrade Script

		static string GetVersion384Script()
		{
			var refUNLOCOUtcOffset = new RefUNLOCOUtcOffset();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refUNLOCOUtcOffset.TableName,
				"IX_RefUNLOCOUtcOffset_RLO_PK"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refUNLOCOUtcOffset.TableName,
				"IX_RefUNLOCOUtcOffset_RLO_RL_NKCode_RLO_StartTimeUtc",
				"CREATE UNIQUE NONCLUSTERED INDEX [IX_RefUNLOCOUtcOffset_RLO_RL_NKCode_RLO_StartTimeUtc] ON [RefUNLOCOUtcOffset] (RLO_RL_NKCode ASC,RLO_StartTimeUtc ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refUNLOCOUtcOffset.TableName, "CK_RefUNLOCOUtcOffset_RLO_EndTimeUtc_RLO_StartTimeUtc"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_EndTimeUtc", "([RLO_EndTimeUtc]>[RLO_StartTimeUtc])"));
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refUNLOCOUtcOffset.TableName, "CK_RefUNLOCOUtcOffset_RLO_RL_NKCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refUNLOCOUtcOffset.TableName, "RefUNLOCOUtcOffset_RLO_RL_NKCode", "(LEN([RLO_RL_NKCode])=(5))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refUNLOCOUtcOffset.TableName, "FK_RefUNLOCOUtcOffset_RefUNLOCO", "[RLO_RL_NKCode]", "RefUNLOCO ([RL_Code])"));
			return sb.ToString();
		}

		#endregion

		#region Version 385 Upgrade Script
		static string GetVersion385Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 386 Upgrade Script
		static string GetVersion386Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 387 Upgrade Script
		static string GetVersion387Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 388 Upgrade Script
		static string GetVersion388Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 389 Upgrade Script
		static string GetVersion389Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 390 Upgrade Script
		static string GetVersion390Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 391 Upgrade Script
		static string GetVersion391Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 392 Upgrade Script
		static string GetVersion392Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 393 Upgrade Script
		static string GetVersion393Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 394 Upgrade Script

		static string GetVersion394Script()
		{
			var sb = new StringBuilder();

			// Drop old indexes
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_StartDate_ZZA"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusVATApplicability", "IX_RefCusVATApplicability_ZX5_ZZ1_Tariff_ZX5_ZZW_TariffNationalCode_ZX5_ZZF_NKTaxOrFeeCode_ZX5_AdditionalCode_ZX5_EndDate_ZZA"));

			// Add new ones
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(
				"RefCusVATApplicability", "IX_RefCusVATApplicability_StartDateUnique",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusVATApplicability_StartDateUnique ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping, ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_StartDate, ZX5_ZZA_TradeGroup)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(
				"RefCusVATApplicability", "IX_RefCusVATApplicability_EndDateUnique",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusVATApplicability_EndDateUnique ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping, ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_EndDate, ZX5_ZZA_TradeGroup)"));

			return sb.ToString();
		}

		#endregion

		#region Version 395 Upgrade Script

		static string GetVersion395Script()
		{
			var refCusRateType = new RefCusRateType();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusRateType.TableName, "ZZR_RX_NKFormulaCurrency", "VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusRateType_ZZR_RX_NKFormulaCurrency DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusRateType.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refCusRateType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));

			return sb.ToString();
		}

		#endregion

		#region Version 396 Upgrade Script
		static string GetVersion396Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 397 Upgrade Script
		static string GetVersion397Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 398 Upgrade Script
		static string GetVersion398Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 399 Upgrade Script
		static string GetVersion399Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 400 Upgrade Script
		static string GetVersion400Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 401 Upgrade Script
		static string GetVersion401Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 402 Upgrade Script
		static string GetVersion402Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 403 Upgrade Script

		static string GetVersion403Script()
		{
			var sb = new StringBuilder();

			sb.AppendLine(new View("RefCusCodeListTransportModeView").GetCreateSqlScriptByVersion(1));
			sb.AppendLine(new View("RefCusCodeListAttributeTransportModeView").GetCreateSqlScriptByVersion(1));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(
				"RefCusCodeListTransportModeView_V1", "IX_RefCusCodeListTransportModeView_V1_ZZU_ZZD_CodeList",
				"CREATE UNIQUE CLUSTERED INDEX IX_RefCusCodeListTransportModeView_V1_ZZU_ZZD_CodeList on RefCusCodeListTransportModeView_V1 (ZZU_ZZD_CodeList)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(
				"RefCusCodeListAttributeTransportModeView_V1", "IX_RefCusCodeListAttributeTransportModeView_V1_ZZU_ZZE_Attribute",
				"CREATE UNIQUE CLUSTERED INDEX IX_RefCusCodeListAttributeTransportModeView_V1_ZZU_ZZE_Attribute on RefCusCodeListAttributeTransportModeView_V1 (ZZU_ZZE_Attribute)"));

			return sb.ToString();
		}

		#endregion

		#region Version 404 Upgrade Script

		static string GetVersion404Script()
		{
			var refCusProcedure = new RefCusProcedure();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProcedure.TableName, "ZZ6_IntoVATWarehouse", "CHAR(1) NOT NULL CONSTRAINT [DF_RefCusProcedure_ZZ6_IntoVATWarehouse] DEFAULT 'N'"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProcedure.TableName, "ZZ6_OutOfVATWarehouse", "CHAR(1) NOT NULL CONSTRAINT [DF_RefCusProcedure_ZZ6_OutOfVATWarehouse] DEFAULT 'N'"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProcedure.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusProcedure.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			return sb.ToString();
		}

		#endregion

		#region Version 405 Upgrade Script

		static string GetVersion405Script()
		{
			var refStlScript = new RefStlScript();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refStlScript.TableName, "STL_CollectionStartDateUtc", "DATETIME NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[4], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}4")));

			return sb.ToString();
		}

		#endregion

		#region Version 406 Upgrade Script
		static string GetVersion406Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 407 Upgrade Script
		static string GetVersion407Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 408 Upgrade Script
		static string GetVersion408Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 409 Upgrade Script
		static string GetVersion409Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 410 Upgrade Script
		static string GetVersion410Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 411 Upgrade Script
		static string GetVersion411Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 412 Upgrade Script

		static string GetVersion412Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 413 Upgrade Script

		static string GetVersion413Script()
		{
			return @"
ALTER TABLE RefCusApplicability SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusCondition SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusConditionLanguage SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusConditionValue SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusExcludedTradeGroup SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusRate SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusRateUOM SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariff SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffAdditionalCode SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffAdditionalCodeLanguage SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffAttribute SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffBRCharacteristic SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffBRCharacteristicAttribute SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffBRCharacteristicValue SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffLanguage SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffNationalCode SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffRelationship SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusTariffUOM SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE RefCusVATApplicability SET (LOCK_ESCALATION = DISABLE);
";
		}

		#endregion

		#region Version 414 Upgrade Script
		static string GetVersion414Script()
		{
			var sb = new StringBuilder();
			var refStlScript = new RefStlScript();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V2", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V3", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefStlScriptTableView_V4", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[4], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}4")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refStlScript.TableViewScriptDictionary[5], "V", FormattableString.Invariant($"{refStlScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}5")));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refStlScript.TableName, "CK_RefStlScript_STL_DataGranularity", "[STL_DataGranularity]='TRN' OR [STL_DataGranularity]='MAH' OR [STL_DataGranularity]='MCO' OR [STL_DataGranularity]='DAY' OR [STL_DataGranularity]='SPS'"));
			return sb.ToString();
		}

		#endregion

		#region Version 415 Upgrade Script

		static string GetVersion415Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 416 Upgrade Script

		static string GetVersion416Script()
		{
			var refCusConditionCode = new RefCusConditionCode();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(refCusConditionCode));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionCode.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusConditionCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 417 Upgrade Script

		static string GetVersion417Script()
		{
			var sb = new StringBuilder();
			var refCusConditionCodeLanguage = new RefCusConditionCodeLanguage();
			sb.AppendLine(GetCreateTableWithIndexSql(refCusConditionCodeLanguage));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionCodeLanguage.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusConditionCodeLanguage.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 418 Upgrade Script

		static string GetVersion418Script()
		{
			var sb = new StringBuilder();
			var refCusCondition = new RefCusCondition();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCondition.TableName, "ZX1_ZY7_NKConditionCode", "VARCHAR(3) NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCondition.TableName, "ZX1_AdditionalComment", "VARCHAR(4000) NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_AdditionalComment] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusCondition.TableName, "FK_RefCusCondition_RefCusConditionCode", "ZX1_ZY7_NKConditionCode", "RefCusConditionCode (ZY7_ConditionCode)"));

			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusCondition.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusCondition.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 419 Upgrade Script

		static string GetVersion419Script()
		{
			var sb = new StringBuilder();
			var refCusConditionLanguage = new RefCusConditionLanguage();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusConditionLanguage.TableName, "ZXJ_AdditionalComment", "VARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusConditionLanguage_ZXJ_AdditionalComment] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionLanguage.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusConditionLanguage.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 420 Upgrade Script

		static string GetVersion420Script()
		{
			var refMessagingBussPackageInfo = new RefMessagingBussPackageInfo();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(refMessagingBussPackageInfo));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refMessagingBussPackageInfo.TableViewScriptDictionary[1], "V",
				FormattableString.Invariant($"{refMessagingBussPackageInfo.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 421 Upgrade Script

		static string GetVersion421Script()
		{
			var refMessagingBussPackageVersion = new RefMessagingBussPackageVersion();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(refMessagingBussPackageVersion));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refMessagingBussPackageVersion.TableViewScriptDictionary[1], "V",
				FormattableString.Invariant($"{refMessagingBussPackageVersion.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 422 Upgrade Script

		static string GetVersion422Script()
		{
			var refMessagingBussCarrierInfo = new RefMessagingBussCarrierInfo();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(refMessagingBussCarrierInfo));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refMessagingBussCarrierInfo.TableViewScriptDictionary[1], "V",
				FormattableString.Invariant($"{refMessagingBussCarrierInfo.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 423 Upgrade Script

		static string GetVersion423Script()
		{
			var undgSubstanceCFR = new UNDGSubstanceCFR();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", undgSubstanceCFR.TableName, "CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(undgSubstanceCFR.TableName, "CK_UNDGSubstanceCFR_CFR_CargoAirRailLimitType",
				"([CFR_CargoAirRailLimitType]='' OR [CFR_CargoAirRailLimitType]='FOB' OR [CFR_CargoAirRailLimitType]='NLM' OR [CFR_CargoAirRailLimitType]='NLT' OR [CFR_CargoAirRailLimitType]='GLM')"));

			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", undgSubstanceCFR.TableName, "CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(undgSubstanceCFR.TableName, "CK_UNDGSubstanceCFR_CFR_PAXAirRailLimitType",
				"([CFR_PAXAirRailLimitType]='' OR [CFR_PAXAirRailLimitType]='FOB' OR [CFR_PAXAirRailLimitType]='NLM' OR [CFR_PAXAirRailLimitType]='NLT' OR [CFR_PAXAirRailLimitType]='GLM')"));


			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceCFRTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceCFR.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceCFR.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			return sb.ToString();
		}

		#endregion

		#region Version 424 Upgrade Script

		static string GetVersion424Script()
		{
			return SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(new View("TariffUOMView").GetCreateSqlScriptByVersion(2), "V", "TariffUOMView_V2");
		}

		#endregion

		#region Version 425 Upgrade Script
		static string GetVersion425Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 426 Upgrade Script
		static string GetVersion426Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 427 Upgrade Script

		static string GetVersion427Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 428 Upgrade Script

		static string GetVersion428Script()
		{
			var refClient = new RefClient();
			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(refClient));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refClient.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refClient.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}

		#endregion

		#region Version 429 Upgrade Script

		static string GetVersion429Script()
		{
			var refUNLOCORelatedPort = new RefUNLOCORelatedPort();

			var sb = new StringBuilder();
			sb.AppendLine(GetCreateTableWithIndexSql(refUNLOCORelatedPort));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refUNLOCORelatedPort.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refUNLOCORelatedPort.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 430 Upgrade Script
		static string GetVersion430Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 431 Upgrade Script
		static string GetVersion431Script()
		{
			var refClient = new RefClient();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refClient.TableName, "RCT_ClientID", "VARCHAR(255) NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refClient.TableName, "RCT_Certificate", "VARBINARY(MAX) NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refClient.TableName, "RCT_LegacyCertificate", "VARBINARY(MAX)"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refClient.TableName, "RCT_Signature", "VARBINARY(MAX) NOT NULL"));
			return sb.ToString();
		}

		#endregion

		#region Version 432 Upgrade Script

		static string GetVersion432Script()
		{
			var undgSubstanceCFR = new UNDGSubstanceCFR();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceCFR.TableName, "CFR_SecondaryPAXAirRailLimit", "DECIMAL(9,3) NOT NULL CONSTRAINT DF_UNDGSubstanceCFR_CFR_SecondaryPAXAirRailLimit DEFAULT 0"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceCFR.TableName, "CFR_SecondaryPAXAirRailLimitUnit", "VARCHAR(2) NOT NULL CONSTRAINT DF_UNDGSubstanceCFR_CFR_SecondaryPAXAirRailLimitUnit DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceCFR.TableName, "CFR_SecondaryCargoAirRailLimit", "DECIMAL(9,3) NOT NULL CONSTRAINT DF_UNDGSubstanceCFR_CFR_SecondaryCargoAirRailLimit DEFAULT 0"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceCFR.TableName, "CFR_SecondaryCargoAirRailLimitUnit", "VARCHAR(2) NOT NULL CONSTRAINT DF_UNDGSubstanceCFR_CFR_SecondaryCargoAirRailLimitUnit DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceCFR.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));

			return sb.ToString();
		}

		#endregion

		#region Version 433 Upgrade Script

		static string GetVersion433Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 434 Upgrade Script
		static string GetVersion434Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 435 Upgrade Script

		static string GetVersion435Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 436 Upgrade Script

		static string GetVersion436Script()
		{
			var tariffReferencedTables = new[] {
				( nameof(RefCusRate), "ZZ2" ),
				( nameof(RefCusCondition), "ZX1" ),
				( nameof(RefCusVATApplicability), "ZX5" ),
				( nameof(RefCusTariffAttribute), "ZZ3" ),
				( nameof(RefCusTariffRelationship), "ZZH" ),
				( nameof(RefCusTariffLanguage), "ZX7" ),
				( nameof(RefCusTariffUOM), "ZZ8" ),
				( nameof(RefCusTariffBRCharacteristic), "ZB1" ),
				( nameof(RefCusTariffNationalCode), "ZZW" ),
				( nameof(RefCusTariffAdditionalCode), "ZY2" )
			};
			var sb = new StringBuilder();
			Array.ForEach(tariffReferencedTables, x => sb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariff")));
			sb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusTariff), $"PK_{nameof(RefCusTariff)}"));
			sb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusTariff), $"PK_{nameof(RefCusTariff)}", "ZZ1_PK", "NONCLUSTERED"));
			Array.ForEach(tariffReferencedTables, x => sb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariff", $"{x.Item2}_ZZ1_Tariff", $"{nameof(RefCusTariff)}(ZZ1_PK)")));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariff), "IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_PK",
				"CREATE CLUSTERED INDEX IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_PK ON RefCusTariff (ZZ1_ZZZ_NKDataGrouping ASC, ZZ1_PK ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 437 Upgrade Script

		static string GetVersion437Script()
		{
			var rateReferencedTables = new[] {
				( nameof(RefCusApplicability), "ZZT" ),
				( nameof(RefCusRateUOM), "ZXG" )
			};
			var sb = new StringBuilder();
			Array.ForEach(rateReferencedTables, x => sb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusRate")));
			sb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusRate), $"PK_{nameof(RefCusRate)}"));
			sb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusRate), $"PK_{nameof(RefCusRate)}", "ZZ2_PK", "NONCLUSTERED"));
			Array.ForEach(rateReferencedTables, x => sb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusRate", $"{x.Item2}_ZZ2_Rate", $"{nameof(RefCusRate)}(ZZ2_PK)")));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_PK",
				"CREATE CLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_PK ON RefCusRate (ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_PK ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 438 Upgrade Script

		static string GetVersion438Script()
		{
			var conditionReferencedTables = new[] {
				( nameof(RefCusApplicability), "ZZT" ),
				( nameof(RefCusConditionValue), "ZX3" ),
				( nameof(RefCusConditionLanguage), "ZXJ" )
			};
			var sb = new StringBuilder();
			Array.ForEach(conditionReferencedTables, x => sb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusCondition")));
			sb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusCondition), $"PK_{nameof(RefCusCondition)}"));
			sb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusCondition), $"PK_{nameof(RefCusCondition)}", "ZX1_PK", "NONCLUSTERED"));
			Array.ForEach(conditionReferencedTables, x => sb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusCondition", $"{x.Item2}_ZX1_{(x.Item2 == "ZZT" ? "Conditions" : "Condition")}", $"{nameof(RefCusCondition)}(ZX1_PK)")));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_PK",
				"CREATE CLUSTERED INDEX IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_PK ON RefCusCondition (ZX1_ZZZ_NKDataGrouping ASC, ZX1_PK ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 439 Upgrade Script

		static string GetVersion439Script()
		{
			var additionalCodeReferencedTables = new[] {
				( nameof(RefCusApplicability), "ZZT" ),
				( nameof(RefCusTariffAdditionalCodeLanguage), "ZY4" )
			};
			var sb = new StringBuilder();
			Array.ForEach(additionalCodeReferencedTables, x => sb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariffAdditionalCode")));
			sb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(nameof(RefCusTariffAdditionalCode), "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
			sb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusTariffAdditionalCode), $"PK_{nameof(RefCusTariffAdditionalCode)}"));
			sb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusTariffAdditionalCode), $"PK_{nameof(RefCusTariffAdditionalCode)}", "ZY2_PK", "NONCLUSTERED"));
			Array.ForEach(additionalCodeReferencedTables, x => sb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariffAdditionalCode", $"{x.Item2}_ZY2_{(x.Item2 == "ZY4" ? "TariffAdditionalCode" : "AdditionalCode")}", $"{nameof(RefCusTariffAdditionalCode)}(ZY2_PK)")));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffAdditionalCode), "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_PK",
				"CREATE CLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_PK ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping ASC, ZY2_PK ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 440 Upgrade Script

		static string GetVersion440Script()
		{
			var nationalCodeReferencedTables = new[] {
				( nameof(RefCusRate), "ZZ2" ),
				( nameof(RefCusVATApplicability), "ZX5" ),
				( nameof(RefCusTariffAttribute), "ZZ3" ),
				( nameof(RefCusTariffUOM), "ZZ8" ),
				( nameof(RefCusTariffAdditionalCode), "ZY2" )
			};
			var sb = new StringBuilder();
			Array.ForEach(nationalCodeReferencedTables, x => sb.Append(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariffNationalCode")));
			sb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusTariffNationalCode), $"PK_{nameof(RefCusTariffNationalCode)}"));
			sb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusTariffNationalCode), $"PK_{nameof(RefCusTariffNationalCode)}", "ZZW_PK", "NONCLUSTERED"));
			Array.ForEach(nationalCodeReferencedTables, x => sb.Append(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(x.Item1, $"FK_{x.Item1}_RefCusTariffNationalCode", $"{x.Item2}_ZZW_{(x.Item2 == "ZY2" ? "NationalCode" : "TariffNationalCode")}", $"{nameof(RefCusTariffNationalCode)}(ZZW_PK)")));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffNationalCode), "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_PK",
				"CREATE CLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_PK ON RefCusTariffNationalCode (ZZW_ZZZ_NKDataGrouping ASC, ZZW_PK ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 441 Upgrade Script

		static string GetVersion441Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusTariffUOM), $"PK_{nameof(RefCusTariffUOM)}"));
			sb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusTariffUOM), $"PK_{nameof(RefCusTariffUOM)}", "ZZ8_PK", "NONCLUSTERED"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_PK",
				"CREATE CLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_PK ON RefCusTariffUOM (ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_PK ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 442 Upgrade Script

		static string GetVersion442Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(nameof(RefCusVATApplicability), $"PK_{nameof(RefCusVATApplicability)}"));
			sb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(nameof(RefCusVATApplicability), $"PK_{nameof(RefCusVATApplicability)}", "ZX5_PK", "NONCLUSTERED"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK",
				"CREATE CLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK ON RefCusVATApplicability (ZX5_ZZZ_NKDataGrouping ASC, ZX5_PK ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 443 Upgrade Script

		static string GetVersion443Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference",
				"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference ON RefCusRate(ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZ1_Tariff ASC, ZZ2_ZZW_TariffNationalCode , ZZ2_ZY1_RateCode ASC, ZZ2_StartDate, ZZ2_ZZS_Preference) INCLUDE (ZZ2_EndDate)"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode",
				"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode ON RefCusRate(ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZ1_Tariff ASC, ZZ2_ZZW_TariffNationalCode ASC)"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff",
				"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff ON RefCusRate(ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZW_TariffNationalCode ASC, ZZ2_ZZ1_Tariff ASC)"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference",
				"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference ON RefCusRate(ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZW_TariffNationalCode, ZZ2_ZY1_RateCode ASC, ZZ2_StartDate, ZZ2_ZZS_Preference) INCLUDE (ZZ2_EndDate)"));

			return sb.ToString();
		}

		#endregion

		#region Version 444 Upgrade Script

		static string GetVersion444Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate",
				"CREATE NONCLUSTERED INDEX IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate ON RefCusCondition (ZX1_ZZZ_NKDataGrouping ASC, ZX1_ZX2_ConditionType ASC, ZX1_ZZ1_Tariff ASC, ZX1_ZZ5_Nomenclature ASC, ZX1_StartDate ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 445 Upgrade Script

		static string GetVersion445Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode ON RefCusTariffUOM(ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZ1_Tariff ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZW_TariffNationalCode ASC)"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff ON RefCusTariffUOM(ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZW_TariffNationalCode ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZ1_Tariff ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 446 Upgrade Script

		static string GetVersion446Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping",
				"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping ON RefCusRate(ZZ2_ZZ1_Tariff ASC, ZZ2_ZZW_TariffNationalCode , ZZ2_ZY1_RateCode ASC, ZZ2_StartDate, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping) INCLUDE (ZZ2_EndDate)"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff_ZZ2_ZZW_TariffNationalCode"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode",
				"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZ1_Tariff_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode ON RefCusRate(ZZ2_ZZ1_Tariff ASC, ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZW_TariffNationalCode ASC)"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZ1_Tariff"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff",
				"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZ1_Tariff ON RefCusRate(ZZ2_ZZW_TariffNationalCode ASC, ZZ2_ZZZ_NKDataGrouping ASC, ZZ2_ZZ1_Tariff ASC)"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZZ_NKDataGrouping_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusRate), "IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping",
				"CREATE NONCLUSTERED INDEX IX_RefCusRate_ZZ2_ZZW_TariffNationalCode_ZZ2_ZY1_RateCode_ZZ2_StartDate_ZZ2_ZZS_Preference_ZZ2_ZZZ_NKDataGrouping ON RefCusRate(ZZ2_ZZW_TariffNationalCode, ZZ2_ZY1_RateCode ASC, ZZ2_StartDate, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping) INCLUDE (ZZ2_EndDate)"));

			return sb.ToString();
		}

		#endregion

		#region Version 447 Upgrade Script

		static string GetVersion447Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZZZ_NKDataGrouping_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusCondition), "IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping",
				"CREATE NONCLUSTERED INDEX IX_RefCusCondition_ZX1_ZX2_ConditionType_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_StartDate_ZX1_ZZZ_NKDataGrouping ON RefCusCondition (ZX1_ZX2_ConditionType ASC, ZX1_ZZ1_Tariff ASC, ZX1_ZZ5_Nomenclature ASC, ZX1_StartDate ASC, ZX1_ZZZ_NKDataGrouping ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 448 Upgrade Script

		static string GetVersion448Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode ON RefCusTariffUOM(ZZ8_ZZ1_Tariff ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZW_TariffNationalCode ASC)"));
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusTariffUOM), "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff ON RefCusTariffUOM(ZZ8_ZZW_TariffNationalCode ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZ1_Tariff ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 449 Upgrade Script

		static string GetVersion449Script()
		{
			var vatApplicability = new RefCusVATApplicability();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(vatApplicability.TableName, "ZX5_ZZ1_ParentTariffOrNationalCode", "AS (ISNULL(ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode))"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(vatApplicability.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{vatApplicability.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			sb.AppendLine(new View("VATApplicabilityView").GetCreateSqlScriptByVersion(2));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZZ1_ParentTariffOrNationalCode",
				"CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZZ1_ParentTariffOrNationalCode ON RefCusVATApplicability(ZX5_ZZ1_ParentTariffOrNationalCode)"));

			return sb.ToString();
		}

		#endregion

		#region Version 450 Upgrade Script
		static string GetVersion450Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 451 Upgrade Script
		static string GetVersion451Script()
		{
			var sb = new StringBuilder();
			var refCusApplicability = new RefCusApplicability();
			// Drop old indexes
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_Rate_SecondTradeGroup"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_Conditions_SecondTradeGroup"));

			// Add new indexes
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup ON RefCusApplicability (ZZT_ZX1_Conditions, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup) INCLUDE ([ZZT_EndDate]) WHERE ZZT_ZX1_Conditions IS NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup ON RefCusApplicability (ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup) INCLUDE ([ZZT_EndDate]) WHERE ZZT_ZZ2_Rate IS NOT NULL"));
			return sb.ToString();
		}

		#endregion

		#region Version 452 Upgrade Script

		static string GetVersion452Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 453 Upgrade Script
		static string GetVersion453Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 454 Upgrade Script
		static string GetVersion454Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 455 Upgrade Script
		static string GetVersion455Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 456 Upgrade Script
		static string GetVersion456Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 457 Upgrade Script
		static string GetVersion457Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 458 Upgrade Script
		static string GetVersion458Script()
		{
			return "select 1";
		}

		#endregion

		#region Version 459 Upgrade Script

		static string GetVersion459Script()
		{
			return SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(new View("TariffView").GetCreateSqlScriptByVersion(2), "V", "TariffView_V2");
		}

		#endregion

		#region Version 460 Upgrade Script

		static string GetVersion460Script()
		{
			var res = SharedDbSchemaChange.GetAddColumnIfNotExistsScript(nameof(RefCusVATApplicability), "ZX5_DataSetId",
				"SMALLINT NOT NULL CONSTRAINT DF_RefCusVATApplicability_ZX5_DataSetId DEFAULT 0");
			return res;
		}

		#endregion

		#region Version 461 Upgrade Script

		static string GetVersion461Script()
		{
			return @"UPDATE vat SET vat.ZX5_DataSetId=201
FROM RefCusVATApplicability vat
LEFT JOIN RefCusTariff taf1 ON vat.ZX5_ZZ1_Tariff=taf1.ZZ1_PK
LEFT JOIN RefCusTariffNationalCode cod ON vat.ZX5_ZZW_TariffNationalCode=cod.ZZW_PK
LEFT JOIN RefCusTariff taf2 ON cod.ZZW_ZZ1_Tariff=taf2.ZZ1_PK
WHERE taf1.ZZ1_ZZZ_NKDataGrouping='GB' OR taf2.ZZ1_ZZZ_NKDataGrouping='GB';

UPDATE RefDbVersionControl SET RVC_UpdaterVersion=10 WHERE RVC_DataSet IN ('RefCusTariff', 'GBCustomsTariffs') AND RVC_UpdaterVersion=9;
";
		}

		#endregion

		#region Version 462 Upgrade Script

		static string GetVersion462Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusVATApplicability), $"IX_RefCusVATApplicability_ZX5_ZZZ_NKDataGrouping_ZX5_PK"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusVATApplicability), "IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK",
				"CREATE CLUSTERED INDEX IX_RefCusVATApplicability_ZX5_DataSetId_ZX5_PK ON RefCusVATApplicability (ZX5_DataSetId ASC, ZX5_PK ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 463 Upgrade Script

		static string GetVersion463Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 464 Upgrade Script

		static string GetVersion464Script()
		{
			var refCusConditionType = new RefCusConditionType();
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusConditionType.TableName, "ZX2_ConditionType", "VARCHAR(6) NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", FormattableString.Invariant($"{refCusConditionType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"), "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", FormattableString.Invariant($"{refCusConditionType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2"), "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionType.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusConditionType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionType.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusConditionType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusConditionType.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refCusConditionType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));

			return sb.ToString();
		}

		#endregion

		#region Version 465 Upgrade Script

		static string GetVersion465Script()
		{
			var refCusCodeListAttributeName = new RefCusCodeListAttributeName();
			const string indexName = "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption";
			var query =
				$"CREATE UNIQUE NONCLUSTERED INDEX [{indexName}] ON [dbo].[RefCusCodeListAttributeName] (ZXE_ZZZ_NKDataGrouping, ZXE_ZZK_NKCodeType, ZXE_ColumnCaption) WHERE ZXE_ColumnCaption <> ''";

			return SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, indexName, query);
		}

		#endregion

		#region Version 466 Upgrade Script

		static string GetVersion466Script()
		{
			var sb = new StringBuilder();
			AddTableAndTableView(sb, new RefCusProfileType());
			AddTableAndTableView(sb, new RefCusProfileQuestion());
			AddTableAndTableView(sb, new RefCusProfile());
			AddTableAndTableView(sb, new RefCusProfileAttribute());

			return sb.ToString();
		}

		#endregion

		#region Version 467 Upgrade Script
		static string GetVersion467Script()
		{
			return "SELECT 1";
		}
		#endregion

		#region Version 468 Upgrade Script
		static string GetVersion468Script()
		{
			return SharedDbSchemaChange.GetAlterColumnIfExistsScript("UNDGSubstanceCFR", "CFR_Variation", "VARCHAR(150) NOT NULL");
		}
		#endregion

		#region Version 469 Upgrade Script
		static string GetVersion469Script()
		{
			return "SELECT 1";
		}
		#endregion

		#region Version 470 Upgrade Script

		static string GetVersion470Script()
		{
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefCusTariffNationalCode", "IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate"));

			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefCusTariffNationalCode",
				"IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode",
				@"
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode] ON [dbo].[RefCusTariffNationalCode]
(
	[ZZW_ZZZ_NKDataGrouping] ASC,
	[ZZW_ZZ1_Tariff] ASC,
	[ZZW_StartDate] ASC,
	[ZZW_ZZF_NKTaxOrFeeCode] ASC,
	[ZZW_NationalCode] ASC
)"));

			return sb.ToString();
		}

		#endregion

		#region Version 471 Upgrade Script
		static string GetVersion471Script()
		{
			var scripts = new StringBuilder();
			var undgSubstanceCFR = new UNDGSubstanceCFR();
			scripts.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("UNDGSubstanceCFR", "CFR_Variation", "VARCHAR(150) NOT NULL"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceCFRTableView_V1", "VIEW"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceCFRTableView_V2", "VIEW"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceCFRTableView_V3", "VIEW"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceCFR.TableViewScriptDictionary[1], "V", $"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceCFR.TableViewScriptDictionary[2], "V", $"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceCFR.TableViewScriptDictionary[3], "V", $"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceCFR.TableViewScriptDictionary[4], "V", $"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}4"));
			return scripts.ToString();
		}
		#endregion

		#region Version 472 Upgrade Script

		static string GetVersion472Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 473 Upgrade Script
		public static string GetVersion473Script()
		{
			var refExchangeRateZZ = new RefExchangeRateZZ();
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetAlterColumnIfExistsScript(new RefExchangeRateZZ().TableName, "ZZN_AsPublished", "VARCHAR(35) NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", FormattableString.Invariant($"{refExchangeRateZZ.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"), "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refExchangeRateZZ.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refExchangeRateZZ.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refExchangeRateZZ.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refExchangeRateZZ.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}
		#endregion

		#region Version 474 Upgrade Script

		static string GetVersion474Script()
		{
			var refDocOrgCusCode = new RefDocOrgCusCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refDocOrgCusCode.TableName, "DOC_Direction", "VARCHAR(3) NOT NULL CONSTRAINT [DF_RefDocOrgCusCode_DOC_Direction] DEFAULT 'BTH'"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refDocOrgCusCode.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refDocOrgCusCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 475 Upgrade Script

		static string GetVersion475Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 476 Upgrade Script

		static string GetVersion476Script()
		{
			var sb = new StringBuilder();

			var refCusProfileType = new RefCusProfileType();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusProfileType.TableName, "FK_RefCusProfileType_RefCusTariffType"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusProfileType.TableName, "FK_RefCusProfileType_RefCusTariffType", "XXX_ZZI_TariffType", "RefCusTariffType (ZZI_PK)"));

			AddTableAndTableView(sb, new RefCusProfileQuestion());
			AddTableAndTableView(sb, new RefCusProfileQuestionAnswerList());
			AddTableAndTableView(sb, new RefCusProfileQuestionAttribute());

			var refCusProfile = new RefCusProfile();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusProfile.TableName, "FK_RefCusProfile_XX0_XXX_ProfileType"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate_XX0_EndDate"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusProfile.TableName, "FK_RefCusProfile_XX0_XXX_ProfileType", "XX0_XXX_ProfileType", "RefCusProfileType (XXX_PK)"));

			var refCusProfileAttribute = new RefCusProfileAttribute();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusProfileAttribute.TableName, "FK_RefCusProfileAttribute_RefCusProfile"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusProfileAttribute.TableName, "FK_RefCusProfileAttribute_RefCusProfile", "XXY_XX0_Profile", "RefCusProfile (XX0_PK)"));

			return sb.ToString();
		}

		#endregion

		#region Version 477 Upgrade Script
		static string GetVersion477Script()
		{
			var refExchange = new RefExchangeRateZZ();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(
				refExchange.TableName,
				"CK_RefExchangeRateZZ_ZZN_ExRateType"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refExchange.TableName,
				"CK_RefExchangeRateZZ_ZZN_ExRateType",
				"([ZZN_ExRateType]='CUE' OR [ZZN_ExRateType]='CUS' OR [ZZN_ExRateType]='CUD' OR [ZZN_ExRateType]='IAT' OR [ZZN_ExRateType]='BNB' OR [ZZN_ExRateType]='BNS')"));

			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refExchange.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refExchange.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			return sb.ToString();
		}
		#endregion

		#region Version 478 Upgrade Script
		static string GetVersion478Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript("RefAccTaxRate", "CK_RefAccTaxRate_ZAT_RN_NKCountry"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefAccTaxRate", "IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript("RefAccTaxRate", "IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate_EndDate"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefAccTaxRate", "ZAT_RN_NKCountry", "CHAR(2) NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript("RefAccTaxRate", "CK_RefAccTaxRate_ZAT_RN_NKCountry", "[ZAT_RN_NKCountry] <> ''"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefAccTaxRate", "IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate ON RefAccTaxRate(ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript("RefAccTaxRate", "IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate_EndDate", "CREATE UNIQUE CLUSTERED INDEX IX_RefAccTaxRate_ZAT_RN_NKCountry_ReferenceRateType_StartDate_EndDate ON RefAccTaxRate(ZAT_RN_NKCountry, ZAT_ReferenceRateType, ZAT_StartDate, ZAT_EndDate)"));
			return sb.ToString();
		}
		#endregion

		#region Version 479 Upgrade Script
		static string GetVersion479Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefCusConditionTypeLanguage", "ZXW_Description", "NVARCHAR(500) NOT NULL"));
			return sb.ToString();
		}
		#endregion

		#region Version 480 Upgrade Script
		static string GetVersion480Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", "RefCusTariffLanguage", "DF_RefCusTariffLanguage_ZX7_Description"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefCusTariffLanguage", "ZX7_Description", "NVARCHAR(MAX) NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript("RefCusTariffLanguage", "DF_RefCusTariffLanguage_ZX7_Description", "''", "ZX7_Description"));
			return sb.ToString();
		}
		#endregion

		#region Version 481 Upgrade Script
		static string GetVersion481Script()
		{
			return @"IF EXISTS (SELECT 1 from sys.objects where name='[PK_RefCusExcludedTradeGroup' and type='PK')
EXEC sp_rename '[dbo].[RefCusExcludedTradeGroup].[[PK_RefCusExcludedTradeGroup]', 'PK_RefCusExcludedTradeGroup'";
		}
		#endregion

		#region Version 482 Upgrade Script
		static string GetVersion482Script()
		{
			var sb = new StringBuilder();
			var tariff = new RefCusTariff();
			var tariffNationCode = new RefCusTariffNationalCode();
			var vatApplicability = new RefCusVATApplicability();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{tariff.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{tariffNationCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{vatApplicability.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(tariff.TableViewScriptDictionary[1], "V", $"{tariff.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(tariffNationCode.TableViewScriptDictionary[1], "V", $"{tariffNationCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(vatApplicability.TableViewScriptDictionary[1], "V", $"{vatApplicability.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"));
			return sb.ToString();
		}
		#endregion

		#region Version 483 Upgrade Script
		static string GetVersion483Script()
		{
			var scripts = new StringBuilder();
			var undgSubstanceJTT = new UNDGSubstanceJTT();
			scripts.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("UNDGSubstanceJTT", "JTT_PSN", "VARCHAR(300) NOT NULL"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceJTTTableView_V1", "VIEW"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceJTT.TableViewScriptDictionary[1], "V", $"{undgSubstanceJTT.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceJTT.TableViewScriptDictionary[2], "V", $"{undgSubstanceJTT.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2"));
			return scripts.ToString();
		}

		#endregion

		#region Version 484 Upgrade Script

		static string GetVersion484Script()
		{
			var refCusTariffUOM = new RefCusTariffUOM();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusTariffUOM.TableName, "FK_RefCusTariffUOM_RefCusTradeGroup"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("foreign_keys", refCusTariffUOM.TableName, "FK_RefCusTariffUOM_RefCusTariffNationalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffUOMView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "TariffUOMView_V2", "VIEW"));

			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusTariffUOM.TableName, "ZZ8_ZZA_TradeGroup", "UNIQUEIDENTIFIER SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusTariffUOM.TableName, "ZZ8_ZZW_TariffNationalCode", "UNIQUEIDENTIFIER SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffUOM.TableName, "ZZ8_ZZA_SecondTradeGroup", "VARCHAR(35) SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffUOM.TableName, "ZZ8_StartDate", "SMALLDATETIME SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffUOM.TableName, "ZZ8_EndDate", "SMALLDATETIME SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffUOM.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusTariffUOM.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(new View("TariffUOMView").GetCreateSqlScriptByVersion(1), "V", "TariffUOMView_V1"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(new View("TariffUOMView").GetCreateSqlScriptByVersion(2), "V", "TariffUOMView_V2"));

			return sb.ToString();
		}

		#endregion

		#region Version 485 Upgrade Script

		static string GetVersion485Script()
		{
			var refCusTariffUOM = new RefCusTariffUOM();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffUOM.TableName, "CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate", "ZZ8_StartDate <= ZZ8_EndDate"));

			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode ON RefCusTariffUOM(ZZ8_ZZ1_Tariff ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZW_TariffNationalCode ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff ON RefCusTariffUOM(ZZ8_ZZW_TariffNationalCode ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZ1_Tariff ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup", "CREATE NONCLUSTERED INDEX IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup ON RefCusTariffUOM(ZZ8_ZZA_TradeGroup ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusTariffUOM.TableName, "FK_RefCusTariffUOM_RefCusTradeGroup", "ZZ8_ZZA_TradeGroup", "RefCusTradeGroup (ZZA_PK)"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusTariffUOM.TableName, "FK_RefCusTariffUOM_RefCusTariffNationalCode", "ZZ8_ZZW_TariffNationalCode", "RefCusTariffNationalCode (ZZW_PK)"));

			return sb.ToString();
		}

		#endregion

		#region Version 486 Upgrade Script

		static string GetVersion486Script()
		{
			var sb = new StringBuilder();
			AddTableAndTableView(sb, new RefCusProfileQuestionLanguage());
			AddTableAndTableView(sb, new RefCusProfileQuestionAnswerListLanguage());
			AddTableAndTableView(sb, new RefCusProfileQuestionPathway());

			return sb.ToString();
		}

		#endregion

		#region Version 487 Upgrade Script
		static string GetVersion487Script()
		{
			return "SELECT 1";
		}
		#endregion

		#region Version 488 Upgrade Script

		static string GetVersion488Script()
		{
			var sb = new StringBuilder();

			//Drop Views
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentTariffGroupTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRCommunityProtectionProfileTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRCommunityProtectionRiskMessageAdviceTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRCommunityProtectionRiskTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRLodgementQuestionTableView_V1", "VIEW"));

			//Drop Tables
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrumentTariffGroup", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrument", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRCommunityProtectionProfile", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRCommunityProtectionRiskMessageAdvice", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRCommunityProtectionRisk", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRLodgementQuestion", "TABLE"));

			return sb.ToString();
		}
		#endregion

		#region Version 489 Upgrade Script

		static string GetVersion489Script()
		{
			var sb = new StringBuilder();
			//Drop Views
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRStatisticalClassificationPeriodCharacteristicTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRStatisticalClassificationPeriodSnapshotTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPermitRequirementTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPermitRequirementExclusionsTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPreferenceSchemePeriodCountryTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPreferenceSchemeRuleTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPreferenceSchemePeriodSnapshotTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPreferenceRulePeriodTariffGroupTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPreferenceRulePeriodCharacteristicTableView_V1", "VIEW"));

			//Drop Tables
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRStatisticalClassificationPeriodCharacteristic", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRStatisticalClassificationPeriodSnapshot", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPermitRequirement", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPermitRequirementExclusions", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPreferenceSchemePeriodCountry", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPreferenceSchemeRule", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPreferenceSchemePeriodSnapshot", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPreferenceRulePeriodTariffGroup", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPreferenceRulePeriodCharacteristic", "TABLE"));

			return sb.ToString();
		}

		#endregion

		#region Version 490 Upgrade Script

		static string GetVersion490Script()
		{
			var sb = new StringBuilder();
			//Drop Views
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTreatmentRatePeriodAdditionalDutyCalculationTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTreatmentRatePeriodCharacteristicTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTreatmentRatePeriodSnapshotTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTariffRatePeriodAdditionalDutyCalculationTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTariffRatePeriodCharacteristicTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTariffRatePeriodSnapshotTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTreatmentSnapshotCountryTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTreatmentSnapshotTariffGroupTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTreatmentSnapshotTableView_V1", "VIEW"));

			//Drop Tables
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTreatmentRatePeriodAdditionalDutyCalculation", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTreatmentRatePeriodCharacteristic", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTreatmentRatePeriodSnapshot", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTariffRatePeriodAdditionalDutyCalculation", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTariffRatePeriodCharacteristic", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTariffRatePeriodSnapshot", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTreatmentSnapshotCountry", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTreatmentSnapshotTariffGroup", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTreatmentSnapshot", "TABLE"));

			return sb.ToString();
		}
		#endregion

		#region Version 491 Upgrade Script

		static string GetVersion491Script()
		{
			var sb = new StringBuilder();
			//Drop Views
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentCategoryTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentCategoryCharacteristicTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentCategoryCountryTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentCategoryTariffGroupTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentMessageAdviceTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentCharacteristicTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRInstrumentCountryTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTreatmentRatePeriodMessageAdviceTableView_V1", "VIEW"));

			//Drop Tables
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrumentCategory", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrumentCategoryCharacteristic", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrumentCategoryCountry", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrumentCategoryTariffGroup", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrumentMessageAdvice", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrumentCharacteristic", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRInstrumentCountry", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTreatmentRatePeriodMessageAdvice", "TABLE"));

			return sb.ToString();
		}

		#endregion

		#region Version 492 Upgrade Script

		static string GetVersion492Script()
		{
			var sb = new StringBuilder();
			//Drop Views
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTariffClassificationSnapshotTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTariffClassificationCharacteristicTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTariffRatePeriodMessageAdviceTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTariffClassificationMessageAdviceTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRTariffClassificationConcordanceTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPreferenceSchemeRuleMessageAdviceTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPreferenceRulePeriodSnapshotTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRPreferenceRulePeriodCountryTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "CMRStatisticalClassificationPeriodMessageAdviceTableView_V1", "VIEW"));

			//Drop Tables
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTariffClassificationSnapshot", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTariffClassificationCharacteristic", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTariffRatePeriodMessageAdvice", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTariffClassificationMessageAdvice", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRTariffClassificationConcordance", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPreferenceSchemeRuleMessageAdvice", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPreferenceRulePeriodSnapshot", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRPreferenceRulePeriodCountry", "TABLE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("U", "CMRStatisticalClassificationPeriodMessageAdvice", "TABLE"));

			return sb.ToString();
		}
		#endregion

		#region Version 493 Upgrade Script

		static string GetVersion493Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 494 Upgrade Script
		static string GetVersion494Script()
		{
			var sb = new StringBuilder();
			AddTableAndTableView(sb, new RefAccessorial());
			return sb.ToString();
		}
		#endregion

		#region Version 495 Upgrade Script
		static string GetVersion495Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropUniqueConstraintIfExistsScript(nameof(RefClient)));
			return sb.ToString();
		}
		#endregion

		#region Version 496 Upgrade Script
		static string GetVersion496Script()
		{
			var undgSubstanceJTT = new UNDGSubstanceJTT();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "UNDGSubstanceJTTTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceJTT.TableViewScriptDictionary[1], "V", $"{undgSubstanceJTT.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"));

			return sb.ToString();
		}

		#endregion

		#region Version 497 Upgrade Script

		static string GetVersion497Script()
		{
			var sb = new StringBuilder();
			sb.Append(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefUNLOCORelatedPort), $"IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort"));
			sb.Append(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefUNLOCORelatedPort), "IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort",
				"CREATE NONCLUSTERED INDEX [IX_RefUNLOCORelatedPort_RLR_RL_NKRelatedPort] ON [RefUNLOCORelatedPort] ([RLR_RL_NKRelatedPort] ASC)"));

			return sb.ToString();
		}

		#endregion

		#region Version 498 Upgrade Script
		static string GetVersion498Script()
		{
			var sb = new StringBuilder();

			var refCusProfile = new RefCusProfile();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refCusProfile.TableName, "DF_RefCusProfile_XX0_XX2_NKQuestionCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfile.TableName, "XX0_XX2_NKQuestionCode"));
			return sb.ToString();
		}
		#endregion

		#region Version 499 Upgrade Script
		static string GetVersion499Script()
		{
			var refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode", "UNIQUEIDENTIFIER NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusTariffAdditionalCode.TableName, "FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode", "ZY2_ZY2_TariffAdditionalCode", "RefCusTariffAdditionalCode (ZY2_PK)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode",
				"CREATE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping, ZY2_ZY2_TariffAdditionalCode)"));
			return sb.ToString();
		}
		#endregion

		#region Version 500 Upgrade Script
		static string GetVersion500Script()
		{
			return "SELECT 1";
		}
		#endregion

		#region Version 501 Upgrade Script

		static string GetVersion501Script()
		{
			var refCusCondition = new RefCusCondition();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCondition.TableName, "ZX1_Severity", "CHAR(3) SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusCondition.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refCusCondition.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));

			return sb.ToString();
		}

		#endregion

		#region Version 502 Upgrade Script

		static string GetVersion502Script()
		{
			var refCusCondition = new RefCusCondition();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusCondition.TableName, "CK_RefCusCondition_ZX1_Severity", "ZX1_Severity = 'MSG' OR ZX1_Severity = 'WAR'"));

			return sb.ToString();
		}

		#endregion

		#region Version 503 Upgrade Script
		static string GetVersion503Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", "RefCarrierCodeAttribute", "CK_RefCarrierCodeAttribute_ZZG_Value"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefCarrierCodeAttribute", "ZZG_Value", "NVARCHAR(100) NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript("RefCarrierCodeAttribute", "CK_RefCarrierCodeAttribute_ZZG_Value", "([ZZG_Value]<>'')"));
			return sb.ToString();
		}
		#endregion

		#region Version 504 Upgrade Script
		static string GetVersion504Script()
		{
			var refAccessorial = new RefAccessorial();
			var sb = new StringBuilder();

			// Remove constraints, primary key, and index
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refAccessorial.TableName, "DF_RefAccessorial_ACS_PK"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refAccessorial.TableName, "DF_RefAccessorial_ACS_Code"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("default_constraints", refAccessorial.TableName, "DF_RefAccessorial_ACS_Description"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refAccessorial.TableName, "CK_RefAccessorial_ACS_Code"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsScript("check_constraints", refAccessorial.TableName, "CK_RefAccessorial_ACS_Description"));
			sb.Append(SharedDbSchemaChange.GetDropPrimaryKeyIfExistsScript(refAccessorial.TableName, $"PK_{refAccessorial.TableName}"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refAccessorial.TableName, "IX_RefAccessorial_ACS_Code"));

			// Drop view
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefAccessorialTableView_V1", "VIEW"));

			return sb.ToString();
		}
		#endregion

		#region Version 505 Upgrade Script
		static string GetVersion505Script()
		{
			var refAccessorial = new RefAccessorial();
			var sb = new StringBuilder();

			// Rename columns
			sb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refAccessorial.TableName, "ACS_PK", "ASI_PK"));
			sb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refAccessorial.TableName, "ACS_Code", "ASI_Code"));
			sb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refAccessorial.TableName, "ACS_Description", "ASI_Description"));

			// Create view
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refAccessorial.TableViewScriptDictionary[1], "V", $"{refAccessorial.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"));

			return sb.ToString();
		}
		#endregion

		#region Version 506 Upgrade Script
		static string GetVersion506Script()
		{
			var refAccessorial = new RefAccessorial();
			var sb = new StringBuilder();

			// Add constraints, primary key, and index
			sb.Append(SharedDbSchemaChange.GetAddPrimaryKeyIfNotExistsScript(refAccessorial.TableName, $"PK_{refAccessorial.TableName}", "ASI_PK", "NONCLUSTERED"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refAccessorial.TableName, "IX_RefAccessorial_ASI_Code", "CREATE UNIQUE CLUSTERED INDEX [IX_RefAccessorial_ASI_Code] ON [dbo].[RefAccessorial] ([ASI_Code])"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refAccessorial.TableName, "DF_RefAccessorial_ASI_PK", "NEWID()", "ASI_PK"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refAccessorial.TableName, "DF_RefAccessorial_ASI_Code", "''", "ASI_Code"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refAccessorial.TableName, "DF_RefAccessorial_ASI_Description", "''", "ASI_Description"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refAccessorial.TableName, "CK_RefAccessorial_ASI_Code", "(LEN([ASI_Code])=(3))"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refAccessorial.TableName, "CK_RefAccessorial_ASI_Description", "([ASI_Description]<>'')"));

			return sb.ToString();
		}
		#endregion

		#region Version 507 Upgrade Script

		static string GetVersion507Script()
		{
			var sb = new StringBuilder();
			var rRefCusCodeListAttributeName = new RefCusCodeListAttributeName();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(rRefCusCodeListAttributeName.TableName, "ZXE_IsDateRangeUsed", "BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_IsDateRangeUsed DEFAULT 0"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(rRefCusCodeListAttributeName.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{rRefCusCodeListAttributeName.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 508 Upgrade Script

		static string GetVersion508Script()
		{
			var sb = new StringBuilder();
			var rRefCusCodeListAttribute = new RefCusCodeListAttribute();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(rRefCusCodeListAttribute.TableName, "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_Value"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(rRefCusCodeListAttribute.TableName, "ZZE_StartDate", "SMALLDATETIME SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(rRefCusCodeListAttribute.TableName, "ZZE_EndDate", "SMALLDATETIME SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(rRefCusCodeListAttribute.TableName, "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_StartDate_ZZE_Value", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList_ZZE_ZXE_NKName_ZZE_StartDate_ZZE_Value ON RefCusCodeListAttribute(ZZE_ZZD_CodeList ASC, ZZE_ZXE_NKName ASC, ZZE_StartDate ASC, ZZE_Value ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(rRefCusCodeListAttribute.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{rRefCusCodeListAttribute.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 509 Upgrade Script

		static string GetVersion509Script()
		{
			var sb = new StringBuilder();
			var rRefCusCodeListAttribute = new RefCusCodeListAttribute();
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(rRefCusCodeListAttribute.TableName, "CK_RefCusCodeListAttribute_ZZE_StartDate_ZZE_EndDate", "(ZZE_StartDate IS NULL AND ZZE_EndDate IS NULL) OR (ZZE_StartDate IS NOT NULL AND ZZE_EndDate IS NOT NULL AND ZZE_StartDate < ZZE_EndDate)"));

			return sb.ToString();
		}

		#endregion

		#region Version 510 Upgrade Script

		static string GetVersion510Script()
		{
			return new Trigger("TG_RefCusCodeListAttributeName_INS_UPD_Dates").GetCreateSqlScriptByVersion();
		}

		#endregion

		#region Version 511 Upgrade Script

		static string GetVersion511Script()
		{
			return new Trigger("TG_RefCusCodeListAttribute_INS_UPD_Dates").GetCreateSqlScriptByVersion();
		}

		#endregion

		#region Version 512 Upgrade Script

		static string GetVersion512Script()
		{
			var sb = new StringBuilder();

			var refCusProfile = new RefCusProfile();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(refCusProfile.TableName, "FK_RefCusProfile_XX0_XQ2_QuestionCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_XQ2_QuestionCode_XX0_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusProfile.TableName, "XX0_XQ2_QuestionCode"));

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfile.TableName, "XX0_QuestionCode", "VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusProfile_XX0_QuestionCode DEFAULT('')"));

			return sb.ToString();
		}

		#endregion

		#region Version 513 Upgrade Script

		static string GetVersion513Script()
		{
			var refExchange = new RefExchangeRateZZ();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(
				refExchange.TableName,
				"CK_RefExchangeRateZZ_ZZN_ExRateType"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(
				refExchange.TableName,
				"CK_RefExchangeRateZZ_ZZN_ExRateType",
				"([ZZN_ExRateType]='CUE' OR [ZZN_ExRateType]='CUS' OR [ZZN_ExRateType]='CUD' OR [ZZN_ExRateType]='IAT' OR [ZZN_ExRateType]='BNB' OR [ZZN_ExRateType]='BNS' OR [ZZN_ExRateType]='BUY' OR [ZZN_ExRateType]='SEL')"));

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefExchangeRateZZ_V3", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefExchangeRateZZ_V4", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refExchange.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refExchange.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refExchange.TableViewScriptDictionary[4], "V", FormattableString.Invariant($"{refExchange.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}4")));

			return sb.ToString();
		}

		#endregion

		#region Version 514 Upgrade Script
		static string GetVersion514Script()
		{
			return "SELECT 1";
		}
		#endregion

		#region Version 515 Upgrade Script
		static string GetVersion515Script()
		{
			var profileQuestionIndexName = "IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate";
			var profileQuestionPathwayIndexName = "IX_RefCusProfileQuestionPathway_XQP_XQ2_QuestionParent_XQP_XQ2_QuestionChild_XQP_StartDate";
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusProfileQuestion), profileQuestionIndexName));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(nameof(RefCusProfileQuestionPathway), profileQuestionPathwayIndexName));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusProfileQuestion), profileQuestionIndexName, "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate ON RefCusProfileQuestion (XQ2_QuestionCode ASC, XQ2_XXX_ProfileType ASC, XQ2_ZZZ_NKDataGrouping ASC, XQ2_StartDate ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(nameof(RefCusProfileQuestionPathway), profileQuestionPathwayIndexName, "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileQuestionPathway_XQP_XQ2_QuestionParent_XQP_XQ2_QuestionChild_XQP_StartDate ON RefCusProfileQuestionPathway (XQP_XQ2_QuestionParent ASC, XQP_XQ2_QuestionChild ASC, XQP_StartDate ASC)"));

			return sb.ToString();
		}
		#endregion

		#region Version 516 Upgrade Script

		static string GetVersion516Script()
		{
			var sb = new StringBuilder();
			AddTableAndTableView(sb, new RefAccElectronicProcessingFee());

			return sb.ToString();
		}

		#endregion

		#region Version 517 Upgrade Script

		static string GetVersion517Script()
		{
			var refCusTariffUOM = new RefCusTariffUOM();
			var sb = new StringBuilder();
			//1.Alter field ZZ8_ZZA_SecondTradeGroup type
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusTariffUOM.TableName, "ZZ8_ZZA_SecondTradeGroup", "UNIQUEIDENTIFIER SPARSE NULL"));
			//2. Add foreign key
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusTariffUOM.TableName, "FK_RefCusTariffUOM_RefCusTradeGroup_SecondTradeGroup", "ZZ8_ZZA_SecondTradeGroup", "RefCusTradeGroup (ZZA_PK)"));
			//3.Update View version
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusTariffUOMTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffUOM.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusTariffUOM.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffUOM.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusTariffUOM.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 518 Upgrade Script

		static string GetVersion518Script()
		{
			var refCusTariffUOM = new RefCusTariffUOM();
			var sb = new StringBuilder();
			//1.Alter CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusTariffUOM.TableName, "CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffUOM.TableName, "CK_RefCusTariffUOM_ZZ8_StartDate_ZZ8_EndDate", "(ZZ8_StartDate IS NULL AND ZZ8_EndDate IS NULL) OR (ZZ8_StartDate IS NOT NULL AND ZZ8_EndDate IS NOT NULL AND ZZ8_StartDate<=ZZ8_EndDate)"));
			//2.Alter Index *Tariff
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZ1_Tariff_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZW_TariffNationalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_Tariff_Type_UOM_NKDataGrouping_TradeGroup_SecondTradeGroup_StartDate_TariffNationalCode", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffUOM_Tariff_Type_UOM_NKDataGrouping_TradeGroup_SecondTradeGroup_StartDate_TariffNationalCode ON RefCusTariffUOM(ZZ8_ZZ1_Tariff ASC, ZZ8_Type ASC, ZZ8_UOM ASC, ZZ8_ZZZ_NKDataGrouping ASC, ZZ8_ZZA_TradeGroup ASC, ZZ8_ZZA_SecondTradeGroup ASC, ZZ8_StartDate ASC, ZZ8_ZZW_TariffNationalCode ASC)"));
			//3.Alter Index *NationalCode
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZW_TariffNationalCode_ZZ8_Type_ZZ8_UOM_ZZ8_ZZZ_NKDataGrouping_ZZ8_ZZA_TradeGroup_ZZ8_ZZ1_Tariff"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_TariffNationalCode", "CREATE NONCLUSTERED INDEX IX_RefCusTariffUOM_TariffNationalCode ON RefCusTariffUOM(ZZ8_ZZW_TariffNationalCode ASC)"));
			//4.Drop Index IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffUOM.TableName, "IX_RefCusTariffUOM_ZZ8_ZZA_TradeGroup"));

			return sb.ToString();
		}

		#endregion

		#region Version 519 Upgrade Script
		static string GetVersion519Script()
		{
			return "UPDATE RefDbVersionControl SET RVC_UpdaterVersion=2 WHERE RVC_DataSet = 'RefCusCodeList' AND RVC_UpdaterVersion=1;";
		}
		#endregion

		#region Version 520 Upgrade Script

		static string GetVersion520Script()
		{
			var refAccElectronicProcessingFee = new RefAccElectronicProcessingFee();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refAccElectronicProcessingFee.TableName, "EPF_JobDirection", "VARCHAR(3) NOT NULL CONSTRAINT DF_RefAccElectronicProcessingFee_EPF_CountryCode DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refAccElectronicProcessingFee.TableName, "EPF_CountryCode", "CHAR(2) NOT NULL CONSTRAINT DF_RefAccElectronicProcessingFee_EPF_JobDirection DEFAULT ''"));

			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refAccElectronicProcessingFee.TableName, "IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_Currency_ValidFrom"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refAccElectronicProcessingFee.TableName, "IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_CountryCode_JobDirection_Currency_ValidFrom", "CREATE UNIQUE CLUSTERED INDEX IX_RefAccElectronicProcessingFee_EPF_SystemCode_Category_Code_CountryCode_JobDirection_Currency_ValidFrom ON RefAccElectronicProcessingFee(EPF_SystemCode, EPF_Category, EPF_Code, EPF_CountryCode, EPF_JobDirection, EPF_Currency, EPF_ValidFrom)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refAccElectronicProcessingFee.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refAccElectronicProcessingFee.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 521 Upgrade Script
		static string GetVersion521Script()
		{
			// RefCarrierCodeAttribute.ZZG_Value has changed to NVARCHAR(100) at version 503
			return "SELECT 1";
		}
		#endregion

		#region Version 522 Upgrade Script
		static string GetVersion522Script()
		{
			return @"UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 5 WHERE RVC_DataSet = 'RefCusPreference' AND RVC_UpdaterVersion = 4;
UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 11 WHERE RVC_DataSet = 'RefCusTariff' AND RVC_UpdaterVersion = 10;
UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 11 WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_UpdaterVersion = 10;
UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 6 WHERE RVC_DataSet = 'RefCusNomenclatureGroup' AND RVC_UpdaterVersion = 5;";
		}
		#endregion

		#region Version 523 Upgrade Script
		static string GetVersion523Script()
		{
			return "SELECT 1";
		}
		#endregion

		#region Version 524 Upgrade Script
		static string GetVersion524Script()
		{
			var sb = new StringBuilder();
			var undgVersion = new UNDGVersion();
			AddTableAndTableView(sb, undgVersion);
			return sb.ToString();
		}

		#endregion

		#region Version 525 Upgrade Script
		static string GetVersion525Script()
		{
			var sb = new StringBuilder();
			var characteristicAttribute = new RefCusTariffBRCharacteristicAttribute();
			var characteristicValue = new RefCusTariffBRCharacteristicValue();
			var vatApplicability = new RefCusVATApplicability();

			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(
				characteristicAttribute.TableName,
				"IX_RefCusTariffBRCharacteristicAttribute_ZB3_ZB1_Characteristic",
				$"CREATE NONCLUSTERED INDEX IX_RefCusTariffBRCharacteristicAttribute_ZB3_ZB1_Characteristic ON {characteristicAttribute.TableName}(ZB3_ZB1_Characteristic)"
				));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(
				characteristicValue.TableName,
				"IX_RefCusTariffBRCharacteristicValue_ZB2_ZB1_Characteristic",
				$"CREATE NONCLUSTERED INDEX IX_RefCusTariffBRCharacteristicValue_ZB2_ZB1_Characteristic ON {characteristicValue.TableName}(ZB2_ZB1_Characteristic)"
				));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(
				vatApplicability.TableName,
				"IX_RefCusVATApplicability_ZX5_ZZ1_Tariff",
				$"CREATE NONCLUSTERED INDEX IX_RefCusVATApplicability_ZX5_ZZ1_Tariff ON {vatApplicability.TableName}(ZX5_ZZ1_Tariff)"
				));
			return sb.ToString();
		}

		#endregion

		#region Version 526 Upgrade Script
		static string GetVersion526Script()
		{
			return "SELECT 1";
		}
		#endregion

		#region Version 527 Upgrade Script
		static string GetVersion527Script()
		{
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("TR", "TG_RefCusCodeListAttribute_INS_UPD", "TRIGGER"));
			sb.AppendLine(new Trigger("TG_RefCusCodeListAttribute_INS_UPD").GetCreateSqlScriptByVersion());

			return sb.ToString();
		}
		#endregion

		#region Version 528 Upgrade Script

		static string GetVersion528Script()
		{
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("P", "GetApplicableRatesWithoutDataGrouping_V1", "PROCEDURE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("P", "GetApplicableRates_V1", "PROCEDURE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("P", "GetApplicableConditions_V1", "PROCEDURE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("P", "GetApplicableConditionsBySingleAdditionalCode_V1", "PROCEDURE"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("IF", "GetRatesBySingleCriteriaSet_V1", "FUNCTION"));
			return sb.ToString();
		}
		#endregion

		#region Version 529 Upgrade Script
		static string GetVersion529Script()
		{
			return @"UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 12 WHERE RVC_DataSet = 'RefCusTariff' AND RVC_UpdaterVersion = 11;
UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 12 WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_UpdaterVersion = 11;
UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 11 WHERE RVC_DataSet = 'RefDataGrouping' AND RVC_UpdaterVersion = 10;
UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 4 WHERE RVC_DataSet = 'RefCusTradeGroup' AND RVC_UpdaterVersion = 3;";
		}

		#endregion

		#region Version 530 Upgrade Script

		static string GetVersion530Script()
		{
			var refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(refCusTariffAdditionalCode.TableName, $"FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ParentAdditionalCode", "NVARCHAR(15) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY3_NKParentCategory", "CHAR(3) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_ZY3_NKParentCategory DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory",
				"CREATE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping ASC, ZY2_ZY3_NKCategory ASC, ZY2_AdditionalCode ASC, ZY2_ZZ1_Tariff ASC, ZY2_ZZW_NationalCode ASC, ZY2_ParentAdditionalCode ASC, ZY2_ZY3_NKParentCategory ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode_ZY2_ZY3_NKParentCategory", "CREATE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode_ZY2_ZY3_NKParentCategory ON RefCusTariffAdditionalCode (ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffAdditionalCode.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusTariffAdditionalCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}
		#endregion

		#region Version 531 Upgrade Script

		static string GetVersion531Script()
		{
			return new Trigger("TG_RefCusTariffAdditionalCode_INS_UPD_Parents").GetCreateSqlScriptByVersion();
		}
		#endregion

		#region Version 532 Upgrade Script

		static string GetVersion532Script()
		{
			var sb = new StringBuilder();
			var refVesselArrival = new RefVesselArrival();
			AddTableAndTableView(sb, refVesselArrival);
			return sb.ToString();
		}

		#endregion

		#region Version 533 Upgrade Script
		static string GetVersion533Script()
		{
			var refExchange = new RefExchangeRateZZ();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refExchange.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refExchange.TableName, "IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate", "CREATE NONCLUSTERED INDEX IX_RefExchangeRateZZ_ZZN_RN_NKCountry_ZZN_ExRateType_ZZN_RX_NKExCurrency_ZZN_StartDate_ZZN_EndDate ON RefExchangeRateZZ(ZZN_RN_NKCountry ASC, ZZN_ExRateType ASC, ZZN_RX_NKExCurrency ASC, ZZN_StartDate DESC, ZZN_EndDate ASC) INCLUDE (ZZN_Rate, ZZN_AsPublished)"));

			return sb.ToString();
		}
		#endregion

		#region Version 534 Upgrade Script

		static string GetVersion534Script()
		{
			var sb = new StringBuilder();
			var refGlbReleaseNote = new RefGlbReleaseNote();
			AddTableAndTableView(sb, refGlbReleaseNote);
			return sb.ToString();
		}

		#endregion

		#region Version 535 Upgrade Script

		static string GetVersion535Script()
		{
			var refCusProfile = new RefCusProfile();
			var refCusProfileQuestionPathway = new RefCusProfileQuestionPathway();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfile.TableName, "XX0_AllowMultipleAnswers", "BIT NOT NULL CONSTRAINT DF_RefCusProfile_XX0_AllowMultipleAnswers DEFAULT(0)"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfile.TableName, "XX0_IsAnswerMandatory", "BIT NOT NULL CONSTRAINT DF_RefCusProfile_XX0_IsAnswerMandatory DEFAULT(0)"));

			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusProfile.TableName, "XX0_TariffCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_QuestionCode_XX0_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refCusProfile.TableName, "XX0_TariffCode", "XX0_AppliesToCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusProfile.TableName, "DF_RefCusProfile_XX0_AppliesToCode", "''", "XX0_AppliesToCode"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate ON RefCusProfile (XX0_ZZZ_NKDataGrouping ASC, XX0_XXX_ProfileType ASC, XX0_AppliesToCode ASC, XX0_QuestionCode ASC, XX0_StartDate ASC)"));

			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProfile.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusProfile.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfileQuestionPathway.TableName, "XQP_AllowMultipleAnswers", "BIT NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_AllowMultipleAnswers DEFAULT(0)"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusProfileQuestionPathway.TableName, "XQP_IsAnswerMandatory", "BIT NOT NULL CONSTRAINT DF_RefCusProfileQuestionPathway_XQP_IsAnswerMandatory DEFAULT(0)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProfileQuestionPathway.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusProfileQuestionPathway.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 536 Upgrade Script
		static string GetVersion536Script()
		{
			var refCusApplicability = new RefCusApplicability();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusApplicability.TableName, "ZZT_ZZH_TariffRelationship", "UNIQUEIDENTIFIER SPARSE NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddForeignKeyIfNotExistsScript(refCusApplicability.TableName, "FK_RefCusApplicability_RefCusTariffRelationship", "ZZT_ZZH_TariffRelationship", "RefCusTariffRelationship (ZZH_PK)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusApplicability.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refCusApplicability.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			return sb.ToString();
		}
		#endregion

		#region Version 537 Upgrade Script
		static string GetVersion537Script()
		{
			var refCusApplicability = new RefCusApplicability();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusApplicability.TableName, "CK_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_ZX1_Conditions_ZZT_ZY2_AdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusApplicability.TableName, "CK_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_ZX1_Conditions_ZZT_ZY2_AdditionalCode_ZZT_ZZH_TariffRelationship", "(ZZT_ZX1_Conditions is not null and ZZT_ZZ2_Rate is null and ZZT_ZY2_AdditionalCode is null and ZZT_ZZH_TariffRelationship is null) or (ZZT_ZZ2_Rate is not null and ZZT_ZX1_Conditions is null and ZZT_ZY2_AdditionalCode is null and ZZT_ZZH_TariffRelationship is null) or (ZZT_ZY2_AdditionalCode is not null and ZZT_ZX1_Conditions is null and ZZT_ZZ2_Rate is null and ZZT_ZZH_TariffRelationship is null) or (ZZT_ZZH_TariffRelationship is not null and ZZT_ZX1_Conditions is null and ZZT_ZZ2_Rate is null and ZZT_ZY2_AdditionalCode is null)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusApplicability.TableName, "IX_RefCusApplicability_ZZT_ZZH_TariffRelationship_ZZT_StartDate_ZZT_ZZA_TradeGroup", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZZH_TariffRelationship_ZZT_StartDate_ZZT_ZZA_TradeGroup ON RefCusApplicability (ZZT_ZZH_TariffRelationship, ZZT_StartDate, ZZT_ZZA_TradeGroup) WHERE ZZT_ZZH_TariffRelationship IS NOT NULL"));
			return sb.ToString();
		}
		#endregion

		#region Version 538 Upgrade Script

		public static string GetVersion538Script()
		{
			var refCusTariffType = new RefCusTariffType();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusTariffType.TableName, "CK_RefCusTariffType_ZZI_Description"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusTariffType.TableName, "ZZI_Description", "NVARCHAR(100) NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffType.TableName, "CK_RefCusTariffType_ZZI_Description", "([ZZI_Description]<>'')"));

			return sb.ToString();
		}

		#endregion

		#region Version 539 Upgrade Script
		static string GetVersion539Script()
		{
			return @"UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 13 WHERE RVC_DataSet = 'RefCusTariff' AND RVC_UpdaterVersion = 12;
UPDATE RefDbVersionControl SET RVC_UpdaterVersion = 13 WHERE RVC_DataSet = 'GBCustomsTariffs' AND RVC_UpdaterVersion = 12;";
		}
		#endregion

		#region Version 540 Upgrade Script
		static string GetVersion540Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 541 Upgrade Script
		static string GetVersion541Script()
		{
			var refMessagingBussAttributeInfo = new RefMessagingBussAttributeInfo();
			var sb = new StringBuilder();
			AddTableAndTableView(sb, refMessagingBussAttributeInfo);
			return sb.ToString();
		}
		#endregion

		#region Version 542 Upgrade Script
		static string GetVersion542Script()
		{
			var refMessagingBussCarrierInfoAttribute = new RefMessagingBussCarrierInfoAttribute();
			var sb = new StringBuilder();
			AddTableAndTableView(sb, refMessagingBussCarrierInfoAttribute);
			return sb.ToString();
		}
		#endregion

		#region Version 543 Upgrade Script
		static string GetVersion543Script()
		{
			var refMessagingBussPackageInfoAttribute = new RefMessagingBussPackageInfoAttribute();
			var sb = new StringBuilder();
			AddTableAndTableView(sb, refMessagingBussPackageInfoAttribute);
			return sb.ToString();
		}
		#endregion

		#region Version 544 Upgrade Script
		static string GetVersion544Script()
		{
			var refGlbReleaseNote = new RefGlbReleaseNote();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefGlbReleaseNoteTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refGlbReleaseNote.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refGlbReleaseNote.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			return sb.ToString();
		}
		#endregion

		#region Version 545 Upgrade Script
		static string GetVersion545Script()
		{
			return "SELECT 1";
		}
		#endregion

		#region Version 546 Upgrade Script

		static string GetVersion546Script()
		{
			var refCusProfileType = new RefCusProfileType();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTypeTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfileType.TableName, "IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ZZI_TariffType_XXX_ProfileType"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfileType.TableName, "IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ProfileType_XXX_ZZI_TariffType"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusProfileType.TableName, "XXX_ZZI_TariffType", "UNIQUEIDENTIFIER NOT NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfileType.TableName, "IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ProfileType_XXX_ZZI_TariffType", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ProfileType_XXX_ZZI_TariffType ON RefCusProfileType (XXX_ZZZ_NKDataGrouping ASC, XXX_ProfileType ASC, XXX_ZZI_TariffType ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProfileType.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusProfileType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 547 Upgrade Script

		static string GetVersion547Script()
		{
			var refCusProfile = new RefCusProfile();
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", FormattableString.Invariant($"{refCusProfile.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1"), "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", FormattableString.Invariant($"{refCusProfile.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2"), "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refCusProfile.TableName, "XX0_TariffCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_TariffCode_XX0_QuestionCode_XX0_StartDate"));
			sb.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refCusProfile.TableName, "XX0_TariffCode", "XX0_AppliesToCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refCusProfile.TableName, "DF_RefCusProfile_XX0_AppliesToCode", "''", "XX0_AppliesToCode"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfile.TableName, "IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfile_XX0_ZZZ_NKDataGrouping_XX0_XXX_ProfileType_XX0_AppliesToCode_XX0_QuestionCode_XX0_StartDate ON RefCusProfile (XX0_ZZZ_NKDataGrouping ASC, XX0_XXX_ProfileType ASC, XX0_AppliesToCode ASC, XX0_QuestionCode ASC, XX0_StartDate ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProfile.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusProfile.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProfile.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusProfile.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProfile.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refCusProfile.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));

			return sb.ToString();
		}

		#endregion

		#region Version 548 Upgrade Script

		static string GetVersion548Script()
		{
			return SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(new View("TariffAdditionalCodeView").GetCreateSqlScriptByVersion(2), "V", "TariffAdditionalCodeView_V2");
		}

		#endregion

		#region Version 549 Upgrade Script

		static string GetVersion549Script()
		{
			var refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", FormattableString.Invariant($"{refCusTariffAdditionalCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2"), "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffAdditionalCode.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusTariffAdditionalCode.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}

		#endregion

		#region Version 550 Upgrade Script

		static string GetVersion550Script()
		{
			return "SELECT 1";
		}

		#endregion

		#region Version 551 Upgrade Script
		static string GetVersion551Script()
		{
			var refStlScript = new RefStlScript();
			return $@"
Update {refStlScript.TableName} set STL_CompanyCode='' where STL_CompanyCode is null;
Update {refStlScript.TableName} set STL_BranchCode ='' where STL_BranchCode is null;
Update {refStlScript.TableName} set STL_CreatingUserCode ='' where STL_CreatingUserCode is null;
Update {refStlScript.TableName} set STL_BillingReference1 ='' where STL_BillingReference1 is null;
Update {refStlScript.TableName} set STL_BillingReference2 ='' where STL_BillingReference2 is null;
Update {refStlScript.TableName} set STL_BillingReference3 ='' where STL_BillingReference3 is null;
Update {refStlScript.TableName} set STL_BillingReference4 ='' where STL_BillingReference4 is null;
Update {refStlScript.TableName} set STL_AdditionalRefs ='' where STL_AdditionalRefs is null;
Update {refStlScript.TableName} set STL_PreparationScript ='' where STL_PreparationScript is null;
Update {refStlScript.TableName} set STL_WhereClause ='' where STL_WhereClause is null;

ALTER INDEX IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version ON RefStlScript DISABLE;
Update {refStlScript.TableName} set STL_MinCW1Version ='' where STL_MinCW1Version is null;
Update {refStlScript.TableName} set STL_MaxCW1Version ='' where STL_MaxCW1Version is null;
WITH CTE AS (
	SELECT 
		*,
		ROW_NUMBER() OVER (PARTITION BY STL_FeatureCode, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version ORDER BY STL_PK DESC) AS rn
	FROM 
		RefStlScript
)
DELETE FROM CTE
WHERE rn > 1;
ALTER INDEX IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version ON RefStlScript REBUILD;
";
		}
		#endregion

		#region Version 552 Upgrade Script
		static string GetVersion552Script()
		{
			var refStlScript = new RefStlScript();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_CompanyCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_BranchCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_CreatingUserCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_BillingReference1"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_BillingReference2"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_BillingReference3"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_BillingReference4"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_AdditionalRefs"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_PreparationScript"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_WhereClause"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_MinCW1Version"));
			sb.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_MaxCW1Version"));
			sb.AppendLine($@"
ALTER TABLE RefStlScript ALTER COLUMN STL_CompanyCode NVARCHAR(1000) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BranchCode NVARCHAR(1000) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_CreatingUserCode NVARCHAR(1000) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference1 NVARCHAR(1000) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference2 NVARCHAR(1000) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference3 NVARCHAR(1000) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_BillingReference4 NVARCHAR(1000) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_AdditionalRefs NVARCHAR(1000) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_PreparationScript NVARCHAR(MAX) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_WhereClause NVARCHAR(MAX) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_MinCW1Version NVARCHAR(20) NOT NULL;
ALTER TABLE RefStlScript ALTER COLUMN STL_MaxCW1Version NVARCHAR(20) NOT NULL;
");
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_CompanyCode", "''", "STL_CompanyCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_BranchCode", "''", "STL_BranchCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_CreatingUserCode", "''", "STL_CreatingUserCode"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_BillingReference1", "''", "STL_BillingReference1"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_BillingReference2", "''", "STL_BillingReference2"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_BillingReference3", "''", "STL_BillingReference3"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_BillingReference4", "''", "STL_BillingReference4"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_AdditionalRefs", "''", "STL_AdditionalRefs"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_PreparationScript", "''", "STL_PreparationScript"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_WhereClause", "''", "STL_WhereClause"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_MinCW1Version", "''", "STL_MinCW1Version"));
			sb.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_MaxCW1Version", "''", "STL_MaxCW1Version"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refStlScript.TableName, "IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version",
				"CREATE UNIQUE NONCLUSTERED INDEX [IX_RefStlScript_STL_FeatureCode_STL_ActiveOn_STL_MinCW1Version_STL_MaxCW1Version] ON [RefStlScript] ([STL_FeatureCode], [STL_ActiveOn], [STL_MinCW1Version], [STL_MaxCW1Version])"));
			return sb.ToString();
		}
		#endregion

		#region Version 553 Upgrade Script

		static string GetVersion553Script() => "SELECT 1";
		#endregion

		#region Version 554 Upgrade Script
		static string GetVersion554Script()
		{
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("TR", "TG_RefCusCodeListAttribute_INS_UPD", "TRIGGER"));
			sb.AppendLine(new Trigger("TG_RefCusCodeListAttribute_INS_UPD").GetCreateSqlScriptByVersion());
			return sb.ToString();
		}
		#endregion

		#region Version 555 Upgrade Script
		static string GetVersion555Script()
		{
			var sb = new StringBuilder();

			var undgSubstanceADN = new UNDGSubstanceADN();
			var undgSubstanceADR = new UNDGSubstanceADR();
			var undgSubstanceCFR = new UNDGSubstanceCFR();
			var undgSubstanceJTT = new UNDGSubstanceJTT();
			var undgSubstanceRID = new UNDGSubstanceRID();

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceADN.TableName, "ADN_MinCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceADN_ADN_MinCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceADN.TableName, "ADN_MaxCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceADN_ADN_MaxCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceADR.TableName, "ADR_MinCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceADR_ADR_MinCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceADR.TableName, "ADR_MaxCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceADR_ADR_MaxCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceCFR.TableName, "CFR_MinCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceCFR_CFR_MinCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceCFR.TableName, "CFR_MaxCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceCFR_CFR_MaxCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceJTT.TableName, "JTT_MinCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceJTT_JTT_MinCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceJTT.TableName, "JTT_MaxCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceJTT_JTT_MaxCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceRID.TableName, "RID_MinCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceRID_RID_MinCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(undgSubstanceRID.TableName, "RID_MaxCW1Version", "NVARCHAR(20) NOT NULL CONSTRAINT [DF_UNDGSubstanceRID_RID_MaxCW1Version] DEFAULT ''"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceADN.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{undgSubstanceADN.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceADR.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{undgSubstanceADR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceCFR.TableViewScriptDictionary[5], "V", FormattableString.Invariant($"{undgSubstanceCFR.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}5")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceJTT.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{undgSubstanceJTT.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(undgSubstanceRID.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{undgSubstanceRID.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}
		#endregion

		#region Version 556 Upgrade Script
		static string GetVersion556Script()
		{
			var scripts = new StringBuilder();
			var refStlScript = new RefStlScript();
			scripts.AppendLine(SharedDbSchemaChange.GetDropConstraintIfExistsFromColumnNameScript(refStlScript.TableName, "STL_AdditionalRefs"));
			scripts.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript("RefStlScript", "STL_AdditionalRefs", "NVARCHAR(MAX) NOT NULL"));
			scripts.AppendLine(SharedDbSchemaChange.GetAddDefaultConstranintIfNotExistsScript(refStlScript.TableName, "DF_RefStlScript_STL_AdditionalRefs", "''", "STL_AdditionalRefs"));
			return scripts.ToString();
		}
		#endregion

		#region Version 557 Upgrade Script
		static string GetVersion557Script()
		{
			var refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
			var sb = new StringBuilder();
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY2_TariffAdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropForeignKeyIfExistsScript(refCusTariffAdditionalCode.TableName, $"FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZY3_NKCategory_ZY2_AdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusTariffAdditionalCode.TableName, "ZY2_ZY2_TariffAdditionalCode"));
			sb.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusTariffAdditionalCode.TableName, "IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping ASC, ZY2_ZY3_NKCategory ASC, ZY2_AdditionalCode ASC, ZY2_ZZ1_Tariff ASC, ZY2_ZZW_NationalCode ASC, ZY2_ParentAdditionalCode ASC, ZY2_ZY3_NKParentCategory ASC)"));

			return sb.ToString();
		}
		#endregion

		#region Version 558 Upgrade Script

		static string GetVersion558Script()
		{
			return SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(new View("TariffUOMView").GetCreateSqlScriptByVersion(3), "V", "TariffUOMView_V3");
		}
		#endregion

		#region Version 559 Upgrade Script
		static string GetVersion559Script()
		{
			var scripts = new StringBuilder();
			var refCusCodeListAttributeName = new RefCusCodeListAttributeName();
			var refCusCodeListAttributeNameTableViewPrefix = $"{refCusCodeListAttributeName.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
			foreach (var (key, _) in refCusCodeListAttributeName.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListAttributeNameTableViewPrefix}{key}", "VIEW"));
			}
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeType_ZXE_ZZZ_NKDataGrouping"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeTypeComputed"));

			scripts.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeType", "VARCHAR(10) NOT NULL"));
			scripts.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeTypeForValueList", "VARCHAR(10)"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeType_ZXE_ZZZ_NKDataGrouping", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeType_ZXE_ZZZ_NKDataGrouping ON RefCusCodeListAttributeName(ZXE_Name,ZXE_ZZK_NKCodeType,ZXE_ZZZ_NKDataGrouping)"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption", "CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption] ON [dbo].[RefCusCodeListAttributeName] (ZXE_ZZZ_NKDataGrouping, ZXE_ZZK_NKCodeType, ZXE_ColumnCaption) WHERE ZXE_ColumnCaption <> ''"));

			scripts.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN(ZXE_ZZK_NKCodeType) <= 5 THEN SUBSTRING(ZXE_ZZK_NKCodeType, 1, 5) ELSE '' END, '')"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping ON RefCusCodeListAttributeName(ZXE_Name,ZXE_ZZK_NKCodeTypeComputed,ZXE_ZZZ_NKDataGrouping)"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption",
				"CREATE NONCLUSTERED INDEX [IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption] ON [dbo].[RefCusCodeListAttributeName] (ZXE_ZZZ_NKDataGrouping, ZXE_ZZK_NKCodeTypeComputed, ZXE_ColumnCaption) WHERE ZXE_ColumnCaption <> ''"));

			foreach (var (key, value) in refCusCodeListAttributeName.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(value, "V", $"{refCusCodeListAttributeNameTableViewPrefix}{key}"));
			}

			return scripts.ToString();
		}
		#endregion

		#region Version 560 Upgrade Script
		static string GetVersion560Script()
		{
			var scripts = new StringBuilder();
			var refCusCodeList = new RefCusCodeList();
			var refCusCodeListTableViewPrefix = $"{refCusCodeList.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
			foreach (var (key, _) in refCusCodeList.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListTableViewPrefix}{key}", "VIEW"));
			}
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeType_ZZD_StartDate_ZZD_EndDate"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeType_ZZD_Code"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeTypeComputed"));

			scripts.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeType", "VARCHAR(10) NOT NULL"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeType_ZZD_Code", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeType_ZZD_Code ON RefCusCodeList ( ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeType, ZZD_Code )"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeType_ZZD_StartDate_ZZD_EndDate", "CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZK_NKCodeType_ZZD_StartDate_ZZD_EndDate ON RefCusCodeList(ZZD_ZZK_NKCodeType,ZZD_StartDate,ZZD_EndDate) INCLUDE (ZZD_PK,ZZD_Code,ZZD_Description,ZZD_ZZZ_NKDataGrouping)"));

			scripts.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN([ZZD_ZZK_NKCodeType]) <= 5 THEN SUBSTRING([ZZD_ZZK_NKCodeType], 1, 5) ELSE '' END, '')"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code ON RefCusCodeList (ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeTypeComputed, ZZD_Code)"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate ON RefCusCodeList(ZZD_ZZK_NKCodeTypeComputed,ZZD_StartDate,ZZD_EndDate) INCLUDE (ZZD_PK,ZZD_Code,ZZD_Description,ZZD_ZZZ_NKDataGrouping)"));

			foreach (var (key, value) in refCusCodeList.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(value, "V", $"{refCusCodeListTableViewPrefix}{key}"));
			}

			return scripts.ToString();
		}
		#endregion

		#region Version 561 Upgrade Script
		static string GetVersion561Script()
		{
			var scripts = new StringBuilder();
			var refCusCodeType = new RefCusCodeType();
			var refCusCodeTypeTableViewPrefix = $"{refCusCodeType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
			foreach (var (key, _) in refCusCodeType.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeTypeTableViewPrefix}{key}", "VIEW"));
			}
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeType"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropColumnIfExistsScript(refCusCodeType.TableName, "ZZK_CodeTypeComputed"));

			scripts.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusCodeType.TableName, "ZZK_CodeType", "VARCHAR(10) NOT NULL"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeType", "CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeType ON RefCusCodeType (ZZK_ZZZ_NKDataGrouping ASC, ZZK_CodeType ASC)"));

			scripts.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeType.TableName, "ZZK_CodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN(ZZK_CodeType) <= 5 THEN SUBSTRING(ZZK_CodeType, 1, 5) ELSE '' END, '')"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed ON RefCusCodeType (ZZK_ZZZ_NKDataGrouping ASC, ZZK_CodeTypeComputed ASC)"));

			foreach (var (key, value) in refCusCodeType.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(value, "V", $"{refCusCodeTypeTableViewPrefix}{key}"));
			}

			return scripts.ToString();
		}
		#endregion

		#region Version 562 Upgrade Script
		static string GetVersion562Script() => "SELECT 1";
		#endregion

		#region Version 563 Upgrade Script
		static string GetVersion563Script()
		{
			var sb = new StringBuilder();
			AddTableAndTableView(sb, new UNDGSubstanceTDG());

			return sb.ToString();
		}
		#endregion

		#region Version 564 Upgrade Script
		static string GetVersion564Script() => "SELECT 1";
		#endregion

		#region Version 565 Upgrade Script
		static string GetVersion565Script()
		{
			var sb = new StringBuilder();
			var refCusCodeList = new RefCusCodeList();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeListTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN([ZZD_ZZK_NKCodeType]) <= 5 THEN SUBSTRING([ZZD_ZZK_NKCodeType], 1, 5) ELSE '' END, '')"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code ON RefCusCodeList (ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeTypeComputed, ZZD_Code)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate ON RefCusCodeList(ZZD_ZZK_NKCodeTypeComputed,ZZD_StartDate,ZZD_EndDate) INCLUDE (ZZD_PK,ZZD_Code,ZZD_Description,ZZD_ZZZ_NKDataGrouping)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusCodeList.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusCodeList.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			var refCusCodeType = new RefCusCodeType();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeTypeTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeType.TableName, "ZZK_CodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN(ZZK_CodeType) <= 5 THEN SUBSTRING(ZZK_CodeType, 1, 5) ELSE '' END, '')"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed ON RefCusCodeType (ZZK_ZZZ_NKDataGrouping ASC, ZZK_CodeTypeComputed ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusCodeType.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusCodeType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 566 Upgrade Script

		static string GetVersion566Script()
		{
			var sb = new StringBuilder();
			AddTableAndTableView(sb, new RefCusCodeTypeAttribute());
			AddTableAndTableView(sb, new RefCusCodeTypeAttributeName());

			return sb.ToString();
		}

		#endregion

		#region Version 567 Upgrade Script
		static string GetVersion567Script()
		{
			var sb = new StringBuilder();
			var refCusCodeList = new RefCusCodeList();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeListTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN([ZZD_ZZK_NKCodeType]) <= 5 THEN SUBSTRING([ZZD_ZZK_NKCodeType], 1, 5) ELSE '' END, '')"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code ON RefCusCodeList (ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeTypeComputed, ZZD_Code)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate ON RefCusCodeList(ZZD_ZZK_NKCodeTypeComputed,ZZD_StartDate,ZZD_EndDate) INCLUDE (ZZD_PK,ZZD_Code,ZZD_Description,ZZD_ZZZ_NKDataGrouping)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusCodeList.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusCodeList.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			var refCusCodeType = new RefCusCodeType();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusCodeTypeTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeType.TableName, "ZZK_CodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN(ZZK_CodeType) <= 5 THEN SUBSTRING(ZZK_CodeType, 1, 5) ELSE '' END, '')"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed ON RefCusCodeType (ZZK_ZZZ_NKDataGrouping ASC, ZZK_CodeTypeComputed ASC)"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusCodeType.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusCodeType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));

			return sb.ToString();
		}

		#endregion

		#region Version 568 Upgrade Script
		static string GetVersion568Script()
		{
			var refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
			var refCusTariffAdditionalCodeTableName = refCusTariffAdditionalCode.TableName;
			var sb = new StringBuilder();

			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(
				refCusTariffAdditionalCodeTableName,
				"ZY2_StartDate", "SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_StartDate default '1900-01-01'"));
			sb.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(
				refCusTariffAdditionalCodeTableName,
				"ZY2_EndDate", "SMALLDATETIME NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_EndDate default '2079-06-06 23:59'"));

			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusTariffAdditionalCode.TableViewScriptDictionary[3], "V", FormattableString.Invariant($"{refCusTariffAdditionalCodeTableName}{SharedDbSchemaChange.TableViewVersionSuffix}3")));
			return sb.ToString();
		}
		#endregion

		#region Version 569 Upgrade Script

		static string GetVersion569Script()
		{
			var sb = new StringBuilder();
			var refCusTariffAdditionalCode = new RefCusTariffAdditionalCode();
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusTariffAdditionalCode.TableName, "CK_RefCusTariffAdditionalCode_ZY2_StartDate_ZY2_EndDate", "(ZY2_StartDate<=ZY2_EndDate)"));

			return sb.ToString();
		}

		#endregion

		#region Version 570 Upgrade Script

		static string GetVersion570Script()
		{
			var scripts = new StringBuilder();
			var refCusCodeListAttributeName = new RefCusCodeListAttributeName();
			var refCusCodeListAttributeNameTableViewPrefix = $"{refCusCodeListAttributeName.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";
			foreach (var (key, _) in refCusCodeListAttributeName.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListAttributeNameTableViewPrefix}{key}", "VIEW"));
			}
			scripts.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeListAttributeName.TableName, "ZXE_ZZK_NKCodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN(ZXE_ZZK_NKCodeType) <= 5 THEN SUBSTRING(ZXE_ZZK_NKCodeType, 1, 5) ELSE '' END, '')"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeTypeComputed_ZXE_ZZZ_NKDataGrouping ON RefCusCodeListAttributeName(ZXE_Name,ZXE_ZZK_NKCodeTypeComputed,ZXE_ZZZ_NKDataGrouping)"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeListAttributeName.TableName, "IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption",
				"CREATE NONCLUSTERED INDEX [IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeTypeComputed_ZXE_ColumnCaption] ON [dbo].[RefCusCodeListAttributeName] (ZXE_ZZZ_NKDataGrouping, ZXE_ZZK_NKCodeTypeComputed, ZXE_ColumnCaption) WHERE ZXE_ColumnCaption <> ''"));

			foreach (var (key, value) in refCusCodeListAttributeName.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(value, "V", $"{refCusCodeListAttributeNameTableViewPrefix}{key}"));
			}

			return scripts.ToString();
		}

		#endregion

		#region Version 571 Upgrade Script

		static string GetVersion571Script()
		{
			var scripts = new StringBuilder();
			var refCusCodeList = new RefCusCodeList();
			var refCusCodeListTableViewPrefix = $"{refCusCodeList.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";

			foreach (var (key, _) in refCusCodeList.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeListTableViewPrefix}{key}", "VIEW"));
			}

			scripts.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeList.TableName, "ZZD_ZZK_NKCodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN([ZZD_ZZK_NKCodeType]) <= 5 THEN SUBSTRING([ZZD_ZZK_NKCodeType], 1, 5) ELSE '' END, '')"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code ON RefCusCodeList (ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeTypeComputed, ZZD_Code)"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeList.TableName, "IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate ON RefCusCodeList(ZZD_ZZK_NKCodeTypeComputed,ZZD_StartDate,ZZD_EndDate) INCLUDE (ZZD_PK,ZZD_Code,ZZD_Description,ZZD_ZZZ_NKDataGrouping)"));

			foreach (var (key, value) in refCusCodeList.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(value, "V", $"{refCusCodeListTableViewPrefix}{key}"));
			}


			return scripts.ToString();
		}

		#endregion

		#region Version 572 Upgrade Script

		static string GetVersion572Script()
		{
			var scripts = new StringBuilder();
			var refCusCodeType = new RefCusCodeType();
			var refCusCodeTypeTableViewPrefix = $"{refCusCodeType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";

			foreach (var (key, _) in refCusCodeType.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusCodeTypeTableViewPrefix}{key}", "VIEW"));
			}

			scripts.AppendLine(SharedDbSchemaChange.GetAddColumnIfNotExistsScript(refCusCodeType.TableName, "ZZK_CodeTypeComputed",
				"AS ISNULL(CASE WHEN LEN(ZZK_CodeType) <= 5 THEN SUBSTRING(ZZK_CodeType, 1, 5) ELSE '' END, '')"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusCodeType.TableName, "IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed",
				"CREATE NONCLUSTERED INDEX IX_RefCusCodeType_ZZK_ZZZ_NKDataGrouping_ZZK_CodeTypeComputed ON RefCusCodeType (ZZK_ZZZ_NKDataGrouping ASC, ZZK_CodeTypeComputed ASC)"));

			foreach (var (key, value) in refCusCodeType.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(value, "V", $"{refCusCodeTypeTableViewPrefix}{key}"));
			}


			return scripts.ToString();
		}

		#endregion

		#region Version 573 Upgrade Script
		static string GetVersion573Script()
		{
			var scripts = new StringBuilder();
			var refCusProfileQuestion = new RefCusProfileQuestion();
			var refCusProfileQuestionTableViewPrefix = $"{refCusProfileQuestion.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";

			foreach (var (key, _) in refCusProfileQuestion.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", $"{refCusProfileQuestionTableViewPrefix}{key}", "VIEW"));
			}
			scripts.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusProfileQuestion.TableName, "CK_RefCusProfileQuestion_XQ2_Code"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfileQuestion.TableName, "IX_RefCusProfileQuestion_XQ2_Code_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate"));
			scripts.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refCusProfileQuestion.TableName, "XQ2_Code", "XQ2_QuestionCode"));

			return scripts.ToString();
		}
		#endregion

		#region Version 574 Upgrade Script
		static string GetVersion574Script()
		{
			var scripts = new StringBuilder();
			var refCusProfileQuestion = new RefCusProfileQuestion();
			var refCusProfileQuestionTableViewPrefix = $"{refCusProfileQuestion.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}";

			foreach (var (key, value) in refCusProfileQuestion.TableViewScriptDictionary)
			{
				scripts.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(value, "V", $"{refCusProfileQuestionTableViewPrefix}{key}"));
			}
			scripts.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refCusProfileQuestion.TableName, "CK_RefCusProfileQuestion_XQ2_QuestionCode", "(XQ2_QuestionCode <> '')"));
			scripts.AppendLine(SharedDbSchemaChange.GetCreateIndexIfNotExistsScript(refCusProfileQuestion.TableName,
				"IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate",
				"CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate ON RefCusProfileQuestion (XQ2_QuestionCode ASC, XQ2_XXX_ProfileType ASC, XQ2_ZZZ_NKDataGrouping ASC, XQ2_StartDate ASC)"));

			return scripts.ToString();
		}
		#endregion

		#region Version 575 Upgrade Script

		static string GetVersion575Script()
		{
			var sb = new StringBuilder();
			var refStlScript = new RefStlScript();
			sb.AppendLine(SharedDbSchemaChange.GetAddCheckConstraintIfNotExistsScript(refStlScript.TableName, "CK_RefStlScript_STL_CollectionStartDateUtc", "(STL_CollectionStartDateUtc IS NULL OR STL_CollectionStartDateUtc > '1900-01-01 00:00:00')"));

			return sb.ToString();
		}

		#endregion

		#region Version 576 Upgrade Script

		static string GetVersion576Script()
		{
			return SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(new View("TariffAdditionalCodeView").GetCreateSqlScriptByVersion(3), "V", "TariffAdditionalCodeView_V3");
		}

		#endregion


		#region Version 577 Upgrade Script
		static string GetVersion577Script() => "UPDATE RefDbVersionControl SET RVC_UpdaterVersion=14 WHERE RVC_DataSet = 'RefCusTariff' AND RVC_UpdaterVersion=13;";
		#endregion

		#region Version 578 Upgrade Script
		static string GetVersion578Script()
		{
			var sb = new StringBuilder();
			var refCusProfileType = new RefCusProfileType();
			sb.AppendLine(SharedDbSchemaChange.GetDropSqlObjectIfExistsScript("V", "RefCusProfileTypeTableView_V1", "VIEW"));
			sb.AppendLine(SharedDbSchemaChange.GetAlterColumnIfExistsScript(refCusProfileType.TableName, "XXX_ZZI_TariffType", "UNIQUEIDENTIFIER NULL"));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProfileType.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{refCusProfileType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
			sb.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(refCusProfileType.TableViewScriptDictionary[2], "V", FormattableString.Invariant($"{refCusProfileType.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}2")));

			return sb.ToString();
		}
		#endregion

		#region Version 579 Upgrade Script

		static string GetVersion579Script()
		{
			return $@"
UPDATE RefDbVersionControl SET RVC_LastUpdatedUtc = '2025-04-02 00:00:00' WHERE RVC_DataSet = '{nameof(RefExchangeRateZZ)}';
UPDATE RefDbVersionControl SET RVC_LastUpdatedUtc = '2025-04-02 00:00:00' WHERE RVC_DataSet = '{nameof(RefCusQuota)}';
UPDATE RefDbVersionControl SET RVC_LastUpdatedUtc = '2025-04-02 00:00:00' WHERE RVC_DataSet = '{nameof(RefCusTaxOrFeeType)}';
";
		}

		#endregion

		static void AddTableAndTableView(StringBuilder stringBuilder, ITableScript tableScript)
		{
			stringBuilder.AppendLine(GetCreateTableWithIndexSql(tableScript));
			stringBuilder.AppendLine(SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(tableScript.TableViewScriptDictionary[1], "V", FormattableString.Invariant($"{tableScript.TableName}{SharedDbSchemaChange.TableViewVersionSuffix}1")));
		}

		static string RemoveContainingLine(string contents, string toRemove)
		{
			var lines = contents.Split('\n').Where(x => !x.Contains(toRemove));
			return string.Join("\n", lines);
		}

#if DEBUG
		public
#endif
		static List<Type> TableExclusionListForVersion313Upgrader => new List<Type>
		{
			typeof(RefAccessorial),
			typeof(RefAccElectronicProcessingFee),
			typeof(RefAirlineCommodityCode),
			typeof(RefAirlineProductCode),
			typeof(RefAirlineProductCodeCommodityCodePivot),
			typeof(RefCarrierCodeLanguage),
			typeof(RefClient),
			typeof(RefCusCodeTypeAttribute),
			typeof(RefCusCodeTypeAttributeName),
			typeof(RefCusConditionCode),
			typeof(RefCusConditionCodeLanguage),
			typeof(RefCusConditionLanguage),
			typeof(RefCusConfiguration),
			typeof(RefCusQuota),
			typeof(RefCusTariffAttributeName),
			typeof(RefCusTariffBRCharacteristic),
			typeof(RefCusTariffBRCharacteristicAttribute),
			typeof(RefCusTariffBRCharacteristicValue),
			typeof(RefCusProfileType),
			typeof(RefCusProfile),
			typeof(RefCusProfileAttribute),
			typeof(RefCusProfileQuestion),
			typeof(RefCusProfileQuestionAnswerList),
			typeof(RefCusProfileQuestionAttribute),
			typeof(RefCusProfileQuestionLanguage),
			typeof(RefCusProfileQuestionAnswerListLanguage),
			typeof(RefCusProfileQuestionPathway),
			typeof(RefCusTariffTypeLanguage),
			typeof(RefGlbReleaseNote),
			typeof(RefMessagingBussCarrierInfo),
			typeof(RefMessagingBussPackageInfo),
			typeof(RefMessagingBussPackageVersion),
			typeof(RefMessagingBussAttributeInfo),
			typeof(RefMessagingBussCarrierInfoAttribute),
			typeof(RefMessagingBussPackageInfoAttribute),
			typeof(RefStlFieldMapping),
			typeof(RefStlScript),
			typeof(RefUNLOCO),
			typeof(RefUNLOCOUtcOffset),
			typeof(RefUNLOCORelatedPort),
			typeof(RefVesselArrival),
			typeof(UNDGVersion),
			typeof(UNDGSubstanceTDG)
		};
	}
}
