using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfileQuestion : IDataSetStorage
	{
		Guid XQ2_PK { get; set; }
		Guid XQ2_XXX_ProfileType { get; set; }
		string XQ2_QuestionCode { get; set; }
		string XQ2_AnswerDataType { get; set; }
		short XQ2_AnswerMaxLength { get; set; }
		short XQ2_AnswerDecimalPlaces { get; set; }
		string XQ2_AnswerMask { get; set; }
		bool XQ2_AllowMultipleAnswers { get; set; }
		string XQ2_Name { get; set; }
		string XQ2_Text { get; set; }
		string XQ2_Note { get; set; }
		DateTime XQ2_StartDate { get; set; }
		DateTime XQ2_EndDate { get; set; }
		bool XQ2_IsAnswerMandatory { get; set; }
		string XQ2_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusProfileQuestionAnswerList> RefCusProfileQuestionAnswerLists { get; }
		IEnumerable<IRefCusProfileQuestionAttribute> RefCusProfileQuestionAttributes { get; }
		IEnumerable<IRefCusProfileQuestionLanguage> RefCusProfileQuestionLanguages { get; }
		IEnumerable<IRefCusProfileQuestionPathway> RefCusProfileQuestionPathways { get; }
		IEnumerable<IRefCusProfileQuestionPathway> RefCusProfileQuestionPathways1 { get; }
	}
}
