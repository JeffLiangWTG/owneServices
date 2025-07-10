using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ExternalRequestInfoTemplateValidation : AutoExternalRequestInfoTemplateValidation
	{
		public ExternalRequestInfoTemplateValidation(AutoExternalRequestInfoTemplate parent) : base(parent)
		{
		}

		protected override void CheckRIT_Code()
		{
			MandatoryValidation.CheckEntered(Parent.RIT_CodeInfo);
			ValidateCodeUniqueness();

			base.CheckRIT_Code();
		}

		void ValidateCodeUniqueness()
		{
			if (!Parent.RIT_Code.IsEmpty)
			{
				var matched = Parent.Factory.Exists(typeof(ExternalRequestInfoTemplate), new ZQuery(ExternalRequestInfoTemplateSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK).AddToFilter(ExternalRequestInfoTemplateSchema.RIT_Code, Parent.RIT_Code));
				if (matched)
				{
					Parent.RIT_CodeInfo.AddError(Res.GetString("6cfaccd5-2cf9-49c9-b5d2-df86df1d1f15", @"Type Code {0} already exists. Specify a different Type Code.", Parent.RIT_Code));
				}
			}
		}

		protected override void CheckRIT_Description()
		{
			MandatoryValidation.CheckEntered(Parent.RIT_DescriptionInfo);

			base.CheckRIT_Description();
		}

		protected override void CheckRIT_JobType()
		{
			MandatoryValidation.CheckEntered(Parent.RIT_JobTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RIT_JobTypeInfo, Parent.Lookups.JobTypeList);

			base.CheckRIT_JobType();
		}
	}
}
