//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmFeatureTestValidation
//
//    This class should be used for overriding validation in AutoStmFeatureTestValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmFeatureTestValidation : AutoStmFeatureTestValidation
	{
		public StmFeatureTestValidation(AutoStmFeatureTest parent) : base(parent)
		{
		}

		protected override void CheckSFT_FeatureName()
		{
			MandatoryValidation.CheckEntered(Parent.SFT_FeatureNameInfo);
			base.CheckSFT_FeatureName();
		}
	}
}
