using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business.Rating
{
	class HVLVRatingAdapterProvider : RatingAdaptersProvider<ForwardingShipment>, IHVLVRatingAdapterProvider
	{
		public HVLVRatingAdapterProvider(ForwardingShipment parent) : base(parent)
		{
		}

		public override AdaptersProviderOptions AdaptersProviderOptions
		{
			get { return AdaptersProviderOptions.HLSShipment; }
		}

		public override bool IsAdapterAvailable(AutoRateOptions options, ZString chargeCodeGroup, ZString chargeCodeSubGroup)
		{
			switch (chargeCodeGroup)
			{
				case ChargeCodeGroupList.Codes.CFSShipment:
					return false;
				default:
					return true;
			}
		}

		protected override List<IAutoRating> GetAdapters(ForwardingShipment parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = new List<IAutoRating> { new HVLVShipmentRatingAdapter(parent) };

			var consignmentHeader = parent.HVLVConsignmentHeader;
			if (consignmentHeader is HVLVConsignmentHeader header)
			{
				header.Factory.AddFetchHint(HVLVConsignmentSchema.Instance, new ZQuery(HVLVConsignmentSchema.HVC_ClusterKey, header.HCH_ClusterKey));
				header.Factory.AddFetchHint(HVLVItemSchema.Instance, new ZQuery(HVLVItemSchema.HVI_JS_LoadedOnShipment, header.HCH_JS_Shipment));

				var items = parent.HVLVItems.Cast<HVLVItem>();
				var itemAdapters = items
									.Where(item => item.HVI_IsActive)
									.Select(item => new HVLVItemRatingAdapter(item, item.Consignment, parent));

				result.AddRange(itemAdapters);
			}

			return result;
		}
	}
}
