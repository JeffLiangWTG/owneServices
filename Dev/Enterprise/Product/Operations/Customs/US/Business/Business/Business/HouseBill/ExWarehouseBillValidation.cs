namespace Enterprise.Customs.US.Business
{
	public class ExWarehouseBillValidation : CusDecHouseBillValidation
	{
		public ExWarehouseBillValidation(Bill bill)
			: base(bill)
		{
		}

		protected override void CheckCU_BillNum()
		{
			//no validation
		}

		protected override void CheckCU_CU_ParentBill()
		{
			//no validation
		}
	}
}
