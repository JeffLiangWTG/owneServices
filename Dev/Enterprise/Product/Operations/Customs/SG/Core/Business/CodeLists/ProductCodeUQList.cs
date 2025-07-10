namespace Enterprise.Customs.SG.V4.Business
{
	public class ProductCodeUQList : UnitOfQuantityCodeList
	{
		public ProductCodeUQList()
			: base()
		{
			AddPair("HDS", ResString.GetMultilingualString("55C8BC50-37C2-44EF-B7C1-CD72943E6A2D", "HDS"));
			AddPair("PCE", ResString.GetMultilingualString("E2C7B4F1-973C-4456-A052-9E46E834F562", "Pieces"));
			AddPair("PCS", ResString.GetMultilingualString("5932B838-07A1-4148-A417-08273D81D321", "Pieces"));
			AddPair("KGS", ResString.GetMultilingualString("FEA28537-9A43-45CF-929E-7BE396A0D601", "Kilograms"));
			AddPair("MC", ResString.GetMultilingualString("D4AB6AC3-16B5-4E11-BA10-592C79F9842B", "MC"));
			Sort();
		}
	}
}
