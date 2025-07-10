//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobTradeLaneValidation
//
//    This class should be used for overriding validation in AutoJobTradeLaneValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class JobTradeLaneValidation : AutoJobTradeLaneValidation
	{
		public JobTradeLaneValidation(AutoJobTradeLane parent) : base(parent)
		{
		}

		protected override void CheckEJ_Code()
		{
			base.CheckEJ_Code();
			MandatoryValidation.CheckEntered(Parent.EJ_CodeInfo);
		}

		protected override void CheckEJ_Location1()
		{
			base.CheckEJ_Location1();
			MandatoryValidation.CheckEntered(Parent.EJ_Location1Info);
			ListValidation.ErrorIfInvalidCode(Parent.EJ_Location1Info, Parent.Lookups.Locations);
		}

		protected override void CheckEJ_Location2()
		{
			base.CheckEJ_Location2();
			MandatoryValidation.CheckEntered(Parent.EJ_Location2Info);
			ListValidation.ErrorIfInvalidCode(Parent.EJ_Location2Info, Parent.Lookups.Locations);
		}

		protected override void CheckEJ_OH_RelatedOrg()
		{
			base.CheckEJ_OH_RelatedOrg();
			ListValidation.ErrorIfInvalidPK(Parent.EJ_OH_RelatedOrgInfo, Parent.Lookups.RelatedOrgs);
		}
	}
}
