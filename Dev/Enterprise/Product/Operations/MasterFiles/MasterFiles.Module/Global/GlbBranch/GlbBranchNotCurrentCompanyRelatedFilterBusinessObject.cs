using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class GlbBranchNotCurrentCompanyRelatedFilterBusinessObject : GlbBranchFilterBusinessObject
	{
		#region Overriden

		protected override FilterVisibility CompanyFilterVisibility()
		{
			return FilterVisibility.Visible;
		}

		#endregion
	}
}
