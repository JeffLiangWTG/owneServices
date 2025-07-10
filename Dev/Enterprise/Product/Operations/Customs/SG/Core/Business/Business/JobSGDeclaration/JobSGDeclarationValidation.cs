//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobSGDeclarationValidation
//
//    This class should be used for overriding validation in AutoJobSGDeclarationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobSGDeclarationValidation : AutoJobSGDeclarationValidation
	{
		public JobSGDeclarationValidation(AutoJobSGDeclaration parent) : base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Parent; }
		}

		protected new JobSGDeclaration Parent
		{
			get { return (JobSGDeclaration)base.Parent; }
		}

		protected override void CheckSGE_RN_NKOutwardVesselNationality()
		{
			base.CheckSGE_RN_NKOutwardVesselNationality();
			if (Declaration.IsSeaStore && Declaration.HasLiquorOrTobacco && Parent.SGE_RN_NKOutwardVesselNationality.IsEmpty)
			{
				Parent.SGE_RN_NKOutwardVesselNationalityInfo.AddMessageError("Please enter the outward Vessel Nationality. Specify the nationality of the Outward Vessel for seastore permits application if goods are liquor/tobacco product.");
			}
		}
	}
}
