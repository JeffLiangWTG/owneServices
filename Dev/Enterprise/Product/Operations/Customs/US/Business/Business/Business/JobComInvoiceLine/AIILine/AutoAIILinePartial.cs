namespace Enterprise.Customs.US.Business
{
	partial class AutoAIILine : Customs.Business.MultiLineAddInfos.CusAddInfoWithAutoDelete<USAIILineAddInfo>
	{
		public bool US_98GoodsValue_ReadOnly
		{
			get { return US_98GoodsValue_ReadOnlyCore; }
		}

		protected virtual bool US_98GoodsValue_ReadOnlyCore
		{
			get { return false; }
		}
	}
}
