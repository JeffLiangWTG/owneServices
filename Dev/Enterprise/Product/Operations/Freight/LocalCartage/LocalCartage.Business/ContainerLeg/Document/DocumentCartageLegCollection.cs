using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class DocumentCartageLegCollection : NonPersistentBusinessObjectCollection<DocumentCartageLeg>
	{
		public DocumentCartageLegCollection(IEnumerable<CommonCartageLeg> cartageLegs, bool groupLegsWithSameInfo = false)
			: base()
		{
			if (groupLegsWithSameInfo)
			{
				foreach (var legs in cartageLegs.GroupBy(p => p, new DocumentCartageComparer()))
				{
					Add(new DocumentCartageLeg(legs.ToArray()));
				}
			}
			else
			{
				foreach (CommonCartageLeg cartageLeg in cartageLegs)
				{
					Add(new DocumentCartageLeg(cartageLeg));
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}

	class DocumentCartageComparer : IEqualityComparer<CommonCartageLeg>
	{
		public bool Equals(CommonCartageLeg leg1, CommonCartageLeg leg2)
		{
			return leg1.JU_PlannedPickupTime == leg2.JU_PlannedPickupTime
						&& leg1.JU_EstimatedDeliveryTime == leg2.JU_EstimatedDeliveryTime
						&& CompareDocAddress(leg1.PickupFromDocAddress, leg2.PickupFromDocAddress)
						&& CompareDocAddress(leg1.WaitPointDocAddress, leg2.WaitPointDocAddress)
						&& CompareDocAddress(leg1.DeliverToDocAddress, leg2.DeliverToDocAddress)
						&& (leg1.BookedCtgMove != null && leg2.BookedCtgMove != null && leg1.BookedCtgMove.EW_DropMode == leg2.BookedCtgMove.EW_DropMode);
		}

		public int GetHashCode(CommonCartageLeg leg)
		{
			int hashJU_PlannedPickupTime = leg.JU_PlannedPickupTime.GetHashCode();
			int hashEstimatedDeliveryTime = leg.JU_EstimatedDeliveryTime.GetHashCode();
			int hashDeliverOrganisation = leg.DeliverToDocAddress == null ? 0 : leg.DeliverToDocAddress.GetHashCode();
			int hashWaitPointOrganisation = leg.WaitPointDocAddress == null ? 0 : leg.WaitPointDocAddress.GetHashCode();
			int hashPickupOrganisation = leg.PickupFromDocAddress == null ? 0 : leg.PickupFromDocAddress.GetHashCode();
			int hashEW_DropMode = leg.BookedCtgMove == null ? 0 : leg.BookedCtgMove.EW_DropMode.GetHashCode();

			return hashJU_PlannedPickupTime ^ hashEstimatedDeliveryTime ^ hashPickupOrganisation ^ hashWaitPointOrganisation ^ hashDeliverOrganisation ^ hashEW_DropMode;
		}

		static bool CompareDocAddress(JobDocAddress address1, JobDocAddress address2)
		{
			return ((address1 == null && address2 == null) || (address1 != null && address2 != null && address1.IsTheSameAddressAs(address2)));
		}
	}
}
