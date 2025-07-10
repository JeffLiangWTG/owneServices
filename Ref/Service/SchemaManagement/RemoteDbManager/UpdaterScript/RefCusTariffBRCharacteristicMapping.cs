using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusTariffBRCharacteristicMapping
	{
		public static DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusTariffBRCharacteristic",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTariffBRCharacteristicAttributes", "ZB3_ZB1_Characteristic" },
				{ "RefCusTariffBRCharacteristicValues", "ZB2_ZB1_Characteristic" },

			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTariffBRCharacteristicAttributes", new DataTableMapping { TableName = "#TempRefCusTariffBRCharacteristicAttribute" }
				},
				{
					"RefCusTariffBRCharacteristicValues", new DataTableMapping { TableName = "#TempRefCusTariffBRCharacteristicValue" }
				}
			}

		};

	}
}
