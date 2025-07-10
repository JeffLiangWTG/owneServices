using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	#region Interfaces

	public interface IOrgHeaderForMatching : IMatchingOrganisation
	{
		OrganisationTypes OrganisationTypes { get; }
		BusinessObjectFactory Factory { get; }

		ZBool HasAddress(IMatchingAddress adr);
		IMatchingAddress MainAddress { get; }

		OrgPatternMatchCollection SimilarOrgMatches { get; }
		OrgPatternMatchCollection PatternMatchesForThisOrg { get; }

		new List<IOrgCusCodeForMatching> CustomsCodes { get; }

		ZPropertyInfo OH_RL_NKClosestPortInfo { get; }

		ISimilarOrganisationsFinder SimilarOrgFinder { get; }
		[SuppressWeaklyTypedCollectionMessage]
		IList OH_State_List { get; }
		void ResetHasChanges();
		IMatchingAddress AddNewAddress();
		void OnAfterImport();
		void Delete();

		ZBool OH_IsSalesLead { get; } // Used in Enterprise.DataTransfer
		ZBool OH_IsTempAccount { get; } // Used in Enterprise.DataTransfer
	}

	public interface IOrgCusCodeForMatching : IMatchingCusCode
	{
		new ZString OK_CustomsRegNo { get; set; }
		new ZString OK_CodeType { get; set; }
		new ZString OK_RN_NKCodeCountry { get; set; }
		new ZGuid OK_OA_PremisesAddress { get; set; }
	}

	public interface IOrgHeaderWithAddressesNoAutoCreate
	{
		List<IMatchingAddress> AddressesNoAutoCreate { get; }
	}

	#endregion

	#region Classes

	#region OrgHeaderForMatching

	public class OrgHeaderForMatching : IOrgHeaderForMatching
	{
		public OrgHeaderForMatching(BusinessObjectFactory factory)
		{
			Factory = factory;
			PK = ZGuid.NewZGuid();
		}

		#region IOrgHeaderForMatching Members

		public ZGuid PK { get; set; }

		public ZString OH_Code { get; set; }

		public ZString OH_FullName { get; set; }

		public ZString OH_Language { get; set; }

		public ZString OH_RL_NKClosestPort { get; set; }

		ZBool fOH_IsActive = ZBool.True;
		public ZBool OH_IsActive
		{
			get
			{
				return fOH_IsActive;
			}
			set
			{
				fOH_IsActive = value;
			}
		}

		public ZPropertyInfo OH_RL_NKClosestPortInfo
		{
			get
			{
				return null;
			}
		}

		public ZBool OH_RL_NKClosestPortInfoHasChanges
		{
			get
			{
				return ZBool.True;
			}
		}

		public IMatchingAddress MainAddress
		{
			get { return mainAddressCache != null ? mainAddressCache.Value : GetMainAddressCore(); }
		}

		IMatchingAddress GetMainAddressCore()
		{
			foreach (IMatchingAddress adr in Addresses)
			{
				if (adr.IsMainAddress)
				{
					return adr;
				}
			}

			IMatchingAddress main = AddNewAddress();
			main.SetMainAddress();
			return main;
		}

		IDisposable IMatchingOrganisation.CacheMainAddress()
		{
			mainAddressCache = new CachedValue<IMatchingAddress>(GetMainAddressCore);
			return new DisposableAction(() => mainAddressCache = null);
		}

		CachedValue<IMatchingAddress> mainAddressCache;

		IEnumerable<IMatchingAddress> IMatchingOrganisationCollections.Addresses
		{
			get { return Addresses; }
		}

		List<IMatchingAddress> addresses;
		public List<IMatchingAddress> Addresses
		{
			get
			{
				if (addresses == null)
				{
					addresses = new List<IMatchingAddress>();
				}
				return addresses;
			}
		}

		List<IOrgCusCodeForMatching> customsCodes;
		public List<IOrgCusCodeForMatching> CustomsCodes
		{
			get
			{
				if (customsCodes == null)
				{
					customsCodes = new List<IOrgCusCodeForMatching>();
				}
				return customsCodes;
			}
		}

		IEnumerable<IMatchingCusCode> IMatchingOrganisationCollections.CustomsCodes
		{
			get { return CustomsCodes.Cast<IMatchingCusCode>().ToList(); }
		}

		List<IMatchingBrandOrRelatedName> brands;
		public List<IMatchingBrandOrRelatedName> Brands
		{
			get
			{
				if (brands == null)
				{
					brands = new List<IMatchingBrandOrRelatedName>();
				}
				return brands;
			}
		}

		public IEnumerable<OrganisationName> OrganisationNamesExceptBrands
		{
			get
			{
				var result = new List<OrganisationName>();
				result.Add(new OrganisationName(OH_FullName, OH_Language));

				foreach (var address in Addresses)
				{
					if (!address.OA_CompanyNameOverride.IsEmpty && address.OA_IsActive)
					{
						result.Add(new OrganisationName(address.OA_CompanyNameOverride, address.OA_Language) { OrgAddressPK = address.PK });
					}
				}

				if (OriginalCollections.OrganisationNamesExceptBrands == null)
				{
					OriginalCollections.OrganisationNamesExceptBrands = result;
				}

				return result;
			}
		}

		public IEnumerable<OrganisationName> OrganisationNamesFromBrands
		{
			get
			{
				var result = new List<OrganisationName>();

				foreach (var brandName in Brands)
				{
					result.Add(new OrganisationName(brandName.P1_RelatedName, OH_Language));
				}

				if (OriginalCollections.OrganisationNamesFromBrands == null)
				{
					OriginalCollections.OrganisationNamesFromBrands = result;
				}

				return result;
			}
		}

		BusinessObjectFactory factory;
		public BusinessObjectFactory Factory
		{
			get
			{
				return factory;
			}
			private set
			{
				factory = value;
			}
		}

		public ZBool PatternMatchRequiresFullRegen
		{
			get
			{
				return true;
			}
			set { }
		}

		public ZBool PatternMatchRequiresRegen
		{
			get
			{
				return true;
			}
			set { }
		}

		public ZBool HasAddress(IMatchingAddress adr)
		{
			if (adr != null)
			{
				foreach (var address in Addresses)
				{
					IMatchingAddress match = address;
					if (match != null && match == adr)
					{
						return ZBool.True;
					}
				}
			}
			return ZBool.False;
		}

		PatternMatchingOriginalCollections IMatchingOrganisation.OriginalCollections
		{
			get { return OriginalCollections; }
		}

		PatternMatchingOriginalCollections originalCollections;
		internal PatternMatchingOriginalCollections OriginalCollections
		{
			get
			{
				if (originalCollections == null)
				{
					originalCollections = new PatternMatchingOriginalCollections();
					originalCollections.SetDefaultValues();
				}

				return originalCollections;
			}
		}

		public OrganisationTypes OrganisationTypes { get; set; }

		public ZBool OH_IsCreditor { get; set; }
		public ZBool OH_IsDebtor { get; set; }
		public ZBool OH_IsBroker { get; set; }
		public ZBool OH_IsShippingProvider { get; set; }
		public ZBool OH_IsConsignee { get; set; }
		public ZBool OH_IsConsignor { get; set; }
		public ZBool OH_IsCompetitor { get; set; }
		public ZBool OH_IsForwarder { get; set; }
		public ZBool OH_IsGlobalAccount { get; set; }
		public ZBool OH_IsNationalAccount { get; set; }
		public ZBool OH_IsSalesLead { get; set; }
		public ZBool OH_IsMiscFreightServices { get; set; }
		public ZBool OH_IsTempAccount { get; set; }
		public ZBool OH_IsTransportClient { get; set; }
		public ZBool OH_IsWarehouseClient { get; set; }

		OrgPatternMatchCollection similarOrgMatches;
		public OrgPatternMatchCollection SimilarOrgMatches
		{
			get
			{
				if (similarOrgMatches == null)
				{
					similarOrgMatches = new OrgPatternMatchCollection(Factory);
					similarOrgMatches.SetReadOnlyIncludingChildren(true);
				}

				return similarOrgMatches;
			}
		}

		OrgPatternMatchCollection patternMatchesForThisOrg;
		public OrgPatternMatchCollection PatternMatchesForThisOrg
		{
			get
			{
				if (patternMatchesForThisOrg == null)
				{
					ZQuery query = new ZQuery(OrgPatternMatchSchema.OS_OH, PK);
					patternMatchesForThisOrg = new OrgPatternMatchCollection(Factory, query);
					patternMatchesForThisOrg.GeneratePatternMatchesFromOrg(this);
				}

				return patternMatchesForThisOrg;
			}
		}

		ISimilarOrganisationsFinder similarOrgFinder;
		public ISimilarOrganisationsFinder SimilarOrgFinder
		{
			get
			{
				if (similarOrgFinder == null)
				{
					similarOrgFinder = new SimilarOrganisationsFinder(this);
				}
				return similarOrgFinder;
			}
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList OH_State_List
		{
			get
			{
				return null;
			}
		}

		public void ResetHasChanges()
		{ }

		public IMatchingAddress AddNewAddress()
		{
			OrgAddressForMatching adr = new OrgAddressForMatching();
			Addresses.Add(adr);
			adr.HeaderForMatching = this;
			return adr;
		}

		public bool IsInDatabase { get { return false; } }

		public void OnAfterImport()
		{
			OriginalCollections.Addresses = Addresses;
			OriginalCollections.OrganisationNamesExceptBrands = OrganisationNamesExceptBrands;
			OriginalCollections.OrganisationNamesFromBrands = OrganisationNamesFromBrands;
			OriginalCollections.CustomsCodes = ((IMatchingOrganisation)this).CustomsCodes;
		}

		public void Delete()
		{
			// realistically this code should do nothing - the interface object should just fall away, talking all of its evil cronies with it
			if (patternMatchesForThisOrg != null)
			{
				PatternMatchesForThisOrg.RemoveAndDeleteAll();
			}
		}

		#endregion

		public ZString PortName
		{
			get { return MainAddress != null ? MainAddress.PortName : ZString.Empty; }
		}

		public MultilingualString CountryName
		{
			get { return MainAddress != null ? MainAddress.CountryName : (NoResString)string.Empty; }
		}
	}

	#endregion

	#region OrgAddressForMatching

	public class OrgAddressForMatching : IMatchingAddress
	{
		public OrgAddressForMatching()
		{
			PK = ZGuid.NewZGuid();
		}

		#region IOrgAddressForMatching Members

		public ZGuid PK { get; set; }

		public ZString OA_Code { get; set; }

		public ZString OA_Address1 { get; set; }

		public ZString OA_Address2 { get; set; }

		public ZString OA_City { get; set; }

		public ZString OA_RL_NKRelatedPortCode { get; set; }

		public bool HasChanges
		{
			get
			{
				return true;
			}
		}

		public ZBool OA_RL_NKRelatedPortCodeInfoHasChanges
		{
			get
			{
				return ZBool.True;
			}
		}

		ZBool fOA_IsActive = ZBool.True;
		public ZBool OA_IsActive
		{
			get
			{
				return fOA_IsActive;
			}
			set
			{
				fOA_IsActive = value;
			}
		}

		public bool IsDeleted
		{
			get
			{
				return false;
			}
		}

		public ZString OA_Language { get; set; }

		public ZString OA_PostCode { get; set; }

		public ZString OA_State { get; set; }

		public ZString OA_Fax { get; set; }

		public ZString OA_Phone { get; set; }

		public ZString OA_Email { get; set; }

		public ZString OA_CompanyNameOverride { get; set; }

		public ZString OA_Mobile { get; set; }

		bool isMainAddress;
		public bool IsMainAddress
		{
			get
			{
				return isMainAddress;
			}
			private set
			{
				isMainAddress = value;
			}
		}

		public IMatchingOrganisation HeaderForMatching { get; set; }

		public void SetMainAddress()
		{
			IsMainAddress = true;
		}

		ZString portName;
		public ZString PortName
		{
			get
			{
				ZString result = portName;
				if (result.IsEmpty && !IsMainAddress)
				{
					if (HeaderForMatching != null)
					{
						result = HeaderForMatching.PortName;
					}
				}
				return result;
			}
			set { portName = value; }
		}

		MultilingualString countryName = (NoResString)"";
		public MultilingualString CountryName
		{
			get
			{
				var result = countryName;
				if (result.IsEmpty && !IsMainAddress)
				{
					if (HeaderForMatching != null)
					{
						result = HeaderForMatching.CountryName;
					}
				}
				return result;
			}
			set { countryName = value; }
		}

		public ZString Contact { get; set; }

		public ZString CountryCode { get; set; }

		public ZString OA_AdditionalAddressInformation { get; set; }

		IDisposable IMatchingAddress.CachePortAndCountryNames()
		{
			return null;
		}

		#endregion
	}

	#endregion

	#region OrgCusCodeForMatching

	public class OrgCusCodeForMatching : IOrgCusCodeForMatching
	{
		#region ICusCodeForMatching Members

		public ZGuid PK { get; set; }

		public bool HasChanges { get; set; }

		public bool IsInDatabase { get { return false; } }

		public ZString OK_CustomsRegNo { get; set; }

		public ZString OK_CodeType { get; set; }

		public ZString OK_CodeTypeOriginalValue
		{
			get
			{
				return OK_CodeType;
			}
		}

		public ZString OK_CustomsRegNoOriginalValue
		{
			get
			{
				return OK_CustomsRegNo;
			}
		}

		public bool OK_CustomsRegNoHasChanges
		{
			get
			{
				return true;
			}
		}

		public bool OK_CodeTypeHasChanges
		{
			get
			{
				return true;
			}
		}

		public ZString OK_RN_NKCodeCountry { get; set; }

		public ZBool PremisesAddressIsAllowed
		{
			get
			{
				return OrgCusCode.GetPremisesAddressIsAllowed(OK_CodeType, OK_RN_NKCodeCountry);
			}
		}

		public ZGuid OK_OA_PremisesAddress { get; set; }

		#endregion
	}

	#endregion

	#region OrgBrandOrRelatedNameForMatching

	public class OrgBrandOrRelatedNameForMatching : IMatchingBrandOrRelatedName
	{
		#region IOrgBrandOrRelatedNameForMatching Members

		public ZGuid PK { get; set; }

		public ZString P1_RelatedName { get; set; }

		public ZString P1_RelatedNameOriginal
		{
			get
			{
				return P1_RelatedName;
			}
		}

		#endregion
	}

	#endregion

	#endregion
}
