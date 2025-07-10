namespace Enterprise.Customs.TW.Business
{
	public class CusPackableItemCollection : Customs.Business.CusPackableItemCollection
	{
		public CusPackableItemCollection(CusPackingList packingList)
			: base(packingList)
		{
		}

		public new CusPackableItem AddNew()
		{
			return (CusPackableItem)base.AddNew();
		}

		public new CusPackableItem this[int index]
		{
			get
			{
				return (CusPackableItem)base[index];
			}
		}
	}
}
