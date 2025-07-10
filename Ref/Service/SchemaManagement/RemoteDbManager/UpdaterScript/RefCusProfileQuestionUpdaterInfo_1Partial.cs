using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusProfileQuestionUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusProfileQuestion",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusProfileType", "XQ2_XXX_ProfileType" },
				{ "RefCusProfileQuestionAnswerLists", "XQ4_XQ2_Question" },
				{ "RefCusProfileQuestionAttributes", "XQ3_XQ2_Question" },
				{ "RefCusProfileQuestionLanguages", "XQL_XQ2_Question" },
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusProfileType", RefCusProfileTypeMapping.Mapping
				},
				{
					"RefCusProfileQuestionAnswerLists",  RefCusProfileQuestionAnswerListMapping.Mapping
				},
				{
					"RefCusProfileQuestionAttributes", new DataTableMapping { TableName = "#TempRefCusProfileQuestionAttribute" }
				},
				{
					"RefCusProfileQuestionLanguages", new DataTableMapping { TableName = "#TempRefCusProfileQuestionLanguage" }
				}
			}
		};
	}
}
