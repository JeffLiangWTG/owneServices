//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSAAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSNHTSAAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USNHTSAAddInfoLookups : AutoUSNHTSAAddInfoLookups
	{
		public USNHTSAAddInfoLookups(AutoUSNHTSAAddInfo parent) : base(parent)
		{
		}

		protected new USNHTSAAddInfo Parent
		{
			get { return (USNHTSAAddInfo)base.Parent; }
		}

		public NHTSAProgramCodeList AgencyProgramCodes
		{
			get { return Factory.GetCachedValue<NHTSAProgramCodeList>(); }
		}

		public IntendedUseCodesList IntendedUseCodes
		{
			get
			{
				var result = Factory.GetCachedValue<IntendedUseCodesList>();
				result.Sort();
				return result;
			}
		}

		public USCCountryCollection USCountries
		{
			get { return new USCCountryCollection(Factory); }
		}

		public DepartmentOfTransportBoxNumberList BoxNumbers
		{
			get
			{
				var programCode = Parent.US_NHTProgramCode;
				return Factory.GetCachedValue("Enterprise.Customs.US.Business.USNHTSAAddInfoLookups | BoxNumbers" + programCode, delegate
				{
					return GetProgramCodeRelatedBoxNumberList(programCode);
				});
			}
		}

		public static DepartmentOfTransportBoxNumberList GetProgramCodeRelatedBoxNumberList(string agenceProgramCode)
		{
			var result = new DepartmentOfTransportBoxNumberList();

			if (agenceProgramCode != NHTSAProgramCodeList.Codes.MVS && agenceProgramCode != NHTSAProgramCodeList.Codes.REI)
			{
				result.RemoveCode(DepartmentOfTransportBoxNumberList.Codes._2A);
			}

			return result;
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public ConsignorCollection Consignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		public TravelDocumentTypeCodeList TravelDocumentTypes
		{
			get { return Factory.GetCachedValue<TravelDocumentTypeCodeList>(); }
		}

		public DOTBondQualifierList BondTypes
		{
			get { return Factory.GetCachedValue<DOTBondQualifierList>(); }
		}

		public NHTSADocumentTypeList DocumentTypes
		{
			get { return Factory.GetCachedValue<NHTSADocumentTypeList>(); }
		}

		public NHTSAOrganizationTypeList OrganizationTypes
		{
			get { return Factory.GetCachedValue<NHTSAOrganizationTypeList>(); }
		}

		public CodeDescriptionPairList NHTSACertifyingIndividualList
		{
			get { return PartyTypeList.GetListForNHTSACertifyingIndividual(Factory); }
		}
	}
}
