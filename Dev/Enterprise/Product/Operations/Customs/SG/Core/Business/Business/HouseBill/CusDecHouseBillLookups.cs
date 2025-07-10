
namespace Enterprise.Customs.SG.V4.Business
{
	public class CusDecHouseBillLookups : Customs.Business.CusDecHouseBillLookups
	{
		public CusDecHouseBillLookups(Bill houseBill)
			: base(houseBill)
		{
		}

		public Bill HouseBill
		{
			get { return Parent; }
		}

		protected new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}
	}
}
