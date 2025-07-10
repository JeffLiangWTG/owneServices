using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class JobDeclarationSupportAdditionalInvoices : BaseJobDeclaration
	{
		public JobDeclarationSupportAdditionalInvoices(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportAdditionalInvoices
		{
			get
			{
				return true;
			}
		}

		protected override InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);
		}
	}
}
