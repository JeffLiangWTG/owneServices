using CargoWise.EntityFramework;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Module
{
	public class AsycudaManifestModuleCollection : BusinessObjectCollection<AsycudaManifestHeader>
	{
		public AsycudaManifestModuleCollection(BusinessObjectFactory factory) : base(factory, DefaultFilter())
		{
		}

		static ZQuery DefaultFilter()
		{
			var headerQuery = new ZQuery();
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration);
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.Taiwan);
			return headerQuery;
		}
	}
}
