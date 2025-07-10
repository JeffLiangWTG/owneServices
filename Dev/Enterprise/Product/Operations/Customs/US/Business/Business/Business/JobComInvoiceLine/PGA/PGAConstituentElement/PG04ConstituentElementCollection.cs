using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class PG04ConstituentElementCollection : DependentCusAddInfoCollection<ConstituentElement, BusinessObject>
	{
		public PG04ConstituentElementCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USPGA)
		{
		}
	}
}
