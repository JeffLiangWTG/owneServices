using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfileQuestionPathway : IDataSetStorage
	{
		Guid XQP_PK { get; set; }
		Guid XQP_XQ2_QuestionParent { get; set; }
		Guid XQP_XQ2_QuestionChild { get; set; }
		string XQP_Description { get; set; }
		DateTime XQP_StartDate { get; set; }
		DateTime XQP_EndDate { get; set; }
		string XQP_ConditionToProceedFormula { get; set; }
		bool XQP_AllowMultipleAnswers { get; set; }
		bool XQP_IsAnswerMandatory { get; set; }
	}
}
