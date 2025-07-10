using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfileQuestionLanguage : IDataSetStorage
	{
		Guid XQL_PK { get; set; }
		Guid XQL_XQ2_Question { get; set; }
		string XQL_Name { get; set; }
		string XQL_Text { get; set; }
		string XQL_Note { get; set; }
		string XQL_ZX6_NKLanguage { get; set; }
	}
}
