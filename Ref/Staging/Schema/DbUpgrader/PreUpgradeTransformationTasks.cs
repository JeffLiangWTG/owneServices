using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
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
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.RefCusProfileQuestionRenameColumnTransformation(265);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.DataFixRefStlScriptColumnsNotNullTransformation(259);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00843852Transformation(251);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.RemoveInvalidProcessorStatusTransformation(227);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00696614Transformation(211);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.QuartzJobTypeTransformationTask(201);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00503706Transformation(185);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.DataFixCheckConstraintsTransformation(170);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.DeleteCodeTypesWithEmptyDataGrouping(148);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.RemoveInvalidRefUNLOCOTransformation(124);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.DataFixWI00338029Transformation(115);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.RefCusProcedureChangeWarehouseColumnsDataType(78);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.FixZZ2_RX_NKCurrencyOverrideColumn(60);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.RenameRefCusCodeListAttributeZZEName(56);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.DBChangeWI00193665Transformation(46);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.RenameRefCusRateCodeLanguageColumns(44);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.AddPrefixToProcessorStatusColumns(43);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.AddPrefixToSourceDataColumns(43);
				yield return new DropUnnamedConstraintTask(17);
				yield return new TariffTypeNKTransformationTask(14);
				yield return new RenameToZZZNKDataGroupColumnTask(8);
			}
		}
	}
}
