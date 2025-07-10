//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSalesValueAssociationPivotValidation
//
//    This class should be used for overriding validation in AutoOrgSalesValueAssociationPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesValueAssociationPivotValidation : AutoOrgSalesValueAssociationPivotValidation
	{
		public OrgSalesValueAssociationPivotValidation(AutoOrgSalesValueAssociationPivot parent) : base(parent)
		{
		}
	}
}
