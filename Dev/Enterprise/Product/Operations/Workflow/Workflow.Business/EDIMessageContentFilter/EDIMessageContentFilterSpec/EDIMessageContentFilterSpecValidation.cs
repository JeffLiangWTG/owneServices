using System;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterSpecValidation : ZValidation
	{
		public EDIMessageContentFilterSpecValidation(EDIMessageContentFilterSpec parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected EDIMessageContentFilterSpec Parent { get; }

		public override Type AutoValidationType => typeof(EDIMessageContentFilterSpec);

		public override void ValidateAll()
		{
			ValidateFilterType();
		}

		public void ValidateFilterType() => ValidateCalculatedProperty(Parent.FilterTypeInfo);

		protected void CheckFilterType()
		{
			MandatoryValidation.CheckEntered(Parent.FilterTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FilterTypeInfo);
		}
	}
}
