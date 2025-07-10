using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefTariffVersionCollection : ActiveBusinessObjectCollection<CusRefTariffVersion>
	{
		public CusRefTariffVersionCollection(BusinessObjectFactory factory) : this(factory, ZString.Empty)
		{
		}

		public CusRefTariffVersionCollection(BusinessObjectFactory factory, ZString countryCode)
			: base(factory, GetFilter(countryCode))
		{
		}

		static ZQuery GetFilter(ZString countryCode)
		{
			var query = new ZQuery();
			if (!countryCode.IsEmpty)
			{
				query.AddToFilter(CusRefTariffVersionSchema.CRT_RN_NKCountryCode, countryCode);
			}
			return query;
		}
	}
}
