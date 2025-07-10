using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLDescriptorPivotValidation : AutoAccGLDescriptorPivotValidation
	{
		public AccGLDescriptorPivotValidation(AutoAccGLDescriptorPivot parent)
			: base(parent)
		{ }
		public new AccGLDescriptorPivot Parent
		{
			get { return (AccGLDescriptorPivot)base.Parent; }
		}

		protected override void CheckYJ_AG()
		{
			base.CheckYJ_AG();
			if (Parent.GLAccountDescriptor != null &&
					(Parent.GLAccountDescriptor.AJ_ReportCategory == AccountTypeComboBoxConstants.ProfitAndLossAccount ||
						Parent.GLAccountDescriptor.AJ_ReportCategory == AccountTypeComboBoxConstants.BalanceSheetAccount)
				)
			{
				if (Parent.YJ_AG.IsEmpty)
				{
					Parent.YJ_AGInfo.AddError(Res.GetString("4c6458eb-2b01-4197-aa81-4e982dee08a3", "This descriptor must reference a global GL account"));
				}
			}
			if (Parent.GLAccountDescriptor != null && !Parent.YJ_AG.IsEmpty && Parent.YJ_AG.IsValid && !Parent.GLAccountDescriptor.AJ_ReportCategory.IsEmpty)
			{
				ValidateParentAccountNotTheSame();
			}
		}

		protected void ValidateParentAccountNotTheSame()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_Language, SQLComparisonOperator.Equal, Parent.GLAccountDescriptor.AJ_Language);
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, SQLComparisonOperator.Equal, Parent.GLAccountDescriptor.AJ_RN_NKCountryOfCompliance);
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.PK, SQLComparisonOperator.NotEqual, Parent.GLAccountDescriptor.PK);
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_ReportType, SQLComparisonOperator.Equal, Parent.GLAccountDescriptor.AJ_ReportType);
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_ReportCategory, SQLComparisonOperator.Equal, Parent.GLAccountDescriptor.AJ_ReportCategory);
			ZDBOnlySubQuery accGLDescriptorPivotSubQuery = new ZDBOnlySubQuery(typeof(AutoAccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AJ);

			accGLDescriptorPivotSubQuery.AddToFilter(AccGLDescriptorPivotSchema.YJ_AG, Parent.YJ_AG);
			query.AddSubQuery(AccGLAccountDescriptorSchema.PK, accGLDescriptorPivotSubQuery, JoinCondition.And);

			AccGLAccountDescriptor nonUniqueDescriptor = (AccGLAccountDescriptor)Parent.Factory.LoadTop1(typeof(AccGLAccountDescriptor), query);

			if (nonUniqueDescriptor != null)
			{
				Parent.YJ_AGInfo.AddError(Res.GetString("4c2f3fbb-5d83-4101-9056-46e7dc69af49", "Please choose another GL Account as this one is already referenced by another Local Account for the current language."));
			}
		}
	}
}
