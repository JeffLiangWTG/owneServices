//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM Autovw_OrgCollectionCallValidation
//
//    This class should be used for overriding validation in Autovw_OrgCollectionCallValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class vw_OrgCollectionCallValidation : Autovw_OrgCollectionCallValidation
	{
		public vw_OrgCollectionCallValidation(Autovw_OrgCollectionCall parent) : base(parent)
		{
		}
	}
}
