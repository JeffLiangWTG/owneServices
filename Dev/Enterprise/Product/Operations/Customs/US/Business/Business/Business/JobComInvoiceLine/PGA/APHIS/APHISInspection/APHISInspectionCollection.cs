using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class APHISInspectionCollection : DependentCusAddInfoCollection<APHISInspection, APHISHeader>
	{
		public APHISInspectionCollection(APHISHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAPHISInspection)
		{
		}
	}
}
