//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCollectionNoteLookups
//
//    This class should be used for overriding collections in AutoOrgCollectionNoteLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCollectionNoteLookups : AutoOrgCollectionNoteLookups
	{
		public OrgCollectionNoteLookups(AutoOrgCollectionNote parent)
			: base(parent)
		{
		}

		#region Call Status List

		public CodeDescriptionPairList CallStatusList
		{
			get
			{
				if (fCallStatusList == null)
				{
					fCallStatusList = new CollectionNoteStatusList();
				}

				return fCallStatusList;
			}
		}

		CodeDescriptionPairList fCallStatusList;

		#endregion

		#region Call Disposition List

		public CodeDescriptionPairList CallDispositionList
		{
			get
			{
				if (fCallDispositionList == null)
				{
					fCallDispositionList = new CollectionNoteDispositionList();
				}

				return fCallDispositionList;
			}
		}
		CodeDescriptionPairList fCallDispositionList;

		#endregion

		#region Dependent Contacts

		public OrgContactDependentCollection DependentContacts
		{
			get
			{
				if (Parent.Header != null)
				{
					return Parent.Header.Contacts;
				}
				else
				{
					if (fDependentContacts == null)
					{
						fDependentContacts = new OrgContactDependentCollection(Factory);
					}
					return fDependentContacts;
				}
			}
		}
		OrgContactDependentCollection fDependentContacts;

		#endregion

		#region Staff Members

		public GlbStaffCollection StaffMembers
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffCollection(Factory);
				}

				return fStaff;
			}
		}

		GlbStaffCollection fStaff;

		#endregion

		#region Organisations

		public OrganisationsFindBoxCollection Organisations_List
		{
			get
			{
				if (fOrganisations_List == null)
				{
					fOrganisations_List = new OrganisationsFindBoxCollection(Factory);
				}
				return fOrganisations_List;
			}
		}

		OrganisationsFindBoxCollection fOrganisations_List;

		#endregion

		new OrgCollectionNote Parent
		{
			get { return (OrgCollectionNote)base.Parent; }
		}
	}
}
