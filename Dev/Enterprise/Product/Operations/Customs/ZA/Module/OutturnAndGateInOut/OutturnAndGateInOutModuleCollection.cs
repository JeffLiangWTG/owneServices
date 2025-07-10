using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public class OutturnAndGateInOutModuleCollection : BusinessObjectCollection<AsycudaManifestHeader>
	{
		public OutturnAndGateInOutModuleCollection(BusinessObjectFactory factory)
			: base(factory, DefaultFilter())
		{
		}

		static ZQuery DefaultFilter()
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, AsycudaManifestHeader.ApplicationCode_Out);
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.SouthAfrica);
			return headerQuery;
		}
	}
}
