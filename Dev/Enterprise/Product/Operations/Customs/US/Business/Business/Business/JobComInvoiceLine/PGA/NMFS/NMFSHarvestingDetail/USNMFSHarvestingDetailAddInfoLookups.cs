//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNMFSHarvestingDetailAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSNMFSHarvestingDetailAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USNMFSHarvestingDetailAddInfoLookups : AutoUSNMFSHarvestingDetailAddInfoLookups
	{
		public USNMFSHarvestingDetailAddInfoLookups(AutoUSNMFSHarvestingDetailAddInfo parent)
			: base(parent)
		{
		}

		protected new USNMFSHarvestingDetailAddInfo Parent
		{
			get { return (USNMFSHarvestingDetailAddInfo)base.Parent; }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public ICodeDescriptionPairList GearTypeList
		{
			get
			{
				ICodeDescriptionPairList result = null;
				if (Parent.Parent is NMFSHarvestingDetail harvestingDetail)
				{
					result = !harvestingDetail.Is370ProgramType ? Factory.GetCachedValue<GearTypeList>() : Enterprise.Customs.US.Business.GearTypeList.GetListFor370Program(Factory);
				}
				return result;
			}
		}

		public ICodeDescriptionPairList OceanAreaCodeList
		{
			get
			{
				ICodeDescriptionPairList result = null;
				if (Parent.Parent is NMFSHarvestingDetail harvestingDetail)
				{
					result = !harvestingDetail.Is370ProgramType ? Factory.GetCachedValue<OceanGeographicAreaCodeList>() : OceanGeographicAreaCodeList.GetListFor370Program(Factory);
				}
				return result;
			}
		}

		public ICodeDescriptionPairList ContactPartyTypes
		{
			get { return EntityRoleCodeList.GetListForNMFSSIM(Factory); }
		}

		public ICodeDescriptionPairList GearDescriptions
		{
			get
			{
				ICodeDescriptionPairList result = null;
				if (Parent.Parent is NMFSHarvestingDetail harvestingDetail)
				{
					result = ProcessingTypeCodeList.GetListForNMFS(Factory, harvestingDetail.Parent?.US_ProgramType ?? ZString.Empty);
				}
				return result;
			}
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public RefVesselCollection RefVessels
		{
			get { return new RefVesselCollection(Factory); }
		}

		public ICodeDescriptionPairList HarvestedMethods
		{
			get { return SourceTypeCodesList.GetListForFishingInformation(Factory); }
		}
	}
}
