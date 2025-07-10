using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class APHISIdentityNumberRangeCollection : DependentCusAddInfoCollection<APHISIdentityNumberRange, APHISIdentity>
	{
		public APHISIdentityNumberRangeCollection(APHISIdentity master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAPHISIdentityNumberRange)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var master = Master;
				return master != null && master.UseMultipleNumbers;
			}
		}
	}
}
