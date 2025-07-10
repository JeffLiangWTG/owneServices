using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfileQuestionAttribute : IDataSetStorage
	{
		Guid XQ3_PK { get; set; }
		Guid XQ3_XQ2_Question { get; set; }
		string XQ3_Name { get; set; }
		string XQ3_Value { get; set; }
	}
}
