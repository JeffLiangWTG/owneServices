using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SalesEnquiryLookups : OrgColdCallRegisterLookups
	{
		public SalesEnquiryLookups(SalesEnquiry parent)
			: base(parent)
		{
		}

		protected new SalesEnquiry Parent
		{
			get { return (SalesEnquiry)base.Parent; }
		}

		#region Factory

		protected override BusinessObjectFactory Factory
		{
			get
			{
				if (Parent != null)
				{
					return Parent.Factory;
				}
				else
				{
					return factory ?? (factory = new BusinessObjectFactory());
				}
			}
		}

		BusinessObjectFactory factory;

		#endregion

		#region Organisations

		public OrganisationsFindBoxCollection OrganisationsList
		{
			get
			{
				return Factory.GetCachedValue("SalesEnquiryLookups.OrganisationsList", () =>
				{
					return new OrganisationsFindBoxCollection(Factory);
				});
			}
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region AllLeadInterests

		public static CodeDescriptionPairList GetAllLeadInterests()
		{
			return OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.Value.GetCodeDescriptionPairList();
		}

		#endregion

		#region Lead Source

		public ReadOnlyCodeDescriptionPairList Source_ActiveList
		{
			get
			{
				var activeCodeDescriptionPairList = OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetActiveCodeDescriptionPairList();
				activeCodeDescriptionPairList.SortByDescription();
				return activeCodeDescriptionPairList;
			}
		}

		public ReadOnlyCodeDescriptionPairList Source_List
		{
			get { return GetSourceList(); }
		}

		public static CodeDescriptionPairList GetSourceList()
		{
			return OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetCodeDescriptionPairList();
		}

		#endregion

		#region Enquiry Types

		public static CodeDescriptionPairList GetAllEnquiryTypes()
		{
			return OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.Value.GetCodeDescriptionPairList();
		}

		public static CodeDescriptionPairList GetActiveEnquiryTypes()
		{
			return OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.Value.GetActiveCodeDescriptionPairList();
		}

		public CodeDescriptionPairList AllEnquiryTypes
		{
			get { return GetAllEnquiryTypes(); }
		}

		public CodeDescriptionPairList ActiveEnquiryTypes
		{
			get
			{
				return Factory.GetCachedValue("SalesEnquiryLookups.ActiveEnquiryTypes", () =>
				{
					var activeCodeDescriptionPairList = GetActiveEnquiryTypes();
					activeCodeDescriptionPairList.SortByDescription();
					return activeCodeDescriptionPairList;
				});
			}
		}

		#endregion

		#region Lead Status

		public CodeDescriptionPairList StatusList
		{
			get { return new SalesEnquiryStatusCodeList(); }
		}

		#endregion

		#region Lead Interest

		public CodeDescriptionPairList LeadInterest_List
		{
			get { return SalesEnquiryLookups.GetAllLeadInterests(); }
		}

		public CodeDescriptionPairList LeadInterest_ActiveList
		{
			get
			{
				return Factory.GetCachedValue("SalesEnquiryLookups.LeadInterest_ActiveList", () =>
				{
					var activeCodeDescriptionPairList = OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.Value.GetActiveCodeDescriptionPairList();
					activeCodeDescriptionPairList.SortByDescription();
					return activeCodeDescriptionPairList;
				});
			}
		}

		#endregion

		#region Addresses

		public ZAddressList AddressList
		{
			get
			{
				var addresses = new ZAddressList();
				if (Parent != null)
				{
					var org = Parent.Header;
					if (org != null)
					{
						foreach (OrgAddress address in org.Addresses)
						{
							addresses.AddAddress(address.PK, address.OA_Code, address.AddressAsASingleLineWithoutCompanyName);
						}
					}
				}

				return addresses;
			}
		}

		#endregion

		#region Contacts

		public OrgContactCollection Contacts
		{
			get
			{
				var orgPK = Parent?.Header?.PK;
				if (innerContacts == null || lastOrgHeaderPK != orgPK)
				{
					lastOrgHeaderPK = orgPK ?? ZGuid.Empty;
					var localInnerContacts = new OrgContactCollection(Factory, GetContactsFilter(lastOrgHeaderPK));
					localInnerContacts.Load();
					innerContacts = localInnerContacts;
				}
				return innerContacts;
			}
		}

		ZQuery GetContactsFilter(ZGuid orgPK)
		{
			var filter = new ZQuery(OrgContactSchema.OC_OH, orgPK);
			filter.AddToFilter(OrgContactSchema.OC_IsActive, true);
			return filter;
		}

		ZGuid lastOrgHeaderPK = ZGuid.Empty;
		OrgContactCollection innerContacts;

		#endregion

		#region Contacts of Referring Organisation

		public OrgContactCollection ContactsOfReferringOrg
		{
			get
			{
				var orgPK = Parent?.ReferringOrg?.PK;
				if (innerReferringContacts == null || lastReferringOrgHeaderPK != orgPK)
				{
					lastReferringOrgHeaderPK = orgPK ?? ZGuid.Empty;
					var localInnerReferringContacts = new OrgContactCollection(Factory, GetContactsFilter(lastReferringOrgHeaderPK));
					localInnerReferringContacts.Load();
					innerReferringContacts = localInnerReferringContacts;
				}
				return innerReferringContacts;
			}
		}

		public OrgContactCollection ContactsOfReferToOrg
		{
			get
			{
				var orgPK = Parent?.ReferTo?.PK;
				if (innerReferToContacts == null || lastReferToOrgHeaderPK != orgPK)
				{
					lastReferToOrgHeaderPK = orgPK ?? ZGuid.Empty;
					var localInnerReferToContacts = new OrgContactCollection(Factory, GetContactsFilter(lastReferToOrgHeaderPK));
					localInnerReferToContacts.Load();
					innerReferToContacts = localInnerReferToContacts;
				}
				return innerReferToContacts;
			}
		}

		ZGuid lastReferringOrgHeaderPK = ZGuid.Empty;
		ZGuid lastReferToOrgHeaderPK = ZGuid.Empty;
		OrgContactCollection innerReferringContacts;
		OrgContactCollection innerReferToContacts;

		#endregion

		#region Contact Job Category List

		public ReadOnlyCodeDescriptionPairList JobCategory_List
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("SalesEnquiryLookups.JobCategory_List", () =>
				{
					UntranslatableCodeDescriptionPairList list = new UntranslatableCodeDescriptionPairList((NoResString)"Description values are stored directly in the database");
					list.AddRange(OrganisationsDataRegistry.Instance.ContactJobCategories.Value);
					return list;
				});
			}
		}

		#endregion

		#region Org Country List

		public RefCountryCollection OrgCountryList
		{
			get { return new RefCountryCollection(Parent.Factory); }
		}

		#endregion

		#region Close Reason

		public CodeDescriptionPairList EnquiryCloseReasonList
		{
			get
			{
				return CreateEnquiryCloseReasonList();
			}
		}

		public static CodeDescriptionPairList CreateEnquiryCloseReasonList()
		{
			var result = new CodeDescriptionPairList();
			foreach (ICodeDescription item in OrganisationsDataRegistry.Instance.SalesEnquiryCloseReasonList.Value)
			{
				result.AddPairIfNotExist(item.Code, item.Description);
			}

			return result;
		}

		public CodeDescriptionPairList CloseReason_ActiveList
		{
			get
			{
				return Factory.GetCachedValue("SalesEnquiryLookups.CloseReasonActiveList", () =>
				{
					return new CodeDescriptionPairList(OrganisationsDataRegistry.Instance.SalesEnquiryCloseReasonList.Value.GetActiveCodeDescriptionPairList());
				});
			}
		}

		#endregion

		#region StateList

		public CodeDescriptionPairList StateList
		{
			get
			{
				var result = new UntranslatableCodeDescriptionPairList((NoResString)"States from RefCountryState table");
				var stateList = (Parent.Country != null) ? new OrgCodeLists().State_List(Parent.Country) : new CodeDescriptionPairList();
				result.AddRange(stateList);

				return result;
			}
		}

		#endregion

	}
}
