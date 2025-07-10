using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusContainerInvoiceLinePivot : Customs.Business.CusContainerInvoiceLinePivot, Integration.Customs.NZ.ICusContainerInvoiceLinePivot
	{
		public CusContainerInvoiceLinePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related BO's
		public new CusContainer Container
		{
			get { return (CusContainer)base.Container; }
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}
		#endregion

		public new CusContainerInvoiceLinePivotValidation Validation
		{
			get { return (CusContainerInvoiceLinePivotValidation)base.Validation; }
		}

		protected override Customs.Business.CusContainerInvoiceLinePivotValidation GetNewValidation()
		{
			return new CusContainerInvoiceLinePivotValidation(this);
		}
	}
}
