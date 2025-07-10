//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryHeaderChargesValidation
//
//    This class should be used for overriding validation in AutoCusEntryHeaderChargesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.Customs.Business
{
	public class CusEntryHeaderChargesValidation : AutoCusEntryHeaderChargesValidation
	{
		public CusEntryHeaderChargesValidation(AutoCusEntryHeaderCharges parent) : base(parent)
		{
		}

		protected override void CheckC1_ChargeType()
		{
			base.CheckC1_ChargeType();
			var charge = (CusEntryHeaderCharges)Parent;
			var entryHeader = charge.EntryHeader;
			if (entryHeader != null && !charge.IsConfirmed)
			{
				var charges = entryHeader.Charges.Cast<CusEntryHeaderCharges>();

				var chargeType = charge.C1_ChargeType;
				if (charges.Any(x => x.C1_ChargeType == chargeType && x.PK != charge.PK && !x.C1_IsLandedCostOnly))
				{
					charge.C1_ChargeTypeInfo.AddError(Res.GetString("A69BD4B0-276C-4ECA-A254-D56FD95B504D", "{0} is not unique in this list.", chargeType));
				}
			}
		}
	}
}
