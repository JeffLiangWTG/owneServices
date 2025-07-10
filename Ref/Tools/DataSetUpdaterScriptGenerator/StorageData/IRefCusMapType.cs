using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusMapType : IDataSetStorage
	{
		Guid ZZP_PK { get; set; }
		string ZZP_MapType { get; set; }
		string ZZP_Direction { get; set; }
		string ZZP_Description { get; set; }
		bool ZZP_IsReadonly { get; set; }
	}
}
