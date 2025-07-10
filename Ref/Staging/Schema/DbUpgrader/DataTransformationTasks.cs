using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
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
	public class DataTransformationTasks : TransformationTasks
	{
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get
			{
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00879039Transformation(21);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00733810Transformation(20);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.UpdateRefApplicationAttributeTransformation(19);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.PopulateProcessorStatusTransformation(18);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00725382Transformation(17);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00707281Transformation(16);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00683548Transformation(15);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.PopulateDatasetPKInDPRTransformation(14);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00616785Transformation(12);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.UpdateQuartzJobGroupsTransformation(11);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.UpdateQuartzJobGroupsTransformation(10);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00470139Transformation(9);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00473541Transformation(8);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.PopulateUnitCodeTransformation(7);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.PopulateCurrenciesTransformation(6);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00218342Transformation(5);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00220678Transformation(4);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.ZASubSourceTransformation(3);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.PopulateCountryNamesTransformation(2);
				yield return new CargoWise.RefDbRepo.Staging.DbUpgrader.WI00202963Transformation(1);
				yield break;
			}
		}
	}
}
