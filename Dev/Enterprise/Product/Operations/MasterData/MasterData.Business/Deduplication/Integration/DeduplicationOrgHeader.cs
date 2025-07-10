using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector.Common;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationOrgHeader : IOrgHeader, IDeduplicationMaster, IDeduplicationGlowObject, IRawNameProvider
	{
		[Newtonsoft.Json.JsonConstructor]
		DeduplicationOrgHeader() { }

		public DeduplicationOrgHeader(string fullName)
		{
			OH_FullName = fullName;
			OrgAddresses = Array.Empty<DeduplicationOrgAddress>();
			OrgBrandOrRelatedNames = Array.Empty<DeduplicationOrgBrandOrRelatedName>();
			OrgContacts = Array.Empty<DeduplicationOrgContact>();
			CusCodes = Array.Empty<DeduplicationOrgCusCode>();
			OrgWebURLs = Array.Empty<DeduplicationOrgWebURL>();
			RawName = fullName;
		}

		internal static DeduplicationOrgHeader CreateBridgeDedupOrgHeader(OrgHeader orgHeader)
		{
			return new DeduplicationOrgHeader()
			{
				OH_PK = orgHeader.PK.ToGuid(),
				OH_FullName = orgHeader.OH_FullName,
				OH_Code = orgHeader.OH_Code,
				RawName = orgHeader.OH_FullName
			};
		}

		public DeduplicationOrgHeader(OrgHeader header, bool isDummy = false)
		{
			OrganisationTypesAsString = header.OrganisationTypesAsString;
			CreditorCompany = header.CreditorCompany;
			DebtorCompany = header.DebtorCompany;

			var masterParent = GetParent(header);
			GetChildren(masterParent);

			IsDummy = isDummy;
			OH_PK = header.PK.ToGuid();
			OH_Category = header.OH_Category;
			OH_Code = header.OH_Code;
			OH_FullName = header.OH_FullName;
			OH_IsActive = header.OH_IsActive;
			OH_Language = header.OH_Language;
			OH_RL_NKClosestPort = header.OH_RL_NKClosestPort;
			OH_ScreeningStatus = header.OH_ScreeningStatus;
			OH_IsShippingLine = header.OH_IsShippingLine;
			IsInDatabase = header.IsInDatabase;
			RawName = header.OH_FullName;
			Master = header;
			ReadOnly = header.ReadOnly;
			UNLOCO = header.UNLOCO;
			CountryCode = OrgHeaderDuplicationFinder.GetOrgCountryCodeWithFallbackLogic(header);
			ShouldRunDeduplication = ((IDeduplicatable)header).ShouldRunDeduplication;

			if (ShouldRunDeduplication)
			{
				OrgAddresses = header.Addresses
				.Cast<OrgAddress>()
				.Where(address => address != null)
				.ToArray() // Prevent address collection changed when access the main address property
				.Select(addr => new DeduplicationOrgAddress(addr, this))
				.ToArray();
			}
			else
			{
				OrgAddresses = GetAddress(new Guid(header.PK.ToString())).ToArray();
			}

			OrgBrandOrRelatedNames = header.BrandsOrRelatedNames
				.Cast<OrgBrandOrRelatedName>()
				.Where(brand => brand != null)
				.Select(brand => new DeduplicationOrgBrandOrRelatedName(brand, this))
				.ToArray();

			if (ShouldRunDeduplication)
			{
				OrgContacts = header.Contacts
					.Cast<OrgContact>()
					.Where(contact => contact != null)
					.Select(contact => new DeduplicationOrgContact(contact, this, false))
					.ToArray();
			}
			else
			{
				OrgContacts = GetContacts(new Guid(header.PK.ToString())).ToArray();
			}

			CusCodes = header.CustomsCodes
				.Cast<OrgCusCode>()
				.Where(cuscode => cuscode != null)
				.Select(cuscode => new DeduplicationOrgCusCode(cuscode, this))
				.ToArray();

			OrgWebURLs = header.OrgWebURLs
				.Cast<OrgWebURL>()
				.Where(weburl => weburl != null)
				.Select(weburl => new DeduplicationOrgWebURL(weburl, this))
				.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public IEnumerable<DeduplicationOrgContact> GetContacts(Guid headerPk)
		{
			var deduplicationOrgContactList = new List<DeduplicationOrgContact>();
			var orgHeaderPk = headerPk;
			var sqlText = @"select OC_PK, OC_ContactName, OC_Birthday, OC_Email, OC_Fax, OC_HomePhone, OC_IsActive, OC_Language, OC_Mobile, OC_OH, OC_OtherPhone, OC_Phone, OC_PER, OC_ContactName
		from dbo.OrgContact where OC_OH = @OC_OH;";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@OC_OH", SqlDbType.UniqueIdentifier, orgHeaderPk);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var deduplicationOrgContact = new DeduplicationOrgContact();
						deduplicationOrgContact.OC_PK = Guid.Parse(Convert.ToString(reader[0]));
						deduplicationOrgContact.OC_ContactName = Convert.ToString(reader[1]);
						if (reader[2] != DBNull.Value)
						{
							deduplicationOrgContact.OC_Birthday = Convert.ToDateTime(reader[2]);
						}
						deduplicationOrgContact.OC_Email = Convert.ToString(reader[3]);
						deduplicationOrgContact.OC_Fax = Convert.ToString(reader[4]);
						deduplicationOrgContact.OC_HomePhone = Convert.ToString(reader[5]);
						deduplicationOrgContact.OC_IsActive = Convert.ToBoolean(reader[6]);
						deduplicationOrgContact.OC_Language = Convert.ToString(reader[7]);
						deduplicationOrgContact.OC_Mobile = Convert.ToString(reader[8]);
						deduplicationOrgContact.OC_OH = Guid.Parse(Convert.ToString(reader[9]));
						deduplicationOrgContact.OC_OtherPhone = Convert.ToString(reader[10]);
						deduplicationOrgContact.OC_Phone = Convert.ToString(reader[11]);
						deduplicationOrgContact.OC_PER = Guid.Parse(Convert.ToString(reader[12]));
						deduplicationOrgContact.OC_ContactName = Convert.ToString(reader[13]);
						deduplicationOrgContactList.Add(deduplicationOrgContact);
					}
				}
			}

			return deduplicationOrgContactList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public IEnumerable<DeduplicationOrgAddress> GetAddress(Guid headerPk)
		{
			var deduplicationOrgAddressDict = new Dictionary<Guid, DeduplicationOrgAddress>();
			var deduplicationOrgAddressCapabilityDict = new Dictionary<Guid, List<DeduplicationOrgAddressCapability>>();
			var countryPKDict = new Dictionary<string, Guid>();
			var deduplicationOrgAddressList = new List<DeduplicationOrgAddress>();
			var orgHeaderPk = headerPk;
			var sqlText = @"select OA_PK, OA_Address1, OA_Address2, OA_City, OA_Code, OA_Email, OA_Fax, OA_IsActive, OA_Mobile, OA_OH, OA_Phone, OA_PostCode, OA_RL_NKRelatedPortCode, OA_RN_NKCountryCode, OA_State, OA_ValidationStatus, OA_VerifiesContainerGrossWeight, OA_Language, OA_GeoLocation,
PZ_PK, PZ_AddressType, PZ_IsMainAddress, PZ_OA, PZ_IsValid, RN_PK
from dbo.OrgAddress a left join dbo.OrgAddressCapability ac 
on a.OA_PK = ac.PZ_OA left join dbo.RefCountry c on a.OA_RN_NKCountryCode = c.RN_Code where OA_OH = @OA_OH;";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@OA_OH", SqlDbType.UniqueIdentifier, orgHeaderPk);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var deduplicationOrgAddress = new DeduplicationOrgAddress();
						var orgAddresspk = Guid.Parse(Convert.ToString(reader[0]));

						if (deduplicationOrgAddressDict.ContainsKey(orgAddresspk))
						{
							deduplicationOrgAddress = deduplicationOrgAddressDict[orgAddresspk];
						}
						else
						{
							deduplicationOrgAddressDict.Add(orgAddresspk, deduplicationOrgAddress);
							deduplicationOrgAddressList.Add(deduplicationOrgAddress);
							deduplicationOrgAddress.OA_PK = orgAddresspk;
							deduplicationOrgAddress.OA_Address1 = Convert.ToString(reader[1]);
							deduplicationOrgAddress.OA_Address2 = Convert.ToString(reader[2]);
							deduplicationOrgAddress.OA_City = Convert.ToString(reader[3]);
							deduplicationOrgAddress.OA_Code = Convert.ToString(reader[4]);
							deduplicationOrgAddress.OA_Email = Convert.ToString(reader[5]);
							deduplicationOrgAddress.OA_Fax = Convert.ToString(reader[6]);
							deduplicationOrgAddress.OA_IsActive = Convert.ToBoolean(reader[7]);
							deduplicationOrgAddress.OA_Mobile = Convert.ToString(reader[8]);
							deduplicationOrgAddress.OA_OH = Guid.Parse(Convert.ToString(reader[9]));
							deduplicationOrgAddress.OA_Phone = Convert.ToString(reader[10]);
							deduplicationOrgAddress.OA_PostCode = Convert.ToString(reader[11]);
							deduplicationOrgAddress.OA_RL_NKRelatedPortCode = Convert.ToString(reader[12]);
							deduplicationOrgAddress.OA_RN_NKCountryCode = Convert.ToString(reader[13]);
							deduplicationOrgAddress.OA_State = Convert.ToString(reader[14]);
							deduplicationOrgAddress.OA_ValidationStatus = Convert.ToString(reader[15]);
							deduplicationOrgAddress.OA_VerifiesContainerGrossWeight = Convert.ToBoolean(reader[16]);
							deduplicationOrgAddress.OA_Language = Convert.ToString(reader[17]);
							var countryPK = reader[24] == DBNull.Value ? Guid.Empty : Guid.Parse(Convert.ToString(reader[24]));
							countryPKDict[deduplicationOrgAddress.OA_RN_NKCountryCode] = countryPK;
							dynamic geoLocation = reader[18];

							if (!geoLocation.Lat.IsNull && !geoLocation.Long.IsNull)
							{
								deduplicationOrgAddress.OA_Latitude = Convert.ToDecimal(geoLocation.Lat.Value);
								deduplicationOrgAddress.OA_Longitude = Convert.ToDecimal(geoLocation.Long.Value);
							}
						}

						if (reader[20] != DBNull.Value)
						{
							var deduplicationOrgAddressCapability = new DeduplicationOrgAddressCapability();

							if (deduplicationOrgAddressCapabilityDict.ContainsKey(orgAddresspk))
							{
								deduplicationOrgAddressCapabilityDict[orgAddresspk].Add(deduplicationOrgAddressCapability);
							}
							else
							{
								deduplicationOrgAddressCapabilityDict.Add(orgAddresspk, new List<DeduplicationOrgAddressCapability>() { deduplicationOrgAddressCapability });
							}

							deduplicationOrgAddressCapability.PZ_PK = Guid.Parse(Convert.ToString(reader[19]));
							deduplicationOrgAddressCapability.PZ_AddressType = Convert.ToString(reader[20]);
							deduplicationOrgAddressCapability.PZ_IsMainAddress = Convert.ToBoolean(reader[21]);
							deduplicationOrgAddressCapability.PZ_OA = Guid.Parse(Convert.ToString(reader[22]));
							deduplicationOrgAddressCapability.PZ_IsValid = Convert.ToBoolean(reader[23]);
						}
					}
				}
			}

			foreach (var orgAddress in deduplicationOrgAddressList)
			{
				if (deduplicationOrgAddressCapabilityDict.ContainsKey(orgAddress.PK))
				{
					orgAddress.OrgAddressCapabilities = deduplicationOrgAddressCapabilityDict[orgAddress.PK].ToArray();
				}

				var shouldUseValidation = OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(countryPKDict[orgAddress.OA_RN_NKCountryCode], AddressValidationSection.OrganizationAddress);
				orgAddress.OA_ValidationStatus = shouldUseValidation ? orgAddress.OA_ValidationStatus : "NTC";
			}

			return deduplicationOrgAddressList;
		}

		public IDeduplicatable Master { get; }

		void GetChildren(OrgHeader masterParent)
		{
			if (!AllRelatedOrganizationsPK.Contains(masterParent.PK))
			{
				AllRelatedOrganizationsPK.Add(masterParent.PK);

				var children = masterParent.RelatedManagementSubsidiaryRelations?.Organisations?.ToArray();
				if (children != null && children.Length > 0)
				{
					foreach (var child in children)
					{
						GetChildren(child);
					}
				}
			}
		}

		OrgHeader GetParent(OrgHeader orgHeader, List<ZGuid> visitedNodes = null)
		{
			visitedNodes = visitedNodes ?? new List<ZGuid>();

			visitedNodes.Add(orgHeader.PK);
			var parentRelations = orgHeader.RelatedManagementParentRelations?.Organisations?.ToArray();
			if (parentRelations != null && parentRelations.Length == 1)
			{
				var parent = parentRelations[0];
				if (visitedNodes.Contains(parent.PK))
				{
					return orgHeader;
				}

				return GetParent(parent, visitedNodes);
			}

			return orgHeader;
		}

		public bool IsDummy { get; set; }
		public bool ReadOnly { get; }
		public Type BizoType => typeof(OrgHeader);
		public Guid PK => OH_PK;
		public Guid OH_PK { get; set; }
		public string TablePrefix => OrgHeaderSchema.Constants.Prefix;

		public string OH_Category { get; set; }
		public string OH_Code { get; set; }
		public string OH_FullName { get; set; }
		public bool OH_IsActive { get; set; }
		public bool OH_IsAirCTO { get; set; }
		public bool OH_IsAirLine { get; set; }
		public bool OH_IsAirWholesaler { get; set; }
		public bool OH_IsBroker { get; set; }
		public bool OH_IsCompetitor { get; set; }
		public bool OH_IsConsignee { get; set; }
		public bool OH_IsConsignor { get; set; }
		public bool OH_IsContainerLeasingCompany { get; set; }
		public bool OH_IsContainerYard { get; set; }
		public bool OH_IsControllingAgent { get; set; }
		public bool OH_IsControllingCustomer { get; set; }
		public bool OH_IsDistributionCentre { get; set; }
		public bool OH_IsForwarder { get; set; }
		public bool OH_IsFumigationContractor { get; set; }
		public bool OH_IsGlobalAccount { get; set; }
		public bool OH_IsInlandWaterwayProvider { get; set; }
		public bool OH_IsLineHaulProvider { get; set; }
		public bool OH_IsLocalTransport { get; set; }
		public bool OH_IsMiscFreightServices { get; set; }
		public bool OH_IsNationalAccount { get; set; }
		public bool OH_IsPackDepot { get; set; }
		public bool OH_IsPersonalEffectsAccount { get; set; }
		public bool OH_IsRailHead { get; set; }
		public bool OH_IsRailProvider { get; set; }
		public bool OH_IsRoadFreightDepot { get; set; }
		public bool OH_IsSalesLead { get; set; }
		public bool OH_IsSeaCTO { get; set; }
		public bool OH_IsFerryWaterTerminal { get; set; }
		public bool OH_IsSeaWholesaler { get; set; }
		public bool OH_IsShippingConsortium { get; set; }
		public bool OH_IsShippingLine { get; set; }
		public bool OH_IsShippingProvider { get; set; }
		public bool OH_IsTempAccount { get; set; }
		public bool OH_IsTransportClient { get; set; }
		public bool OH_IsUnpackDepot { get; set; }
		public bool OH_IsUserFlag1 { get; set; }
		public bool OH_IsUserFlag10 { get; set; }
		public bool OH_IsUserFlag11 { get; set; }
		public bool OH_IsUserFlag12 { get; set; }
		public bool OH_IsUserFlag13 { get; set; }
		public bool OH_IsUserFlag14 { get; set; }
		public bool OH_IsUserFlag15 { get; set; }
		public bool OH_IsUserFlag16 { get; set; }
		public bool OH_IsUserFlag17 { get; set; }
		public bool OH_IsUserFlag18 { get; set; }
		public bool OH_IsUserFlag19 { get; set; }
		public bool OH_IsUserFlag2 { get; set; }
		public bool OH_IsUserFlag20 { get; set; }
		public bool OH_IsUserFlag21 { get; set; }
		public bool OH_IsUserFlag22 { get; set; }
		public bool OH_IsUserFlag23 { get; set; }
		public bool OH_IsUserFlag24 { get; set; }
		public bool OH_IsUserFlag25 { get; set; }
		public bool OH_IsUserFlag26 { get; set; }
		public bool OH_IsUserFlag27 { get; set; }
		public bool OH_IsUserFlag28 { get; set; }
		public bool OH_IsUserFlag29 { get; set; }
		public bool OH_IsUserFlag3 { get; set; }
		public bool OH_IsUserFlag30 { get; set; }
		public bool OH_IsUserFlag31 { get; set; }
		public bool OH_IsUserFlag32 { get; set; }
		public bool OH_IsUserFlag4 { get; set; }
		public bool OH_IsUserFlag5 { get; set; }
		public bool OH_IsUserFlag6 { get; set; }
		public bool OH_IsUserFlag7 { get; set; }
		public bool OH_IsUserFlag8 { get; set; }
		public bool OH_IsUserFlag9 { get; set; }
		public bool OH_IsValid { get; set; }
		public bool OH_IsVGMContractor { get; set; }
		public bool OH_IsWarehouseClient { get; set; }
		public string OH_Language { get; set; }
		public bool OH_OverrideAdditionalAddressInformation { get; set; }
		public string OH_RL_NKClosestPort { get; set; }
		public Guid? OH_RSL_ShippingLine { get; set; }
		public string OH_ScreeningStatus { get; set; }
		public bool IsInDatabase { get; }
		public string CountryCode { get; set; }
		public ZString OrganisationTypesAsString { get; private set; }
		public ZString CreditorCompany { get; }
		public ZString DebtorCompany { get; }
		public RefUNLOCO UNLOCO { get; set; }
		public bool ShouldRunDeduplication { get; set; }
		public DateTime? OH_SystemCreateTimeUtc { get; set; }
		public string OH_SystemCreateUser { get; set; }
		public string OH_SystemCreateBranch { get; set; }
		public string OH_SystemCreateDepartment { get; set; }
		public DateTime? OH_SystemLastEditTimeUtc { get; set; }
		public string OH_SystemLastEditUser { get; set; }
		public IRefUNLOCOInfo ClosestPort { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }
		public IRefShippingLineInfo ShippingLine { get; set; }

		public ICollection<INettingOrganisation> NettingOrganisations { get; }

		public ICollection<IOrgAddress> OrgAddresses { get; set; }

		public ICollection<IOrgBrandOrRelatedName> OrgBrandOrRelatedNames { get; set; }

		public ICollection<IOrgCompanyData> OrgCompanyData { get; }

		public ICollection<IOrgContact> OrgContacts { get; set; }

		public ICollection<IOrgCountryData> OrgCountryData { get; }

		public ICollection<IOrgCusCode> CusCodes { get; set; }

		public ICollection<IOrgCustomerAddress> OrgCustomerAddresses { get; }

		public ICollection<IOrgMiscServ> OrgMiscServs { get; }

		public ICollection<ICrmOpportunity> CrmOpportunities { get; }

		public ICollection<IOrgProductType> OrgProductTypes { get; }

		public ICollection<IOrgRelatedParty> RelatedParties { get; }

		public ICollection<IOrgRelatedParty> PartiesRelatedToThisOrg { get; }

		public ICollection<IOrgSalesCall> OrgSalesCalls { get; }

		public ICollection<IOrgSecurity> OrgSecurities { get; }

		public ICollection<IOrgStaffAssignment> OrgStaffAssignments { get; }

		public ICollection<IGlbGroupOrgLink> GlbGroupOrgLinks { get; }

		public ICollection<IOrgSupplierBuyerLink> BuyerLinks { get; }

		public ICollection<IOrgSupplierBuyerLink> SupplierLinks { get; }

		public ICollection<IOrgWebURL> OrgWebURLs { get; set; }

		public ICollection<INote<IOrgHeader>> Notes { get; }

		public ICollection<ILog<IOrgHeader>> Logs { get; set; }

		public ICollection<IAcknowledgement<IOrgHeader>> Acknowledgements { get; }

		public ICollection<ZGuid> AllRelatedOrganizationsPK { get; } = new List<ZGuid>();

		public ZString DeduplicationCountryCode => string.Empty;

		public ICollection<IConversationParticipant<IOrgHeader>> ConversationParticipants { get; }

		public ICollection<IOrgCarrierAccount> CarrierAccounts { get; }

		public ICollection<IOrgContainerDetention> CarrierContainerDetentions { get; }

		public ICollection<IOrgContainerDetention> ClientContainerDetentions { get; }

		public ICollection<IWorkflowAuditLog<IOrgHeader>> WorkflowAuditLogs { get; }

		public string RawName { get; private set; }

		public ICollection<IJobRequiredDocument<IOrgHeader>> JobRequiredDocuments => Array.Empty<IJobRequiredDocument<IOrgHeader>>();

		public ICollection<IProcessHeader<IOrgHeader>> ProcessHeaders => Array.Empty<IProcessHeader<IOrgHeader>>();
	}
}
