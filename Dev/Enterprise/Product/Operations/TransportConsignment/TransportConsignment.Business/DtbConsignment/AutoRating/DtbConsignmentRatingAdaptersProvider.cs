using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentRatingAdaptersProvider : RatingAdaptersProvider<DtbConsignment>
	{
		public DtbConsignmentRatingAdaptersProvider(DtbConsignment consignment)
			: base(consignment)
		{
		}

		#region GetAdapters

		protected override List<IAutoRating> GetAdapters(DtbConsignment parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var list = new List<IAutoRating>();

			var picAddress = parent.Addresses.FirstOrDefault(i => i.LTS_InstructionType == ConsignmentAddressTypes.Codes.PickUp);
			var dlvAddress = parent.Addresses.FirstOrDefault(i => i.LTS_InstructionType == ConsignmentAddressTypes.Codes.Delivery);
			if (picAddress != null && dlvAddress != null)
			{
				if (parent.Containers.Any())
				{
					list.Add(new DtbConsignmentRatingAdapter(parent, FreightMode.FRO));
				}

				if (!list.Any() || parent.LoosePackages.Any())
				{
					list.Add(new DtbConsignmentRatingAdapter(parent, FreightMode.LRO));
				}
			}
			else
			{
				uiInteractor.Warning(LogMessages.RatingAdaptersCannotBeCreated((NoResString)"No pickup/delivery addresses found")); // Log messages are english only for now
			}

			return list;
		}

		#endregion
	}
}
