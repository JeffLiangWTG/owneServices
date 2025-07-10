using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefAccessorial : IDataSetStorage
	{
		Guid ASI_PK { get; set; }
		string ASI_Code { get; set; }
		string ASI_Description { get; set; }
	}
}
