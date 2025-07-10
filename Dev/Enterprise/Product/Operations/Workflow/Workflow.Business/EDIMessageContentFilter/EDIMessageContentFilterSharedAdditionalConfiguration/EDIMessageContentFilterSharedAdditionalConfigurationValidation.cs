using System;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterSharedAdditionalConfigurationValidation : ZValidation
	{
		public EDIMessageContentFilterSharedAdditionalConfigurationValidation(EDIMessageContentFilterSharedAdditionalConfiguration parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected EDIMessageContentFilterSharedAdditionalConfiguration Parent { get; }

		public override Type AutoValidationType => typeof(EDIMessageContentFilterSharedAdditionalConfiguration);

		public override void ValidateAll()
		{
		}
	}
}
