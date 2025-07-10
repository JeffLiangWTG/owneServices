using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvoiceGroupHeaderSupportAdditionalDeclarations : BaseJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeaderSupportAdditionalDeclarations(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportAdditionalDeclarations
		{
			get { return true; }
		}
	}
}
