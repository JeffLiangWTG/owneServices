using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class ManifestToOpenCollection : CusSupportingInfoCollection<ManifestToOpen>
	{
		public ManifestToOpenCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.AdditionalInfo)
		{
		}
	}
}
