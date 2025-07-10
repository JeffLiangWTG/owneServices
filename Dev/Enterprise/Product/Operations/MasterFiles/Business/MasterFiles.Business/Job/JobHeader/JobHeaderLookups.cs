//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobHeaderLookups
//
//    This class should be used for overriding collections in AutoJobHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class JobHeaderLookups : AutoJobHeaderLookups
	{
		public JobHeaderLookups(AutoJobHeader parent) : base(parent)
		{
		}

		#region Tax Branch

		public override GlbBranchCollection TaxBranches
		{
			get { return AccountingMasterFilesUtils.GetBranchesOfCurrentCompany(Factory); }
		}

		#endregion

		public OrgHeaderCollection LocalCharges
		{
			get
			{
				return new OrganisationsFindBoxCollection(Factory);
			}
		}

		public OrgHeaderCollection AgentCollects
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}
	}
}
