using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class BaseBillFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public BaseBillFetchStrategy(Bill bill)
			: base(bill)
		{
		}

		Bill Bill
		{
			get { return (Bill)BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(BaseJobDeclaration), Bill.CU_JE);
		}

		protected override void FetchForViewCore(CargoWise.EntityFramework.TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(typeof(Bill), Bill.CU_CU_ParentBill);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(Bill), Bill.CU_CU_ParentBill);
			Factory.AddFetchHint(CusDecHouseBillSchema.CU_CU_ParentBill, Bill.PK);
			Factory.AddFetchHint(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, BusinessObject.PK);
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill, BusinessObject.PK);
		}
	}
}
