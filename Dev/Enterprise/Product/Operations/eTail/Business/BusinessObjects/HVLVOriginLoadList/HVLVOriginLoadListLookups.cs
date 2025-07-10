//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVOriginLoadListLookups
//
//    This class should be used for overriding collections in AutoHVLVOriginLoadListLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Definitions.Ecommerce;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public class HVLVOriginLoadListLookups : AutoHVLVOriginLoadListLookups
	{
		public HVLVOriginLoadListLookups(AutoHVLVOriginLoadList parent) : base(parent)
		{
		}

		public CodeDescriptionPairList HVL_Status_List => Factory.GetCachedValue("ETail|HVL_Status_List", () =>
		{
			var result = new CodeDescriptionPairList();
			result.Add(new CodeDescriptionPair(HVLVOriginLoadListStatus.Codes.Open, ResString.GetMultilingualString("0c142aaf-f428-493e-8e91-e8f338ad2403", HVLVOriginLoadListStatus.Descriptions.Open)));
			result.Add(new CodeDescriptionPair(HVLVOriginLoadListStatus.Codes.Pending, ResString.GetMultilingualString("cc3f1b32-411c-4436-9fa5-c26df491d7e9", HVLVOriginLoadListStatus.Descriptions.Pending)));
			result.Add(new CodeDescriptionPair(HVLVOriginLoadListStatus.Codes.Closed, ResString.GetMultilingualString("ec291ef5-ae06-4868-9659-f68cf428515e", HVLVOriginLoadListStatus.Descriptions.Closed)));
			result.Add(new CodeDescriptionPair(HVLVOriginLoadListStatus.Codes.Consolidated, ResString.GetMultilingualString("33c9233c-e84e-4d13-a96d-f10c6679db3c", HVLVOriginLoadListStatus.Descriptions.Consolidated)));
			result.Add(new CodeDescriptionPair(HVLVOriginLoadListStatus.Codes.Lodged, ResString.GetMultilingualString("b46f9bb1-396a-455f-b103-a665d36b39a1", HVLVOriginLoadListStatus.Descriptions.Lodged)));
			result.Add(new CodeDescriptionPair(HVLVOriginLoadListStatus.Codes.Failed, ResString.GetMultilingualString("a50dbc86-d412-4056-8e67-eba33196e281", HVLVOriginLoadListStatus.Descriptions.Failed)));
			return result;
		});

		public CodeDescriptionPairList INCOTermsList
		{
			get { return Factory.GetCachedValue("HVLVOriginLoadListLookups.INCOTermsList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)); }
		}

		public OrganisationsFindBoxCollection HVL_OA_OriginDepot_List => originDepotList ?? (originDepotList = new PackDepotCollection(Factory));
		OrganisationsFindBoxCollection originDepotList;

		public OrganisationsFindBoxCollection HVL_OA_DestinationDepot_List => destinationDepotList ?? (destinationDepotList = new UnpackDepotCollection(Factory));
		OrganisationsFindBoxCollection destinationDepotList;

		public override OrgHeaderCollection Carriers => new CarrierCollection(Factory);

		public CodeDescriptionPairList HVL_TransportMode_List => new CodeDescriptionPairList(OLookUpEditType.TransportType);

		public OrganisationsFindBoxCollection OriginCTO_List => originCTOCollection ?? (originCTOCollection = new CTOCollection(Factory));
		OrganisationsFindBoxCollection originCTOCollection;

		public RefVesselCollection Vessels => new RefVesselCollection(Factory);
	}
}
