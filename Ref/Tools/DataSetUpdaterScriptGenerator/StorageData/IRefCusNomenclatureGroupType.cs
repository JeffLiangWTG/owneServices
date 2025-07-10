using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusNomenclatureGroupType : IDataSetStorage
	{
		Guid ZZ9_PK { get; set; }
		string ZZ9_GroupType { get; set; }
		string ZZ9_Description { get; set; }
	}
}
