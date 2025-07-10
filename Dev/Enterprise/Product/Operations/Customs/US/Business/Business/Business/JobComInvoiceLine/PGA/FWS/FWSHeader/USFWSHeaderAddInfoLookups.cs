//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFWSHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSFWSHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USFWSHeaderAddInfoLookups : AutoUSFWSHeaderAddInfoLookups
	{
		public USFWSHeaderAddInfoLookups(AutoUSFWSHeaderAddInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList GetCachedUSStateList
		{
			get { return Factory.GetCachedValue("USStateList", delegate { return new OrgCodeLists().State_List(Factory, Core.Constants.CountryCodes.UnitedStates); }); }
		}

		public FWSIntendedUseCodesList IntendedUseCodeList
		{
			get { return Factory.GetCachedValue<FWSIntendedUseCodesList>(); }
		}

		public FWSProcessingCodeList ProcessingCodes
		{
			get { return Factory.GetCachedValue<FWSProcessingCodeList>(); }
		}

		public GlobalUniqueProductCodeQualifierList ProductTypes
		{
			get { return Factory.GetCachedValue<GlobalUniqueProductCodeQualifierList>(); }
		}

		public RefCountryCollection SpeciesOrigins
		{
			get { return new RefCountryCollection(Factory); }
		}

		public OceanGeographicAreaCodeList HighSeaAreas
		{
			get { return Factory.GetCachedValue<OceanGeographicAreaCodeList>(); }
		}

		public ICodeDescriptionPairList IdentityTypes
		{
			get { return ItemIdentityNumberQualifierList.GetListForFWS(Factory); }
		}

		public FWSHybridTypeList HybridTypes
		{
			get { return Factory.GetCachedValue<FWSHybridTypeList>(); }
		}

		public FWSWildlifeCategoryCodesList WildlifeCategoryCodes
		{
			get { return Factory.GetCachedValue<FWSWildlifeCategoryCodesList>(); }
		}

		public CodeDescriptionPairList YesNoList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(Factory); }
		}

		public FWSWildlifeDescriptionCodesList WildlifeDescriptionCodes
		{
			get { return Factory.GetCachedValue<FWSWildlifeDescriptionCodesList>(); }
		}

		public ICodeDescriptionPairList WildlifeSources
		{
			get { return FWSWildlifeSourceList.GetList(Factory, Header.IsExport); }
		}

		public FWSUnitOfMeasureList UnitOfMeasureList
		{
			get { return Factory.GetCachedValue<FWSUnitOfMeasureList>(); }
		}

		public ZZRefCusCodeListCombinedCollection FIRMSList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public OrganisationsFindBoxCollection Organisations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		protected new USFWSHeaderAddInfo Parent
		{
			get { return (USFWSHeaderAddInfo)base.Parent; }
		}

		protected FWSHeader Header
		{
			get { return Parent.Parent; }
		}

		public FWSPurposeCodeList PurposeCodeList
		{
			get { return Factory.GetCachedValue<FWSPurposeCodeList>(); }
		}

		public FWSCertificationCodeList CertificationCodeList
		{
			get { return Factory.GetCachedValue<FWSCertificationCodeList>(); }
		}

		public CodeDescriptionPairList FWSCertifyingIndividualList
		{
			get { return PartyTypeList.GetListForFWSCertifyingIndividual(Factory); }
		}
	}
}
