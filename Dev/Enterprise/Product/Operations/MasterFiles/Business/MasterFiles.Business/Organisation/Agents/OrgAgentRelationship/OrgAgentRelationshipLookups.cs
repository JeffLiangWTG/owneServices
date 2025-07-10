using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAgentRelationshipLookups : AutoOrgAgentRelationshipLookups
	{
		public OrgAgentRelationshipLookups(AutoOrgAgentRelationship parent) : base(parent)
		{
		}

		#region ReceivingAgents

		public new ForwarderCollection ReceivingAgents
		{
			get { return new ForwarderCollection(Factory); }
		}

		#endregion

		#region SendingAgents

		public new ForwarderCollection SendingAgents
		{
			get { return new ForwarderCollection(Factory); }
		}

		#endregion

		#region GroupNetworkOrFranchises

		public new OrganisationsFindBoxCollection GroupNetworkOrFranchises
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region Profit Share Type

		public CodeDescriptionPairList ProfitShareTypeList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(OrgAgentRelationship.ProfitShareTypes.Standard, Res.GetString("MasterFiles|ProfitShareTypeList|Standard", "Standard Agent Agreement"));
				list.AddPair(OrgAgentRelationship.ProfitShareTypes.AgencyProfile, Res.GetString("MasterFiles|ProfitShareTypeList|AgencyProfile", "Single Agency Profile"));
				return list;
			}
		}

		#endregion
	}
}
