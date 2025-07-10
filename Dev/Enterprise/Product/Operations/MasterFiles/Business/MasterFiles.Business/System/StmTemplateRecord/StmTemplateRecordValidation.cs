using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmTemplateRecordValidation : AutoStmTemplateRecordValidation
	{
		public StmTemplateRecordValidation(AutoStmTemplateRecord parent) : base(parent)
		{
			templateRecord = parent;
		}

		readonly AutoStmTemplateRecord templateRecord;

		protected override void CheckSTR_TemplateName()
		{
			base.CheckSTR_TemplateName();

			if (templateRecord.STR_TemplateName != "" && TemplateNameIsNotUnique())
			{
				templateRecord.STR_TemplateNameInfo.AddError(
					Res.GetString("073e41a5-e260-4954-86ed-e5d62ba63520", "An active template record with the same template name already exists in current scope.")
				);
			}
		}

		public bool TemplateNameIsNotUnique()
		{
			var query = new ZDBOnlyQuery(typeof(StmTemplateRecord));
			query.AddToFilter(StmTemplateRecordSchema.STR_TemplateName, templateRecord.STR_TemplateName);
			query.AddToFilter(StmTemplateRecordSchema.STR_IsActive, true);
			query.AddToFilter(StmTemplateRecordSchema.STR_ModuleID, templateRecord.STR_ModuleID);
			query.AddToFilter(StmTemplateRecordSchema.PK, SQLComparisonOperator.NotEqual, templateRecord.PK);

			return Parent.Factory.Exists(typeof(StmTemplateRecord), query);
		}
	}
}
