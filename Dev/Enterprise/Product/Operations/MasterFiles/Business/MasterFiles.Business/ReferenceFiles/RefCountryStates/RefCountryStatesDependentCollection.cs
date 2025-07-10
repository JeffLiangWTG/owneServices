using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryStatesDependentCollection : BusinessObjectCollection<RefCountryStates>
	{
		readonly RefCountry country;
		public RefCountryStatesDependentCollection(RefCountry parent, BusinessObjectFactory factory)
			: base(factory)
		{
			country = parent;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query;
			if (country == null)
			{
				query = ZQuery.NoResultQuery;
			}
			else
			{
				query = base.CreateAdditionalFilter();
				query.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, country.RN_Code);
			}
			return query;
		}

		protected override bool AllowNewCore => false;
	}
}
