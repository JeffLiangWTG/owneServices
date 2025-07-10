using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business
{
	[UniversalDataContext(DataContextType.AccEInvoicingCredential)]
	public sealed class GlbCompanyEInvoicingCertificateCredential : EInvoicingCertificateCredential
	{
		public GlbCompanyEInvoicingCertificateCredential(BusinessObjectFactory factory, System.Data.DataRow row)
		: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_GC = GlbCompany.CurrentCompany.PK;
		}
	}
}
