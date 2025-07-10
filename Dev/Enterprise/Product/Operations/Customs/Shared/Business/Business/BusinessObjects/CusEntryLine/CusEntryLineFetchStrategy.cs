using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class CusEntryLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusEntryLineFetchStrategy(CusEntryLine line)
			: base(line)
		{
		}

		internal CusEntryLine Line
		{
			get { return (CusEntryLine)BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(typeof(CusEntryHeader), Line.CL_CH);
			Factory.AddFetchHint(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL, Line.PK);
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(CusEntryHeader), Line.CL_CH);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL, Line.PK);
			Factory.AddFetchHint(CusEntryLineFeeSchema.CF_CL, BusinessObject.PK);
			Factory.AddFetchHint(CusUnderbondDecSchema.BU_CL, BusinessObject.PK);
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_CL, Line.PK);
			Factory.AddFetchHint(CusEntryLineFeeSchema.CF_CL, BusinessObject.PK);
			Factory.AddFetchHint(CusUnderbondDecSchema.BU_CL, BusinessObject.PK);
		}
	}
}
