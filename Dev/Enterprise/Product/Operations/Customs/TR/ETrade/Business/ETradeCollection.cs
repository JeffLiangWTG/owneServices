using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeCollection : BusinessObjectCollection<AsycudaManifestHeader>
	{
		public ETradeCollection(BusinessObjectFactory factory)
			: base(factory, DefaultFilter())
		{
		}

		static ZQuery DefaultFilter()
		{
			var headerQuery = new ZQuery();
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, ApplicationCodeTypeList.Codes.TRETrade);
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Core.Constants.CountryCodes.Turkey);
			return headerQuery;
		}
	}
}
