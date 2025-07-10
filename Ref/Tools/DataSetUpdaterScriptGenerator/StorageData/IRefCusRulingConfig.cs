using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusRulingConfig : IDataSetStorage
	{
		Guid ZZY_PK { get; set; }
		string ZZY_Category { get; set; }
		string ZZY_Type { get; set; }
		decimal ZZY_Rate { get; set; }
		string ZZY_Value { get; set; }
		Guid ZZY_ZZX_CusRuling { get; set; }
	}
}
