using System.Linq;

namespace Enterprise.TransportBookings.Business
{
	public class DtbMasterBookingVersionChecker
	{
		public DtbMasterBookingVersionChecker(DtbBooking masterBooking)
		{
			MasterBooking = masterBooking;
		}

		public bool IsSubInSyncWithMaster(DtbBooking subBooking)
		{
			if (subBooking.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation != MasterBooking.ConsolidationSingleJob.PK)
			{
				return false;
			}

			if (subBooking.ConsolidationSingleJob.KB_MasterBookingVersion != MasterBooking.ConsolidationSingleJob.KB_MasterBookingVersion)
			{
				return false;
			}

			if (subBooking.KM_KM_MasterBooking != MasterBooking.PK)
			{
				return false;
			}

			if (subBooking.KM_MasterBookingVersion != MasterBooking.KM_MasterBookingVersion)
			{
				return false;
			}

			foreach (var masterInstruction in MasterBooking.Instructions)
			{
				var subInstruction = subBooking.Instructions.FirstOrDefault(i => i.KN_KN_MasterBookingInstruction == masterInstruction.PK);

				if (subInstruction == null)
				{
					return false;
				}

				if (subInstruction.KN_MasterBookingVersion != masterInstruction.KN_MasterBookingVersion)
				{
					return false;
				}

				foreach (var masterConfirmation in masterInstruction.Confirmations)
				{
					var subConfirmation = subInstruction.Confirmations.FirstOrDefault(c => c.KK_KK_MasterBookingConfirmation == masterConfirmation.PK);

					if (subConfirmation == null)
					{
						return false;
					}

					if (subConfirmation.KK_MasterBookingVersion != masterConfirmation.KK_MasterBookingVersion)
					{
						return false;
					}
				}
			}

			return true;
		}

		public DtbBooking MasterBooking { get; }
	}
}
