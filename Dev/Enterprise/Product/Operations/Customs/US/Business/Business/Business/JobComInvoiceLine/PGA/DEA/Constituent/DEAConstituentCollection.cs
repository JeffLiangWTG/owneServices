using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class DEAConstituentCollection : DependentCusAddInfoCollection<DEAConstituent, BusinessObject>
	{
		public DEAConstituentCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDEAConstituent)
		{
		}
	}
}
