namespace Enterprise.Customs.US.Business
{
	public class HouseBillRefNoCollection : Customs.Business.CusCodeDataCollection<HouseBillRefNo>
	{
		public HouseBillRefNoCollection(Bill bill)
			: base(bill, CusCodeDataTypeList.Codes.HouseBillRefNo)
		{
		}
	}
}
