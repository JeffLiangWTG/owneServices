namespace Enterprise.Customs.US.Business
{
	public class BillContainerCollection : Customs.Business.BaseBillContainerCollection
	{
		public BillContainerCollection(Bill bill)
			: base(bill)
		{
		}

		public new CusContainer this[int index]
		{
			get { return (CusContainer)base[index]; }
		}

		public new CusContainer AddNew()
		{
			return (CusContainer)base.AddNew();
		}
	}
}
