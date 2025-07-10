namespace Enterprise.Customs.TW.Business
{
	public class GroupInvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public GroupInvoiceChargeLookups(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		public new GroupInvoiceCharge Parent
		{
			get { return (GroupInvoiceCharge)base.Parent; }
		}
	}
}
