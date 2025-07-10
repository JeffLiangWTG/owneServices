using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PreUpgradeTransformationTasks : TransformationTasks
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
		/// See ShelfCheckinPreUpgradeTransformationMapper.txt for instructions on how to add and map
		/// new transformations according to the SHELF CHECK-IN process
		/// 
		/// </summary>
		/// 
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get
			{
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefCusProfileQuestionRenameColumnTransformation(307);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixRefStlScriptColumnsNotNullTransformation(301);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixRefCusRateUOMAddIndexOfRateAndUOMTransformation(292);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveRefClientUniqueConstraintTransformation(268);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations.DataFixWI00699943Transformation(254);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveDuplicateRefCusVATApplicabilityWI00627080Transformation(236);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveDuplicateRefCusExcludedTradeGroupTransformation(228);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00503706Transformation(218);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DeleteInvalidRefLocoMapAndRefUNLOCOUtcOffset(208);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefSysConfigAddAndRemoveColumnAndPopulateNewColumnTask(185);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.OffsetFromUTCToOffsetMinutesFromUTC(178);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixRefCusCodeListAttributeNameTransformation(176);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefTimeZoneRuleUniqueIndexValidation(159);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveInvalidRefUNLOCOTransformation(147);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations.MakeRVC_ParentPKPrimaryKeyTransformation(134);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataFixWI00338029Transformation(132);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefCurrencyPopulateISOSubUnitRatio(113);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveTransactionIdLevelTransformation(109);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RenameDG_FPtoDG_FlashPoint(100);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DuplicatedEndDatesRemoval(98);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.SchemaChangeTimeZoneDataTransformWI00202373(90);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RenameRefDbVersionControlColumns(82);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RenameClientRefDbVersionControlColumns(82);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveDuplicateRefAccTaxRateTransformation(76);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RefCusProcedureChangeWarehouseColumnsDataType(72);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RemoveDuplicateRefCusCodeListTransformation(69);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.FixZZ2_RX_NKCurrencyOverrideColumn(56);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RenameRefCusCodeListAttributeZZEName(51);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.RenameRefCusRateCodeLanguageColumns(44);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.USCustomsExportCodeCorrectionTransformation(39);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.EUQuotaWI00187638Transformation(35);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataSetPKTransformationOnRefCusTariffUOMTask(28);
				yield return new CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataSetPKTransformationOnRefCusTariffUOMTask(26);
				yield return new DataSetPKTransformationOnRefCusCodeOrAttributeTransportModeTask(21);
				yield return new DropUnnamedConstraintTask(20);
				yield return new DataSetPKTransformationOnTariffAttributeTask(15);
				yield return new DropSanctionTablesTask(14);
				yield return new DataSetPKTransformationTask(12);
				yield return new RenameToZZZNKDataGroupColumnTask(8);
				yield return new PopulateRefDataGroupingTask(8);
				yield return new ChangeRefCusCodeTypePrefixTask(8);
				yield return new AddZZZ_NKDataGroupColumnTask(8);
			}
		}
	}
}
