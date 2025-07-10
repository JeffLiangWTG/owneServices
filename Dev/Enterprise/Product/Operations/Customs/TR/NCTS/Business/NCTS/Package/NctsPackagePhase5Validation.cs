namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsPackagePhase5Validation : EU.NCTS.Business.NctsPackagePhase5Validation
	{
		public NctsPackagePhase5Validation(NctsPackage parent)
			: base(parent)
		{
		}

		public new NctsPackage Parent => (NctsPackage)base.Parent;
	}
}
