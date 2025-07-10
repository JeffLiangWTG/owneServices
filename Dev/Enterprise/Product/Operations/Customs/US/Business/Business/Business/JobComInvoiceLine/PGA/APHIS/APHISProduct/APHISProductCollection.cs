using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class APHISProductCollection : DependentCusAddInfoCollection<APHISProduct, APHISHeader>
	{
		public APHISProductCollection(APHISHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAPHISProduct)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				return Master.IsLiveAnimalsCategory || (Master.IsAnimalProductsAndAnimalByProductsCategory && !Master.IsSendProductWithOutCharacteristicCategoryCode) || Master.IsAPQProgramType;
			}
		}
	}
}
