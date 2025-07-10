using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgLandedCostingPrefsLookups : AutoOrgLandedCostingPrefsLookups
	{
		public OrgLandedCostingPrefsLookups(AutoOrgLandedCostingPrefs parent)
			: base(parent)
		{
		}

		#region Cost Distribution Mechanisms

		public CodeDescriptionPairList CostDistributionMechanisms
		{
			get { return new CostDistributionMechanismList(); }
		}

		#endregion
	}
}
