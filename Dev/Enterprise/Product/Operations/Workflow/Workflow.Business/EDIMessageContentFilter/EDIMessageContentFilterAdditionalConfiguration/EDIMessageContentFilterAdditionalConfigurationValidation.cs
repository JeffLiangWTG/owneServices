using System;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterAdditionalConfigurationValidation : ZValidation
	{
		public EDIMessageContentFilterAdditionalConfigurationValidation(EDIMessageContentFilterAdditionalConfiguration parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected EDIMessageContentFilterAdditionalConfiguration Parent { get; }

		public override Type AutoValidationType => typeof(EDIMessageContentFilterAdditionalConfiguration);

		public override void ValidateAll()
		{
			ValidateFilterContent();
		}

		public void ValidateFilterContent() => ValidateCalculatedProperty(Parent.PrimaryDataSourceInfo);

		protected void CheckPrimaryDataSource()
		{
			if (Parent.PrimaryDataSourceInfo.OriginalValue != null && !Parent.PrimaryDataSourceInfo.OriginalValue.Equals(""))
			{
				ListValidation.ErrorIfInvalidCode(Parent.PrimaryDataSourceInfo);
			}
		}
	}
}
