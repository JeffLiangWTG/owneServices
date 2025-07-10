using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	/// <summary>
	///  _____                _   _______ _     _       ______ _          _   _
	/// |  __ \              | | |__   __| |   (_)     |  ____(_)        | | | |
	/// | |__) |___  __ _  __| |    | |  | |__  _ ___  | |__   _ _ __ ___| |_| |
	/// |  _  // _ \/ _` |/ _` |    | |  | '_ \| / __| |  __| | | '__/ __| __| |
	/// | | \ \  __/ (_| | (_| |    | |  | | | | \__ \ | |    | | |  \__ \ |_|_|
	/// |_|  \_\___|\__,_|\__,_|    |_|  |_| |_|_|___/ |_|    |_|_|  |___/\__(_)
	///
	/// *******************************************************************
	/// **********      DO NOT CHANGE THIS FILE MANUALLY!!!      **********
	/// *******************************************************************
	/// 
	/// See ShelfCheckinDataTransformationMapper.txt for instructions on how to add and map
	/// new transformations according to the SHELF CHECK-IN process
	/// 
	/// </summary>
	///
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506: Avoid excessive class coupling")]
	public class DataTransformationTasks : TransformationTasks
	{
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get
			{
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefAccessorial(119);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationUNDGVersion(118);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefComplianceCommodityAlert(117);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefGlbReleaseNote(116);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefEquipmentGrade(114);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefAccElectronicProcessingFee(113);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefCusProfileQuestion(112);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefCusProfileQuestionPathway(112);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefCusProfile(111);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefRepairCode(110);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefUnitSection(110);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefCusProfileType(109);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefMRComponentCode(108);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefDamage(107);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefMaterial(107);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.PopulateRefDocOrgCusCodeTransformation(106);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.UserAuthorizationUpdateUserTransformation(105);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefMessagingBussPackageInfo(104);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefCusConditionCode(103);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefCusTariffAttributeName(102);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations.CorrectRefClientRDSTableCodeTransformation(101);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefClient(100);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixShippingLineIsShippingLineTransformation(98);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.PopulateDatasetPKAndCodeForRefCusTariffBRCharacteristic(97);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.PopulateRefDataSetInformationInactiveRefAirline(96);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00514782Transformation(95);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixDeleteRefCarrierVesselPivotWhereVesselIsDeleted(94);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefStlFieldMapping(93);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.UpdateRefShippingLineTransformation(92);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefFacility(91);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataSetPKTransformationOnRefCusTaxOrFeeLanguage(90);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataSetPKTransformationOnRefCusRateCodeLanguage(90);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefCusQuota(89);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefAirlineProductCode(88);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixRemoveEUNConditionWhenConditionTypeIsF(87);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefCusConfiguration(86);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.FixZATariffUom10SticksTransformation(85);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefStlScript(84);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefShippingLineMessagingRequirementType(83);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00437538Transformation(82);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefAirlineCommodityCode(81);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations.FixEUNConditionsWithIncorrectSupUOMnFormulanValueType(80);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationUNDGCountryReference(78);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationUNDGSubstanceCFR(77);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefAirline(76);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefVessel(75);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationUNDGSubstanceJTT(74);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.UpdateRefCusRateCode_InternalUseTransformation(73);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefPortPolygon(72);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixIncorrectTriggerOnRefCusExcludedTradeGroup(71);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixConditionTypeTransformation(70);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixConditionTypeTransformation(69);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationRefSysConfigType(68);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixHLTConditionalRateFormulaEUNTransformation(67);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixIncorrectTimezoneToYear(66);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixRefCusRateIncorrectFormulas(65);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefDataSetInformationUNDGSubstanceADN(64);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.FixEUNTariffPreferenceRateMappingTransformation(63);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixExchangeRateZWD(62);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DeleteDuplicateVATApplicabilitiesTransformation(61);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00307638Transformation(60);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.PopulateRefDataSetInitialInformation(59);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.PopulateDataChangeHistoryForExistingRecordsPart2(58);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DeactivateUNDGSubstanceWithIncorrectVariantInformation(57);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations.CreateVersionControlForRefCusCodeListAttributeName(56);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00289989RefShippingLineTransformation(55);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixRefCusCodeListLanguageDETransformation(53);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.UpdateUNDGSubstanceIATAVariantsTransformation(52);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveInvalidLengthEUNIMPTariffs(51);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00271682MeursingDataTransformation(50);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.ChangeRateCodeFrom160To125ForItalianTariffTransformation(49);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixApplicabilityDataSetPKs(48);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.PopulateDataChangeHistoryForExistingRecordsPart1(47);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.AddingRVC_IsPublishedTranformation(46);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.PopulateDataChangeHistoryForExistingRecords(45);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00227004Transformation(44);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00165065Transformation(43);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00234455Transformation(42);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DatafixZZ1_IAmUniqueWI00220677(41);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.FixDuplicateRatesForZA2P3(40);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DeleteEUNConditionsWithMoreThanOneConditionValueType(39);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.GeoLocationWI00199334Transformation(38);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixEUNInvalidTariffs(37);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.TariffTypeAddWI00222952Transformation(36);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00218681Transformation(35);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.AddEUTradeGroupWI00217416Task(34);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DeleteCASIMAIncorrectRates(33);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00205047Transformation(32);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00214784Transformation(31);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00212326Transformation(30);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00212036Transformation(29);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00212900Transformation(28);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00196253Transformation(27);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00203641Transformation(26);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00205448Transformation(24);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RuleRemoveWI00205083Transformation(23);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveDuplicatedMEURates(22);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Za17ARuleWI00202850Transformation(21);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.EUQuotaWI00199546Transformation(19);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.FixMeursingApplicationDataSetPKTransformation(18);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00196418Transformation(17);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.AddRulesWI00196189Transformation(16);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.FormulaChangeWI00196892Transformation(15);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00195443Transformation(14);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.MergeMeursingTariffsTransformation(12);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00190456Transformation(11);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00189669Transformation(10);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00183560Transformation(9);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixZARefCusTariffRelationshipTariffTypeTask(8);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RuleAddWI00176734Transformation(7);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RuleAddWI00178368Transformation(6);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.FixTransformDataSelectorFormulaTask(5);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.TransformDataSelectorFormulaTask(4);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixesWI00175292Transformation(3);
				yield return new IncorrectEndDateTransformation(2);
				yield return new PRCCTransformation(1);
			}
		}
	}
}
