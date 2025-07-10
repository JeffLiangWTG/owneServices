using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
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

		public override CodeDescriptionPairList ChargeTypeList
		{
			get { return chargeTypeList ?? (chargeTypeList = new ChargeTypeList()); }
		}
		ChargeTypeList chargeTypeList;
	}
}
