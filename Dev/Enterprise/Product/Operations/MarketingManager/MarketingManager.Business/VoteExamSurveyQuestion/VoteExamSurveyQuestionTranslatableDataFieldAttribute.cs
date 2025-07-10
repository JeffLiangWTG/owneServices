using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	sealed class VoteExamSurveyQuestionTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public VoteExamSurveyQuestionTranslatableDataFieldAttribute(string tableName, string columnName, int maxLength, string contextColumnName)
			: base(tableName, columnName, maxLength, contextColumnName)
		{
		}

		public override ZQuery Filter => new ZQuery(VoteExamSurveyQuestionSchema.HY_IsActive, true);
	}
}
