//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM Autovw_OrgCollectionCallLookups
//
//    This class should be used for overriding collections in Autovw_OrgCollectionCallLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class vw_OrgCollectionCallLookups : Autovw_OrgCollectionCallLookups
	{
		public vw_OrgCollectionCallLookups(Autovw_OrgCollectionCall parent)
			: base(parent)
		{
		}

		#region Branches

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}

				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

		#endregion

		#region Contacts

		public OrgContactDependentCollection Contacts
		{
			get
			{
				if (Parent.Header != null)
				{
					return Parent.Header.Contacts;
				}
				else
				{
					if (fContacts == null)
					{
						fContacts = new OrgContactDependentCollection(Factory);
					}
					return fContacts;
				}
			}
		}

		OrgContactDependentCollection fContacts;

		#endregion

		#region Debtor Group

		public OrgDebtorGroupCollection DebtorGroups
		{
			get
			{
				if (fDebtorGroups == null)
				{
					fDebtorGroups = new OrgDebtorGroupCollection(Factory);
				}

				return fDebtorGroups;
			}
		}

		public OrgDebtorGroupCollection fDebtorGroups;

		#endregion

		protected new OrgCollectionCall Parent
		{
			get { return (OrgCollectionCall)base.Parent; }
		}
	}
}
