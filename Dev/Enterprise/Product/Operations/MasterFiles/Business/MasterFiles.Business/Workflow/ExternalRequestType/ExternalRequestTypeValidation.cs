using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ExternalRequestTypeValidation : AutoExternalRequestTypeValidation
	{
		public ExternalRequestTypeValidation(AutoExternalRequestType parent) : base(parent)
		{
		}

		protected override void CheckRQT_Code()
		{
			MandatoryValidation.CheckEntered(Parent.RQT_CodeInfo);
			ValidateCodeUniqueness();

			base.CheckRQT_Code();
		}

		void ValidateCodeUniqueness()
		{
			if (!Parent.RQT_Code.IsEmpty)
			{
				var matched = Parent.Factory.Exists(typeof(ExternalRequestType), new ZQuery(ExternalRequestTypeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK).AddToFilter(ExternalRequestTypeSchema.RQT_Code, Parent.RQT_Code));
				if (matched)
				{
					Parent.RQT_CodeInfo.AddError(Res.GetString("E2088DC3-0FFD-438E-9323-5F0EF6666224", @"Type Code {0} already exists. Specify a different Type Code.", Parent.RQT_Code));
				}
			}
		}

		protected override void CheckRQT_Description()
		{
			MandatoryValidation.CheckEntered(Parent.RQT_DescriptionInfo);

			base.CheckRQT_Description();
		}

		protected override void CheckRQT_JobType()
		{
			if (!Parent.RQT_JobType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.RQT_JobTypeInfo, Parent.Lookups.JobTypeList);
			}

			base.CheckRQT_JobType();
		}

		protected override void CheckRQT_RIT_Template()
		{
			MandatoryValidation.CheckEntered(Parent.RQT_RIT_TemplateInfo);

			if (!Parent.RQT_RIT_Template.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.RQT_RIT_TemplateInfo);
			}

			base.CheckRQT_RIT_Template();
		}

		protected override void CheckRQT_RequiredInDays()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.RQT_RequiredInDaysInfo, 0);

			base.CheckRQT_RequiredInDays();
		}

		protected override void CheckRQT_Reviewer()
		{
			if (!Parent.RQT_Reviewer.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.RQT_ReviewerInfo, Parent.Lookups.ReviewerAddressTypes);
			}

			base.CheckRQT_Reviewer();
		}

		protected override void CheckRQT_Assignee()
		{
			if (!Parent.RQT_Assignee.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.RQT_AssigneeInfo, Parent.Lookups.AssigneeAddressTypes);
			}

			base.CheckRQT_Assignee();
		}
	}
}
