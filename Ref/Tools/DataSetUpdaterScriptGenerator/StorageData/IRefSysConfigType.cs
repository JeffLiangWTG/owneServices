using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefSysConfigType : IDataSetStorage
	{
		Guid ZRT_PK { get; set; }
		string ZRT_ConfigCode { get; set; }
		string ZRT_Description { get; set; }
		string ZRT_LongDescription { get; set; }
	}
}
