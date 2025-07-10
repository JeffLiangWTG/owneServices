//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessageContentFilterValidation
//
//    This class should be used for overriding validation in AutoEDIMessageContentFilterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterValidation : AutoEDIMessageContentFilterValidation
	{
		public EDIMessageContentFilterValidation(AutoEDIMessageContentFilter parent) : base(parent)
		{
		}

		protected override void CheckECF_FilterType()
		{
			base.CheckECF_FilterType();
			MandatoryValidation.CheckEntered(Parent.ECF_FilterTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ECF_FilterTypeInfo);
		}

		protected override void CheckECF_Name()
		{
			base.CheckECF_Name();
			MandatoryValidation.CheckEntered(Parent.ECF_NameInfo);
		}
	}
}
