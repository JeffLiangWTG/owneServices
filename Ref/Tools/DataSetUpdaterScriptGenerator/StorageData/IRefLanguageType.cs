using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefLanguageType : IDataSetStorage
	{
		Guid ZX6_PK { get; set; }
		string ZX6_Language { get; set; }
		string ZX6_Description { get; set; }
	}
}
