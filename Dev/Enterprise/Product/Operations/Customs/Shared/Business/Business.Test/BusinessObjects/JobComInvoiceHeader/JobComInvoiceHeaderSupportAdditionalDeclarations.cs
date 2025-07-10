using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvoiceHeaderSupportAdditionalDeclarations : BaseJobComInvoiceHeader
	{
		public JobComInvoiceHeaderSupportAdditionalDeclarations(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportAdditionalDeclarations
		{
			get { return true; }
		}
	}
}
