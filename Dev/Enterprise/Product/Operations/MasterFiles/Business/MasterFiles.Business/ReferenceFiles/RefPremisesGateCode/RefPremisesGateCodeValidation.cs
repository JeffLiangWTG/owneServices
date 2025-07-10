//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefPremisesGateCodeValidation
//
//    This class should be used for overriding validation in AutoRefPremisesGateCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefPremisesGateCodeValidation : AutoRefPremisesGateCodeValidation
	{
		public RefPremisesGateCodeValidation(AutoRefPremisesGateCode parent)
			: base(parent)
		{
		}

		protected override void CheckR5_OrgRegCodeType()
		{
			base.CheckR5_OrgRegCodeType();
			MandatoryValidation.CheckEntered(base.Parent.R5_OrgRegCodeTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.R5_OrgRegCodeTypeInfo);
		}

		protected override void CheckR5_DataProvider()
		{
			base.CheckR5_DataProvider();
			MandatoryValidation.CheckEntered(base.Parent.R5_DataProviderInfo);
			ListValidation.ErrorIfInvalidCode(Parent.R5_DataProviderInfo);
		}

		protected override void CheckR5_PremisesGateCode()
		{
			base.CheckR5_PremisesGateCode();
			MandatoryValidation.CheckEntered(base.Parent.R5_PremisesGateCodeInfo);
		}
		protected override void CheckR5_PremisesGateDescription()
		{
			base.CheckR5_PremisesGateDescription();
			MandatoryValidation.CheckEntered(base.Parent.R5_PremisesGateDescriptionInfo);
		}
	}
}
