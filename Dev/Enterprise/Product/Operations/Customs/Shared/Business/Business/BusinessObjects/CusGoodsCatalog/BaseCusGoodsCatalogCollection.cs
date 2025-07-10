using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IBaseCusGoodsCatalogCollection<out T> : IBusinessObjectCollection<T>
		where T : BaseCusGoodsCatalog
	{
		new T this[int index] { get; }
	}

	[ModuleID(ModuleId.GoodsCatalog)]
	public class BaseCusGoodsCatalogCollection<T> : BusinessObjectCollection<T>, IBaseCusGoodsCatalogCollection<T> where T : BaseCusGoodsCatalog
	{
		public BaseCusGoodsCatalogCollection(BusinessObjectFactory factory) : this(factory, GlbCompany.CurrentCompany.PK)
		{
		}

		public BaseCusGoodsCatalogCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, GetCompanyQuery(companyPkToFilterOn))
		{
		}

		static ZQuery GetCompanyQuery(ZGuid companyPkToFilterOn)
		{
			return new ZQuery(CusGoodsCatalogSchema.CGC_GC_Company, companyPkToFilterOn);
		}

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();
	}
}
