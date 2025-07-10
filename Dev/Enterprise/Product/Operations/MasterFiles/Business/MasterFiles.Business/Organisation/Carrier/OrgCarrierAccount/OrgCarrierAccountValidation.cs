//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCarrierAccountValidation
//
//    This class should be used for overriding validation in AutoOrgCarrierAccountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAccountValidation : AutoOrgCarrierAccountValidation
	{
		public OrgCarrierAccountValidation(AutoOrgCarrierAccount parent) : base(parent)
		{
		}

		protected override void CheckOAN_AccountNumber()
		{
			base.CheckOAN_AccountNumber();
			MandatoryValidation.CheckEntered(Parent.OAN_AccountNumberInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.OAN_AccountNumberInfo, Parent.Factory.Load<OrgCarrierAccount>(new ZQuery(OrgCarrierAccountSchema.OAN_OH_Carrier, Parent.OAN_OH_Carrier)));
		}
	}
}
