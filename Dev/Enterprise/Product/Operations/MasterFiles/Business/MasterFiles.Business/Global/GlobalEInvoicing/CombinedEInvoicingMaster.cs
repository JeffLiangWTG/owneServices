using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public struct CombinedEInvoicingMaster
	{
		public CombinedEInvoicingMaster(GlbCompany company)
		{
			Value = company;
		}

		public CombinedEInvoicingMaster(GlbBranch branch)
		{
			Value = branch;
		}

		public readonly BusinessObject Value;

		public GlbCompany AsCompany => Value as GlbCompany;
		public GlbBranch AsBranch => Value as GlbBranch;
	}
}
