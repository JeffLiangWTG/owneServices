using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CASCCodeValidation : Customs.Business.CusCodeDataValidation
	{
		public CASCCodeValidation(CASCCode parent)
			: base(parent)
		{
		}

		protected JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Parent.Factory.Load<JobComInvoiceLine>(Parent.CY_ParentID)); }
		}
		JobComInvoiceLine invoiceLine;

		protected override void CheckCY_Data()
		{
			MandatoryValidation.CheckEntered(Parent.CY_DataInfo, "CA/SC Code");
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
