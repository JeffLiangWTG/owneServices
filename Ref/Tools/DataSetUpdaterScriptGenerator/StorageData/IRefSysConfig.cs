using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefSysConfig : IDataSetStorage
	{
		Guid ZRC_PK { get; set; }
		string ZRC_ZRT_NKConfigCode { get; set; }
		decimal ZRC_DecimalValue { get; set; }
		string ZRC_StringValue { get; set; }
		bool ZRC_BitValue { get; set; }
		DateTime ZRC_StartDate { get; set; }
		Nullable<DateTime> ZRC_EndDate { get; set; }
		byte[] ZRC_BinaryValue { get; set; }
	}
}
