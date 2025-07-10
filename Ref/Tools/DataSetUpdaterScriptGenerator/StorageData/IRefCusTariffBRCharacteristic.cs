using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffBRCharacteristic : IDataSetStorage
	{
		Guid ZB1_PK { get; set; }
		string ZB1_CharacteristicType { get; set; }
		Guid ZB1_ZZ1_Tariff { get; set; }
		Guid ZB1_ZZ5_Nomenclature { get; set; }
		string ZB1_Style { get; set; }
		short ZB1_MaxLength { get; set; }
		short ZB1_DecimalPlaces { get; set; }
		string ZB1_Code { get; set; }
		string ZB1_Text { get; set; }
		DateTime ZB1_StartDate { get; set; }
		DateTime ZB1_EndDate { get; set; }
		bool ZB1_IsImport { get; set; }
		bool ZB1_IsExport { get; set; }
		bool ZB1_IsMandatory { get; set; }
		bool ZB1_IsConditioningAttribute { get; set; }

		IEnumerable<IRefCusTariffBRCharacteristicAttribute> RefCusTariffBRCharacteristicAttribute { get; }
		IEnumerable<IRefCusTariffBRCharacteristicValue> RefCusTariffBRCharacteristicValue { get; }
	}
}
