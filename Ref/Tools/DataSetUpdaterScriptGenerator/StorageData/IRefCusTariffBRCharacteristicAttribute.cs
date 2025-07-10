using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffBRCharacteristicAttribute : IDataSetStorage
	{
		Guid ZB3_PK { get; set; }
		Guid ZB3_ZB1_Characteristic { get; set; }
		string ZB3_Name { get; set; }
		string ZB3_Code { get; set; }
		string ZB3_Value { get; set; }
	}
}
