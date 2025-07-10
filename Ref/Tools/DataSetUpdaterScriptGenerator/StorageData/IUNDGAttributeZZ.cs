using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IUNDGAttributeZZ : IDataSetStorage
	{
		Guid DAZ_PK { get; set; }
		string DAZ_Language { get; set; }
		string DAZ_Type { get; set; }
		string DAZ_Index { get; set; }
		string DAZ_Descriptor { get; set; }
		string DAZ_ParentCode { get; set; }
		Guid DAZ_ParentPK { get; set; }
	}
}
