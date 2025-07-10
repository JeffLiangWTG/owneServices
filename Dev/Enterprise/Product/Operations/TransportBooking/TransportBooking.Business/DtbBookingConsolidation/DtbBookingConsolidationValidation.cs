using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using Common = Enterprise.TransportCommon.Business.Common;

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM DtbTransportConsolidationValidation
//
//    This class should be used for overriding validation in DtbTransportConsolidationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationValidation : Common.DtbBookingConsolidationValidation
	{
		public DtbBookingConsolidationValidation(DtbBookingConsolidation parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateCarrierAccountNumbers();
			ValidateTBHasAccessToJobWhenParentJobIsForwardingConsolidation();
		}

		void ValidateCarrierAccountNumbers()
		{
			if (Consolidation?.IsMultiBooking ?? false)
			{
				var carrierOrg = Consolidation?.Address.Organisation;

				var allowMixedCANs = carrierOrg != null && carrierOrg.MiscServ.OM_TBAllowMixedAccountNumbersOnManifest;
				if (!allowMixedCANs)
				{
					var distinctCANCount = Consolidation.Bookings.Select(b => b.KM_OAN_CarrierAccount).Distinct().Count();
					if (distinctCANCount > 1)
					{
						Parent.AddRowError(Res.GetString("56F11D90-B92C-4124-B99B-DF07739BDDAC", "All Bookings must have identical Carrier Account Numbers. This limitation can be removed by allowing Mixed Carrier Account Numbers on Carrier."));
					}
				}
			}
		}

		void ValidateTBHasAccessToJobWhenParentJobIsForwardingConsolidation()
		{
			foreach (var booking in Consolidation.Bookings)
			{
				if ((string)booking.ConsolidationSingleJob?.Parent?.JobType == TransportParentTypes.Consol)
				{
					var consolShipmentProvider = new ConsolShipmentProvider(booking.ConsolidationSingleJob.ParentBO);

					if (consolShipmentProvider.Shipments.Count == 0)
					{
						booking.AddRowWarning(Res.GetString("14BE4862-2B5D-4B0E-8445-56A303B65E3C",
							"Does not have a valid Shipment. Costs cannot be apportioned to this record."));
					}
				}
			}
		}

		protected override void CheckKB_JobDirection()
		{
			base.CheckKB_JobDirection();
			if (Consolidation.IsSub & Consolidation.KB_KB_MasterBookingConsolidationInfo.HasChanges)
			{
				var masterConsolidationDirection = Consolidation.MasterBookingConsolidation.KB_JobDirection;
				var subBookingDirection = Consolidation.KB_JobDirection;
				if (masterConsolidationDirection != subBookingDirection)
				{
					Consolidation.KB_JobDirectionInfo.AddError(Res.GetString("063a0bb0-80b2-4831-8a5a-9e50cde06cb0", "This Consolidation has a different job direction from the Master Consolidation."));
				}
			}
		}

		DtbBookingConsolidation Consolidation
		{
			get { return (DtbBookingConsolidation)Parent; }
		}
	}
}
