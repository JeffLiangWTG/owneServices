using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfileQuestionAnswerListLanguage : IDataSetStorage
	{
		Guid XAL_PK { get; set; }
		Guid XAL_XQ4_QuestionAnswer { get; set; }
		string XAL_Description { get; set; }
		string XAL_ZX6_NKLanguage { get; set; }
	}
}
