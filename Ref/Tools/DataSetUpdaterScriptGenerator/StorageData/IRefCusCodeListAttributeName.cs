using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCodeListAttributeName : IDataSetStorage
	{
		Guid ZXE_PK { get; set; }
		string ZXE_Name { get; set; }
		string ZXE_Description { get; set; }
		string ZXE_ZZK_NKCodeType { get; set; }
		string ZXE_ZZZ_NKDataGrouping { get; set; }
		bool ZXE_IsMandatory { get; set; }
		bool ZXE_AllowDuplicates { get; set; }
		bool ZXE_IsValueMandatory { get; set; }
		string ZXE_ZZK_NKCodeTypeForValueList { get; set; }
		string ZXE_ValueDataType { get; set; }
		short ZXE_MinLengthOrValue { get; set; }
		decimal ZXE_MaxLengthOrValue { get; set; }
		byte ZXE_DecimalPlaces { get; set; }
		string ZXE_ColumnCaption { get; set; }
		bool ZXE_IsDateRangeUsed { get; set; }

		IEnumerable<IRefCusCodeListAttributeNameLanguage> RefCusCodeListAttributeNameLanguage { get; }
	}
}
