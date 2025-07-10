using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class BondedWarehouseTransaction : Customs.Business.BondedWarehouseTransaction
	{
		public BondedWarehouseTransaction(JobDeclaration dec)
			: base(dec)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override OrgAddress WarehouseAddressCore
		{
			get { return null; }
		}
	}
}
