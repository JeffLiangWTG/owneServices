using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateBookingDetailLookups : AutoGateBookingDetailLookups
	{
		public GateBookingDetailLookups(AutoGateBookingDetail parent)
			: base(parent)
		{
		}

		#region GTD_VolumeUQ_List

		public CodeDescriptionPairList GTD_VolumeUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region GTD_WeightUQ_List

		public CodeDescriptionPairList GTD_WeightUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region CommodityCode_List

		public RefCommodityCodeCollection CommodityCode_List
			=> commodityCode_List ?? (commodityCode_List = new RefCommodityCodeCollection(Factory));
		RefCommodityCodeCollection commodityCode_List;

		#endregion

		#region FacilityJobType_List

		public CodeDescriptionPairList FacilityJobType_List =>
			Factory.GetCachedValue("GateBookingDetail.Lookups.FacilityJobType_List",
				delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.FacilityJobType.Codes.Cargo, Core.Constants.FacilityJobType.Descriptions.Cargo);
					result.AddPair(Core.Constants.FacilityJobType.Codes.Container, Core.Constants.FacilityJobType.Descriptions.Container);
					return result;
				});

		#endregion
	}
}
