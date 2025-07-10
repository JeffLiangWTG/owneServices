using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxConfigurationCollection : ActiveBusinessObjectCollection<AccTaxConfiguration>
	{
		public AccTaxConfigurationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccTaxConfigurationCollection(BusinessObjectFactory factory, ZString countryCode)
		: base(factory, CreateCountryFilter(countryCode))
		{
		}

		public AccTaxConfigurationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public AccTaxConfigurationCollection(BusinessObjectFactory factory, BusinessObject parent, ZQuery filter, SchemaGuidColumn parentId)
			: base(factory, parent, filter, parentId)
		{
		}

		static ZQuery CreateCountryFilter(ZString countryCode) =>
			!countryCode.IsEmpty
			? new ZQuery(AccTaxConfigurationSchema.ETC_RN_NKCountry, countryCode)
			: new ZQuery();
	}
}
