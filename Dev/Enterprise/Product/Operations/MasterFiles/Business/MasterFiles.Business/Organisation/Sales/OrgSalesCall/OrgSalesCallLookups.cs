//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSalesCallLookups
//
//    This class should be used for overriding collections in AutoOrgSalesCallLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallLookups : AutoOrgSalesCallLookups
	{
		public OrgSalesCallLookups(AutoOrgSalesCall parent) : base(parent)
		{
		}

		new AutoOrgSalesCall Parent
		{
			get { return base.Parent as AutoOrgSalesCall; }
		}

		#region Location List

		LocationListsBuilder LocationLists
		{
			get
			{
				return Factory.GetCachedValue("OrgSalesCallLookups.LocationLists" + ((Parent.Header != null) ? Parent.Header.OH_Code : ZString.Empty), () =>
				{
					return new LocationListsBuilder(Factory, Parent.Header, ZBool.False);
				});
			}
		}

		LocationListsBuilder ActiveLocationLists
		{
			get
			{
				return Factory.GetCachedValue("OrgSalesCallLookups.ActiveLocationLists" + ((Parent.Header != null) ? Parent.Header.OH_Code : ZString.Empty), () =>
				{
					return new LocationListsBuilder(Factory, Parent.Header, ZBool.True);
				});
			}
		}

		public CodeDescriptionPairList LocationList
		{
			get { return LocationLists.CodeDescriptionPairList; }
		}

		public CodeDescriptionPairList ActiveLocationList
		{
			get { return ActiveLocationLists.CodeDescriptionPairList; }
		}

		public Dictionary<ZString, ZGuid> LocationCodePkMap
		{
			get { return LocationLists.LocationCodePkMap; }
		}

		public Dictionary<ZGuid, ZString> LocationPkCodeMap
		{
			get { return LocationLists.LocationPkCodeMap; }
		}

		#endregion

		public ICodeDescriptionPairList OQ_Status_List
		{
			get { return OrganisationsDataRegistry.Instance.CommunicationStatusList.Value; }
		}

		public ICodeDescriptionPairList OQ_Category_List
		{
			get { return OrganisationsDataRegistry.Instance.CategoryList.Value; }
		}

		public ICodeDescriptionPairList OQ_Status_ActiveList
		{
			get { return Factory.GetCachedValue<ICodeDescriptionPairList>("OrgSalesCall.OQ_Status_ActiveList", () => OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.GetActiveCodeDescriptionPairList()); }
		}

		public ICodeDescriptionPairList OQ_Category_ActiveList
		{
			get { return Factory.GetCachedValue<ICodeDescriptionPairList>("OrgSalesCall.OQ_Category_ActiveList", () => OrganisationsDataRegistry.Instance.CategoryList.Value.GetActiveCodeDescriptionPairList()); }
		}

		public ICodeDescriptionPairList OQ_TypeOfCall_List
		{
			get { return OrganisationsDataRegistry.Instance.CommunicationType.Value; }
		}

		public ICodeDescriptionPairList OQ_TypeOfCall_ActiveList
		{
			get { return Factory.GetCachedValue<ICodeDescriptionPairList>("OrgSalesCallLookups.OQ_TypeOfCall_ActiveList", () => OrganisationsDataRegistry.Instance.CommunicationType.Value.GetActiveCodeDescriptionPairList()); }
		}

		public OrgContactDependentCollection OrgContacts
		{
			get
			{
				var org = Parent.Header;
				return org != null ? org.Contacts : null;
			}
		}

		public OrgContactCollection OrgActiveContactsPlusExistingContact
		{
			get
			{
				var org = Parent.Header;
				if (org == null)
				{
					return null;
				}
				else
				{
					var result = new OrgContactCollection(Factory);
					result.AddRange(org.ContactsActive);
					if (Parent.IsInDatabase && !Parent.OQ_OCInfo.HasChanges && Parent.Contact != null)
					{
						result.Add(Parent.Contact);
					}

					result.Sort(OrgContactSchema.Constants.OC_ContactName, ListSortDirection.Ascending);
					return result;
				}
			}
		}

		public ICodeDescriptionPairList OverallDispositionList
		{
			get { return new OrgSalesCallOverallDispositionList(); }
		}

		class LocationListsBuilder
		{
			public LocationListsBuilder(BusinessObjectFactory factory, OrgHeader header, ZBool isActive)
			{
				this.factory = factory;
				this.header = header;
				BuildLocationLists(isActive);
			}

			readonly BusinessObjectFactory factory;
			readonly OrgHeader header;

			public readonly CodeDescriptionPairList CodeDescriptionPairList = new CodeDescriptionPairList();
			public readonly Dictionary<ZString, ZGuid> LocationCodePkMap = new Dictionary<ZString, ZGuid>();
			public readonly Dictionary<ZGuid, ZString> LocationPkCodeMap = new Dictionary<ZGuid, ZString>();

			void BuildLocationLists(ZBool isActiveOnly)
			{
				if (header != null)
				{
					foreach (OrgAddress address in header.Addresses)
					{
						string addressLine = address.AddressAsASingleLineWithoutCompanyName;
						if (LocationCodePkMap.ContainsKey(addressLine))
						{
							addressLine = AddSuffixForDuplicateLocation(LocationCodePkMap, addressLine);
						}
						LocationCodePkMap.Add(addressLine, address.PK);
						LocationPkCodeMap.Add(address.PK, addressLine);
						if ((isActiveOnly && address.OA_IsActive) || !isActiveOnly)
						{
							CodeDescriptionPairList.AddPairIfNotExist(addressLine, Res.GetString("a17f2192-e960-477a-85f0-623b48cf4495", "Client Address ({0})", address.OA_Code));
						}
					}
				}

				var meetingLocationList = SystemDataRegistry.Instance.ResourceTypes.Value.GetActiveCodeDescriptionPairList();
				foreach (CodeDescriptionPair pair in meetingLocationList)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(GlbStaffSchema.GS_IsResource, true);
					query.AddToFilter(GlbStaffSchema.GS_ResourceType, pair.Code);
					var locationResources = factory.Load<GlbStaff>(query);

					foreach (GlbStaff location in locationResources)
					{
						ZString locationName = location.GS_FullName;
						if (LocationCodePkMap.ContainsKey(locationName))
						{
							locationName = AddSuffixForDuplicateLocation(LocationCodePkMap, locationName);
						}
						LocationCodePkMap.Add(locationName, location.PK);
						LocationPkCodeMap.Add(location.PK, locationName);
						CodeDescriptionPairList.AddPairIfNotExist(locationName, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} ({1})", pair.Description, location.GS_Code));
					}
				}

				CodeDescriptionPairList.SortByDescription();
			}

			static string AddSuffixForDuplicateLocation(Dictionary<ZString, ZGuid> locationMap, string location)
			{
				ZString newLocation;

				int suffixNumber = 1;
				do
				{
					newLocation = location + " (" + suffixNumber + ")";
					suffixNumber++;
				}
				while (locationMap.ContainsKey(newLocation));

				return newLocation;
			}
		}
	}
}
