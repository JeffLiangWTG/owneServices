//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVBookingHeaderValidation
//
//    This class should be used for overriding validation in AutoHVLVBookingHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.eTail.Business
{
	public class HVLVBookingHeaderValidation : AutoHVLVBookingHeaderValidation
	{
		public HVLVBookingHeaderValidation(AutoHVLVBookingHeader parent)
			: base(parent)
		{ }

		new HVLVBookingHeader Parent
		{
			get { return (HVLVBookingHeader)base.Parent; }
		}

		protected override void CheckHVH_IsBookingReceived()
		{
			base.CheckHVH_IsBookingReceived();
			if (Parent.HVH_IsBookingReceived && !Parent.HVH_IsBookingConfirmed)
			{
				Parent.HVH_IsBookingReceivedInfo.AddError(Res.GetString("2899c345-b631-4ec4-bcd2-88c246ad7e65", "Please confirm the Booking Header before receival."));
			}
		}

		protected override void CheckHVH_RS_NKBookingServiceLevel()
		{
			base.CheckHVH_RS_NKBookingServiceLevel();
			MandatoryValidation.CheckEntered(Parent.HVH_RS_NKBookingServiceLevelInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HVH_RS_NKBookingServiceLevelInfo, Parent.Lookups.BookingServiceLevels);
		}

		protected override void CheckHVH_OA_DispatchAddress()
		{
			base.CheckHVH_OA_DispatchAddress();
			MandatoryValidation.CheckEntered(Parent.HVH_OA_DispatchAddressInfo);
		}

		protected override void CheckHVH_IsActive()
		{
			base.CheckHVH_IsActive();
			if (!Parent.HVH_IsActive && Parent.Consignments.OfType<HVLVConsignment>().Any(consignment => consignment.HVC_IsActive))
			{
				Parent.HVH_IsActiveInfo.AddError(Parent.DeactivateErrorMessageActiveConsignment);
			}

			if (!Parent.HVH_IsActive && Parent.HVH_IsProcessedAtOriginDepot)
			{
				Parent.HVH_IsActiveInfo.AddError(Parent.DeactivateErrorMessageProcessed);
			}
		}

		protected override void CheckHVH_GrossVolumeUQ()
		{
			base.CheckHVH_GrossVolumeUQ();
			MandatoryValidation.CheckEntered(Parent.HVH_GrossVolumeUQInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HVH_GrossVolumeUQInfo);
		}

		protected override void CheckHVH_GrossWeightUQ()
		{
			base.CheckHVH_GrossWeightUQ();
			MandatoryValidation.CheckEntered(Parent.HVH_GrossWeightUQInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HVH_GrossWeightUQInfo);
		}
	}
}
