using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class FoodDataCollection : CusCodeDataCollection<FoodData>
	{
		public FoodDataCollection(JobComInvoiceLine jobComInvoiceLine)
		: base(jobComInvoiceLine, CusCodeDataTypeList.Codes.Food)
		{
		}
	}
}
