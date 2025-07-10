using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfileAttribute : IDataSetStorage
	{
		Guid XXY_PK { get; set; }
		Guid XXY_XX0_Profile { get; set; }
		string XXY_Name { get; set; }
		string XXY_Value { get; set; }

	}
}
