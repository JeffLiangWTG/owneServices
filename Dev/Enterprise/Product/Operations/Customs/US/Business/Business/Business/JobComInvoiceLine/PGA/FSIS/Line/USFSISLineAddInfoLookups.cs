//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFSISLineAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSFSISLineAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USFSISLineAddInfoLookups : AutoUSFSISLineAddInfoLookups
	{
		public USFSISLineAddInfoLookups(AutoUSFSISLineAddInfo parent)
			: base(parent)
		{
		}

		public USCCountryCollection USCountries
		{
			get { return new USCCountryCollection(Factory); }
		}

		public GlobalUniqueProductCodeQualifierList ProductIDQualifiers
		{
			get { return Factory.GetCachedValue<GlobalUniqueProductCodeQualifierList>(); }
		}

		public ACEIntendedUseBaseCodeList ACEIntendedUseBaseCodes
		{
			get { return Factory.GetCachedValue<ACEIntendedUseBaseCodeList>(); }
		}

		public ZZRefCusCodeListCombinedCollection ImportEstablishments
		{
			get { return new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers, ZDate.Today); }
		}

		public CodeDescriptionPairList FSISCertifyingIndividualList
		{
			get { return PartyTypeList.GetListForFSISCertifyingIndividual(Factory); }
		}
	}
}
