namespace Enterprise.Customs.NO.Business
{
	public class GroupInvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public GroupInvoiceChargeLookups(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		protected new GroupInvoiceCharge Parent => (GroupInvoiceCharge)base.Parent;
	}
}
