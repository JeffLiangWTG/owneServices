using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyLookups : AutoGlbCompanyLookups
	{
		public GlbCompanyLookups(AutoGlbCompany parent) : base(parent)
		{
		}

		#region OrgProxies

		public new ForwarderOrBrokerOrCarrierOrServicesCollection OrgProxies
		{
			get { return new ForwarderOrBrokerOrCarrierOrServicesCollection(Factory); }
		}

		#endregion

		#region Branches

		GlbBranchCollection fBranch_List;
		public GlbBranchCollection Branch_List
		{
			get
			{
				if (fBranch_List == null)
				{
					fBranch_List = new GlbBranchCollection(Factory);
				}
				return fBranch_List;
			}
		}

		#endregion

		#region State

		public CodeDescriptionPairList StateList
		{
			get
			{
				var result = new UntranslatableCodeDescriptionPairList((NoResString)"States from RefCountryState table");
				var country = ((AutoGlbCompany)Parent).Country;
				var stateList = (country != null) ? new OrgCodeLists().State_List(country) : new CodeDescriptionPairList();
				result.AddRange(stateList);

				return result;
			}
		}

		#endregion
	}
}
