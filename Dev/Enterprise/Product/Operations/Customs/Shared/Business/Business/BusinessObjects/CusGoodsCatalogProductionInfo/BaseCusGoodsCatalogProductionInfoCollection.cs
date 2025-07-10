using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IBaseCusGoodsCatalogProductionInfoCollection<out T> : IActiveBusinessObjectCollection<T>
	where T : BaseCusGoodsCatalogProductionInfo
	{
		new T this[int index] { get; }
	}

	public class BaseCusGoodsCatalogProductionInfoCollection<T> : ActiveBusinessObjectCollection<T>, IBaseCusGoodsCatalogProductionInfoCollection<T> where T : BaseCusGoodsCatalogProductionInfo
	{
		public BaseCusGoodsCatalogProductionInfoCollection(BaseCusGoodsCatalog catalog, string type) : base(catalog.Factory, catalog, new ZQuery(CusGoodsCatalogProductionInfoSchema.CGI_Type, type), CusGoodsCatalogProductionInfoSchema.CGI_CGC_Catalog)
		{
		}
	}
}

