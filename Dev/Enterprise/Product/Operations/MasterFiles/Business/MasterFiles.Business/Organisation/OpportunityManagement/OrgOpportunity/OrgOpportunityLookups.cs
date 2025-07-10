using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityLookups : AutoOrgOpportunityLookups
	{
		public OrgOpportunityLookups(AutoOrgOpportunity parent) : base(parent)
		{
		}

		protected new OrgOpportunity Parent
		{
			get { return (OrgOpportunity)base.Parent; }
		}

		#region Contacts

		public ActiveOrgContactCollection ActiveContacts
		{
			get
			{
				if (innerContacts == null || lastContactOrgHeaderPK != Parent?.Header?.PK)
				{
					innerContacts = new ActiveOrgContactCollection(Factory, GetContactsFilter(Parent?.Header, Parent?.P8_OC ?? ZGuid.Empty));
					lastContactOrgHeaderPK = Parent?.Header?.PK ?? ZGuid.Empty;
				}
				return innerContacts;
			}
		}

		ZGuid lastContactOrgHeaderPK = ZGuid.Empty;
		ActiveOrgContactCollection innerContacts;

		ZQuery GetContactsFilter(OrgHeader orgHeader, ZGuid contactPk)
		{
			var contactsFilter = new ZQuery(OrgContactSchema.OC_OH, orgHeader?.PK ?? ZGuid.Empty);
			var activeFilter = new ZQuery(OrgContactSchema.OC_IsActive, true);

			if (Parent is null || contactPk.IsEmpty || contactPk == ZGuid.Invalid)
			{
				contactsFilter.AddToFilter(activeFilter);
				return contactsFilter;
			}

			var previousContactPk = new ZQuery(OrgContactSchema.PK, contactPk);
			contactsFilter.AddToFilter(new ZQuery(activeFilter, JoinCondition.Or, previousContactPk));

			return contactsFilter;
		}

		#endregion

		#region Addresses

		public override OrgAddressCollection Addresses
		{
			get
			{
				OrgAddressCollection addresses = new OrgAddressCollection(Factory);
				if (Parent != null && Parent.Header != null)
				{
					Parent.Header.Addresses.CopyToList(addresses);
				}
				return addresses;
			}
		}

		#endregion

		#region Types

		public ReadOnlyCodeDescriptionPairList Types
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.Types", () =>
				{
					return OrganisationsDataRegistry.Instance.OpportunitySalesTypes.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList ActiveTypes
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.ActiveTypes", () =>
				{
					var activeCodeDescriptionPairList = OrganisationsDataRegistry.Instance.OpportunitySalesTypes.Value.GetActiveCodeDescriptionPairList();
					activeCodeDescriptionPairList.SortByDescription();
					return activeCodeDescriptionPairList;
				});
			}
		}

		#endregion

		#region Stages

		public ReadOnlyCodeDescriptionPairList Stages
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.Stages", () =>
				{
					return OrganisationsDataRegistry.Instance.OpportunityStages.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList ActiveStages
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.ActiveStages", () =>
				{
					return OrganisationsDataRegistry.Instance.OpportunityStages.Value.GetActiveCodeDescriptionPairList();
				});
			}
		}

		#endregion

		#region Statuses

		public ICodeDescriptionBoolList Statuses
		{
			get
			{
				return Factory.GetCachedValue<ICodeDescriptionBoolList>("OrgOpportunityLookups.Statuses", () =>
				{
					return OrganisationsDataRegistry.Instance.OpportunityStatus.Value;
				});
			}
		}

		public ICodeDescriptionBoolList ActiveStatuses
		{
			get
			{
				return Factory.GetCachedValue<ICodeDescriptionBoolList>("OrgOpportunityLookups.ActiveStatuses", () =>
				{
					var list = new OpportunityStatusCollection();
					list.AddRange(OrganisationsDataRegistry.Instance.OpportunityStatus.Value.OfType<OpportunityStatus>().Where(x => x.Enabled));
					return list;
				});
			}
		}

		#endregion

		#region OverallDispositions

		public ICodeDescriptionPairList OverallDispositions
		{
			get { return new OrgOpportunityOverallDispositionList(); }
		}

		#endregion

		#region Outcomes

		public ReadOnlyCodeDescriptionPairList Outcomes
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.Outcomes", () =>
				{
					return OrganisationsDataRegistry.Instance.OpportunityOutcome.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList ActiveOutcomes
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.ActiveOutcomes", () =>
				{
					return OrganisationsDataRegistry.Instance.OpportunityOutcome.Value.GetActiveCodeDescriptionPairList();
				});
			}
		}

		#endregion

		#region Sources

		public ReadOnlyCodeDescriptionPairList ActiveSources
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.ActiveSources", () =>
				{
					var activeCodeDescriptionPairList = OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetActiveCodeDescriptionPairList();
					activeCodeDescriptionPairList.SortByDescription();
					return activeCodeDescriptionPairList;
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList Sources
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.Sources", () =>
				{
					return OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetCodeDescriptionPairList();
				});
			}
		}

		#endregion

		#region Source Details

		public ReadOnlyCodeDescriptionPairList ActiveSourceDetails
		{
			get
			{
				return OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetActiveRelatedItemList(Parent.P8_Source);
			}
		}

		public ReadOnlyCodeDescriptionPairList SourceDetails
		{
			get
			{
				return OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetRelatedItemList(Parent.P8_Source);
			}
		}

		#endregion

		#region Extra Categories

		public ReadOnlyCodeDescriptionPairList ActiveExtraCategories
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.ActiveExtraCategories", () =>
				{
					var activeCodeDescriptionPairList = OrganisationsDataRegistry.Instance.ProductTypeList.Value.GetActiveCodeDescriptionPairList();
					activeCodeDescriptionPairList.SortByDescription();
					return activeCodeDescriptionPairList;
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList ExtraCategories
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.ExtraCategories", () =>
				{
					return OrganisationsDataRegistry.Instance.ProductTypeList.Value.GetCodeDescriptionPairList();
				});
			}
		}

		#endregion

		#region Sales Organisations

		public override OrgHeaderCollection Headers
		{
			get { return new SalesOrganisationCollection(Factory); }
		}

		#endregion

		#region Organisations

		public OrganisationsFindBoxCollection Orgs
		{
			get
			{
				if (fOrgs == null)
				{
					fOrgs = new OrganisationsFindBoxCollection(Factory);
				}
				return fOrgs;
			}
		}

		OrganisationsFindBoxCollection fOrgs;

		#endregion

		#region Contacts of Referring Organisation

		public OrgContactCollection ContactsOfReferringOrg
		{
			get
			{
				if (innerReferringContacts == null || lastReferringContactOrgHeaderPK != Parent?.ReferringOrganisation?.PK || Parent?.P8_OC_ReferringContactInfo.HasChanges == true)
				{
					innerReferringContacts = new OrgContactCollection(Factory, GetContactsFilter(Parent?.ReferringOrganisation, Parent?.P8_OC_ReferringContact ?? ZGuid.Empty));
					lastReferringContactOrgHeaderPK = Parent?.ReferringOrganisation?.PK ?? ZGuid.Empty;
				}
				return innerReferringContacts;
			}
		}

		ZGuid lastReferringContactOrgHeaderPK = ZGuid.Empty;
		OrgContactCollection innerReferringContacts;

		public OrgContactCollection ActiveContactsOfReferringOrg
		{
			get
			{
				OrgContactCollection contacts = new OrgContactCollection(Factory);
				OrgHeader referringOrg = Parent != null ? Parent.ReferringOrganisation : null;
				if (referringOrg != null)
				{
					referringOrg.ContactsActive.CopyToList(contacts);
				}
				return contacts;
			}
		}

		#endregion

		#region AssignedOfficeContacts

		public override OrgContactCollection AssignedOfficeContacts
		{
			get
			{
				if (innerAssignedOfficeContacts == null || lastAssignedOfficeContactOrgHeaderPK != Parent?.AssignedOrg?.PK || Parent?.P8_OC_AssignedOfficeContactInfo.HasChanges == true)
				{
					if (Parent.P8_OA_AssignedOffice != ZGuid.Empty && Parent.AssignedOffice != null)
					{
						innerAssignedOfficeContacts = new OrgContactCollection(Factory, GetContactsFilter(Parent?.AssignedOrg, Parent?.P8_OC_AssignedOfficeContact ?? ZGuid.Empty));
					}
					else
					{
						innerAssignedOfficeContacts = new OrgContactCollection(Factory, ZQuery.NoResultQuery);
					}
					lastAssignedOfficeContactOrgHeaderPK = Parent?.AssignedOrg?.PK ?? ZGuid.Empty;
				}
				return innerAssignedOfficeContacts;
			}
		}

		ZGuid lastAssignedOfficeContactOrgHeaderPK = ZGuid.Empty;
		OrgContactCollection innerAssignedOfficeContacts;

		#endregion

		#region P8_OA_AssignedOffice

		public override OrgAddressCollection AssignedOffices
		{
			get
			{
				OrgAddressCollection result = new OrgAddressCollection(Factory);
				if (Parent.AssignedOrg != null)
				{
					result.Load(new ZQuery(OrgAddressSchema.OA_OH, Parent.AssignedOrg.PK));
				}
				return result;
			}
		}

		#endregion

		#region Campaigns

		public GlbCompanyCampaignCollection Campaigns
		{
			get { return new GlbCompanyCampaignCollection(Factory); }
		}

		#endregion

		#region Close Reasons

		public ReadOnlyCodeDescriptionPairList CloseReasons
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.CloseReasons", () =>
				{
					return OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList ActiveCloseReasons
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityLookups.ActiveCloseReasons", () =>
				{
					return OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.Value.GetActiveCodeDescriptionPairList();
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList ActiveCloseReasonsByStatus
		{
			get
			{
				return Factory.GetCachedValue(FormattableString.Invariant($"OrgOpportunityLookups.ActiveCloseReasonsByStatus.{Parent.P8_Status}"), () =>
				{
					return OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.Value.GetActiveCodeDescriptionPairList(Parent.P8_Status);
				});
			}
		}

		#endregion
	}
}
