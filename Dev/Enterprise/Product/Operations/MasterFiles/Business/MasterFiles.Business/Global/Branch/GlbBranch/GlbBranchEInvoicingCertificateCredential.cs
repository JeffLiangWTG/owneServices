using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public sealed class GlbBranchEInvoicingCertificateCredential : EInvoicingCertificateCredential
	{
		public GlbBranchEInvoicingCertificateCredential(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_GB = GlbBranch.CurrentBranch.PK;
		}
	}
}
