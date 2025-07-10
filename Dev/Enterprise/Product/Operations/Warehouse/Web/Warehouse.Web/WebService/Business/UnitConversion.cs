namespace Enterprise.Warehouse.Web.WebService.Business
{
	public class UnitConversion
	{
		public UnitConversion()
		{
			PackType = "";
		}

		public UnitConversion(string packType, decimal qty)
		{
			PackType = packType;
			Qty = qty;
		}

		public string PackType { get; set; }
		public decimal Qty { get; set; }
	}
}
