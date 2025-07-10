using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business
{
	class DtbBookingConsignmentRatingAdaptersProvider : RatingAdaptersProvider<DtbBookingConsignment>
	{
		public DtbBookingConsignmentRatingAdaptersProvider(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		#region GetAdapters

		protected override List<IAutoRating> GetAdapters(DtbBookingConsignment parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var list = new List<IAutoRating>();

			var picInstruction = parent.Instructions.FirstOrDefault(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
			var dlvInstruction = parent.Instructions.FirstOrDefault(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery);
			if (picInstruction != null && dlvInstruction != null)
			{
				if (parent.IsContainerised)
				{
					list.Add(new DtbBookingConsignmentRatingAdapter(parent, FreightMode.FRO));
				}

				if (!list.Any() || parent.IsLoose)
				{
					list.Add(new DtbBookingConsignmentRatingAdapter(parent, FreightMode.LRO));
				}
			}
			else
			{
				uiInteractor.Warning(LogMessages.RatingAdaptersCannotBeCreated((NoResString)"No pickup/delivery instructions found")); // Log messages are english only for now
			}

			return list;
		}

		#endregion
	}
}
