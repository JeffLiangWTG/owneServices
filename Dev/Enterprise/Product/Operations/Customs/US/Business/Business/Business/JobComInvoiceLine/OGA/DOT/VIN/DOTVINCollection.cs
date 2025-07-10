using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DOTVINCollection : DependentCusAddInfoCollection<DOTVIN, DOT>
	{
		public DOTVINCollection(DOT master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDOTVIN)
		{
		}
	}
}
