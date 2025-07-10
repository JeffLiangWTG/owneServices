using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusProfileQuestionAnswerListMapping
	{
		public static DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusProfileQuestionAnswerList",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusProfileQuestionAnswerListLanguages", "XAL_XQ4_QuestionAnswer" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{ "RefCusProfileQuestionAnswerListLanguages", new DataTableMapping { TableName = "#TempRefCusProfileQuestionAnswerListLanguage"} }
			}
		};
	}
}
