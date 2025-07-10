using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class PersonAssociationsTreeModel : ZTreeModel<PersonAssociationsTreeBizObjWrapper>
	{
		public PersonAssociationsTreeModel(GlbPerson person)
			: base(person.Factory)
		{
			this.person = person;

			BuildTree(false);
		}

		readonly GlbPerson person;
		readonly ZNodeCollection<PersonAssociationsTreeBizObjWrapper> rootNodes = new ZNodeCollection<PersonAssociationsTreeBizObjWrapper>();

		public GlbPerson Person => person;

		#region Build Tree

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void BuildTree(bool showInactive, bool needRefreshCollection = true)
		{
			rootNodes.Clear();

			var treeData = new List<PersonAssociationsTreeData>();

			LoadRelatedParties(showInactive, treeData);
			LoadOrgGroups(treeData);
			LoadStaff(showInactive, needRefreshCollection, person.StaffCollection);
			LoadJobApplicant(needRefreshCollection, person.ApplicantCollection);
		}

		void LoadRelatedParties(bool showInactive, List<PersonAssociationsTreeData> treeData)
		{
			var orgContacts = Factory.Load<OrgContact>(new ZQuery(OrgContactSchema.OC_PER, person.PK)).ToList();
			foreach (var oc in orgContacts)
			{
				if (showInactive || oc.OC_IsActive)
				{
					var org = Factory.Load<OrgHeader>(oc.OC_OH);

					var refUNLOCO = org.ClosestPort;
					var country = refUNLOCO?.Country;

					var query = new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, org.PK);
					query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);

					var orgRelatedParties = Factory.Load<OrgRelatedParty>(query);

					var totalOrgRelatedParties = orgRelatedParties.Count(x => x.PR_OH_Parent != x.PR_OH_RelatedParty);
					if (totalOrgRelatedParties > 0)
					{
						foreach (var orp in orgRelatedParties)
						{
							var orgRelatedParty = Factory.Load<OrgHeader>(orp.PR_OH_RelatedParty);
							refUNLOCO = orgRelatedParty.ClosestPort;
							country = refUNLOCO?.Country;
							treeData.Add(new PersonAssociationsTreeData(org, country, orgRelatedParty, oc));
						}
					}
					else if (!treeData.Any(x => x.OrgRelatedParty == org && x.Contact == oc))
					{
						treeData.Add(new PersonAssociationsTreeData(org, country, org, oc));
					}
				}
			}
		}

		void LoadOrgGroups(List<PersonAssociationsTreeData> treeData)
		{
			var dataGroupedByOrg = treeData.GroupBy(y => y.OrgRelatedParty.OH_FullName).OrderBy(x => x.Key);
			foreach (var dataGroupedByOrgEntry in dataGroupedByOrg)
			{
				var organizationWrapper = new List<PersonAssociationsOrganizationWrapper>();

				var sortedOrgRelatedParties = dataGroupedByOrgEntry.OrderBy(s => s.OrgRelatedParty.OH_FullName);
				foreach (var sortedByNameParty in sortedOrgRelatedParties)
				{
					organizationWrapper.Add(
						new PersonAssociationsOrganizationWrapper(this, sortedByNameParty.Org, sortedByNameParty.Contact));
				}

				var orgGroupTreeData = dataGroupedByOrgEntry.FirstOrDefault();
				if (orgGroupTreeData != null)
				{
					rootNodes.Add(new PersonAssociationsTreeNode(this,
						new PersonAssociationsOrgGroupWrapper(this, orgGroupTreeData.OrgRelatedParty, organizationWrapper)));
				}
			}
		}

		void LoadStaff(bool showInactive, bool needRefreshCollection, GlbStaffCollection staffCollection)
		{
			if (needRefreshCollection)
			{
				staffCollection.RefreshFromDb();
			}

			foreach (var staff in staffCollection)
			{
				if (showInactive || staff.GS_IsActive)
				{
					rootNodes.Add(new PersonAssociationsTreeNode(this, new PersonAssociationsStaffWrapper(this, staff)));
				}
			}
		}

		void LoadJobApplicant(bool needRefreshCollection, BusinessObjectCollection applicantsCollection)
		{
			if (needRefreshCollection)
			{
				applicantsCollection.Reload(false);
			}

			foreach (IHRJobApplicant applicant in applicantsCollection)
			{
				rootNodes.Add(new PersonAssociationsTreeNode(this, new PersonAssociationsJobApplicantWrapper(this, applicant)));
			}
		}

		#endregion

		#region PrimaryLocation

		internal ZString PrimaryLocation => person.PER_RN_NKCountry;

		#endregion

		#region Nodes

		protected override ZNode<PersonAssociationsTreeBizObjWrapper> CreateNewNodeCore(ZTreeModel<PersonAssociationsTreeBizObjWrapper> treeModel, PersonAssociationsTreeBizObjWrapper bizObj)
		{
			return new PersonAssociationsTreeNode((PersonAssociationsTreeModel)treeModel, bizObj);
		}

		protected override ZNodeCollection<PersonAssociationsTreeBizObjWrapper> GetRootNodes()
		{
			return rootNodes;
		}

		#endregion
	}

	#region PersonAssociationsTreeData

	internal class PersonAssociationsTreeData
	{
		public PersonAssociationsTreeData(OrgHeader org, RefCountry country, OrgHeader orgRelatedParty, OrgContact contact)
		{
			this.Country = country;
			this.Org = org;
			this.OrgRelatedParty = orgRelatedParty;
			this.Contact = contact;
		}

		public RefCountry Country { get; }

		public OrgHeader Org { get; }

		public OrgHeader OrgRelatedParty { get; }

		public OrgContact Contact { get; }
	}

	#endregion
}
