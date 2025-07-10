using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefCusProfileQuestionPathwayUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusProfileQuestionPathway",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusProfileQuestionParent", "XQP_XQ2_QuestionParent" },
				{ "RefCusProfileQuestionChild", "XQP_XQ2_QuestionChild" },
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusProfileQuestionParent", questionMapping
				},
				{
					"RefCusProfileQuestionChild", questionMapping
				}
			}
		};

		DataTableMapping questionMapping = new DataTableMapping
		{
			TableName = "#TempRefCusProfileQuestion",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusProfileType", "XQ2_XXX_ProfileType" },
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusProfileType", RefCusProfileTypeMapping.Mapping
				}
			}
		};
	}
}
