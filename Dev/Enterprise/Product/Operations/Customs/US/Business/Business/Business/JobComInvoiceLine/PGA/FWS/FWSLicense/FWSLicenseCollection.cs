using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class FWSLicenseCollection : DependentCusAddInfoCollection<FWSLicense, FWSHeader>
	{
		public FWSLicenseCollection(FWSHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USFWSLicense)
		{
		}
	}
}
