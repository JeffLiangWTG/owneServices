using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfile : IDataSetStorage
	{
		Guid XX0_PK { get; set; }
		Guid XX0_XXX_ProfileType { get; set; }
		string XX0_AppliesToCode { get; set; }
		string XX0_QuestionCode { get; set; }
		DateTime XX0_StartDate { get; set; }
		DateTime XX0_EndDate { get; set; }
		string XX0_ZZZ_NKDataGrouping { get; set; }
		bool XX0_AllowMultipleAnswers { get; set; }
		bool XX0_IsAnswerMandatory { get; set; }

		IEnumerable<IRefCusProfileAttribute> RefCusProfileAttributes { get; }
	}
}
