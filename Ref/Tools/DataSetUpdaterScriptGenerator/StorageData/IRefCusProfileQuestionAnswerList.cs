using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfileQuestionAnswerList : IDataSetStorage
	{
		Guid XQ4_PK { get; set; }
		Guid XQ4_XQ2_Question { get; set; }
		string XQ4_Value { get; set; }
		string XQ4_Description { get; set; }

		IEnumerable<IRefCusProfileQuestionAnswerListLanguage> RefCusProfileQuestionAnswerListLanguages { get; }
	}
}
