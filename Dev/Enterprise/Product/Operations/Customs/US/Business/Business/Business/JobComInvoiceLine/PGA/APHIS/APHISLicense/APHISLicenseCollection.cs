using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class APHISLicenseCollection : DependentCusAddInfoCollection<APHISLicense, APHISHeader>
	{
		public APHISLicenseCollection(APHISHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAPHISLicense)
		{
		}

		protected override bool AllowNewCore
		{
			get { return !Master.IsRelatedAnimalProductsCategory; }
		}
	}
}
