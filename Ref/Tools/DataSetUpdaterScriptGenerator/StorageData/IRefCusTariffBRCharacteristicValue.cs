using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffBRCharacteristicValue : IDataSetStorage
	{
		Guid ZB2_PK { get; set; }
		Guid ZB2_ZB1_Characteristic { get; set; }
		string ZB2_Value { get; set; }
		string ZB2_Description { get; set; }	
	}
}
