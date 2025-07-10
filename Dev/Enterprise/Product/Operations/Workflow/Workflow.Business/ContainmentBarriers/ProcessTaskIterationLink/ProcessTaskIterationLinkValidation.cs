using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class ProcessTaskIterationLinkValidation : AutoProcessTaskIterationLinkValidation
	{
		public ProcessTaskIterationLinkValidation(AutoProcessTaskIterationLink parent)
			: base(parent)
		{
		}

		protected override void CheckP9I_LinkType()
		{
			base.CheckP9I_LinkType();

			MandatoryValidation.CheckEntered(Parent.P9I_LinkTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.P9I_LinkTypeInfo);
		}
	}
}
