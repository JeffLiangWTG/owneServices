//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobContainerDetentionValidation
//
//    This class should be used for overriding validation in AutoJobContainerDetentionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class JobContainerDetentionValidation : AutoJobContainerDetentionValidation
	{
		public JobContainerDetentionValidation(AutoJobContainerDetention parent)
			: base(parent) { }

		protected override void CheckNC_DetentionType()
		{
			base.CheckNC_DetentionType();
			MandatoryValidation.CheckEntered(Parent.NC_DetentionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.NC_DetentionTypeInfo, Parent.Lookups.DetentionTypes);
		}

		protected override void CheckNC_OH_Client()
		{
			base.CheckNC_OH_Client();
			MandatoryValidation.CheckEntered(Parent.NC_OH_ClientInfo);
			ListValidation.ErrorIfInvalidPK(Parent.NC_OH_ClientInfo, Parent.Lookups.Clients);
		}

		protected override void CheckNC_OH_Principal()
		{
			base.CheckNC_OH_Principal();
			MandatoryValidation.CheckEntered(Parent.NC_OH_PrincipalInfo);
			ListValidation.ErrorIfInvalidPK(Parent.NC_OH_PrincipalInfo, Parent.Lookups.Principals);
		}
	}
}


