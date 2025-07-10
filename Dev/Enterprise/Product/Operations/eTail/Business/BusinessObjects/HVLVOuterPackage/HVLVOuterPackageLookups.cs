//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVOuterPackageLookups
//
//    This class should be used for overriding collections in AutoHVLVOuterPackageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Definitions.Ecommerce;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public class HVLVOuterPackageLookups : AutoHVLVOuterPackageLookups
	{
		public HVLVOuterPackageLookups(AutoHVLVOuterPackage parent)
			: base(parent)
		{
		}

		public HVLVOriginLoadListCollection HVO_HVL_LoadList_List
		{
			get
			{
				return new HVLVOriginLoadListCollection(Factory);
			}
		}

		public static CodeDescriptionPairList GetAllHVLVOuterPackageStatus()
		{
			var result = new CodeDescriptionPairList();
			result.Add(new CodeDescriptionPair(HVLVOuterPackageStatus.Codes.Open, ResString.GetMultilingualString("97332d9f-e71c-47ac-b2ba-01d2440a59d8", HVLVOuterPackageStatus.Descriptions.Open)));
			result.Add(new CodeDescriptionPair(HVLVOuterPackageStatus.Codes.Closed, ResString.GetMultilingualString("ef6bbbdd-32a7-4e25-a6c2-ffd945201204", HVLVOuterPackageStatus.Descriptions.Closed)));
			result.Add(new CodeDescriptionPair(HVLVOuterPackageStatus.Codes.Consolidated, ResString.GetMultilingualString("7ea7bd17-8523-4e2f-96c0-adf32b16b451", HVLVOuterPackageStatus.Descriptions.Consolidated)));
			result.Add(new CodeDescriptionPair(HVLVOuterPackageStatus.Codes.Lodged, ResString.GetMultilingualString("414373dd-63ce-4853-bffb-e656b579fe6a", HVLVOuterPackageStatus.Descriptions.Lodged)));
			return result;
		}

		public CodeDescriptionPairList HVO_VolumeUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList HVO_WeightUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList HVO_UnitOfDimensionList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		public OrgCarrierServiceLevelCollection HVO_PL_NKLastMileCarrierServiceLevel_List
		{
			get
			{
				var carrierServiceLevels = (Parent.LastMileCarrier != null && (bool)Parent.LastMileCarrier.OH_IsShippingProvider) ?
					new OrgCarrierServiceLevelCollection(Parent.LastMileCarrier.MiscServ) :
					new OrgCarrierServiceLevelCollection(Factory, includeAllServiceLevel: false, includeStdServiceLevel: false);
				carrierServiceLevels.Load();
				return carrierServiceLevels;
			}
		}

		public new HVLVOuterPackage Parent
		{
			get { return (HVLVOuterPackage)base.Parent; }
		}
	}
}
