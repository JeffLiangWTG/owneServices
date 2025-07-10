using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class FDALicenseCollection : DependentCusAddInfoCollection<FDALicense, ACEFDA>
	{
		public FDALicenseCollection(ACEFDA master)
			: base(master, CusAddInfoTypeAttribute.Codes.USFDALicense)
		{
		}
	}
}
