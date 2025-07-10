using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefPostCodeLookups : AutoRefPostCodeLookups
	{
		public RefPostCodeLookups(AutoRefPostCode parent) : base(parent)
		{
		}

		new RefPostCode Parent
		{
			get { return (RefPostCode)base.Parent; }
		}

		public RefCityTownCollection CityTowns
		{
			get
			{
				var result = new RefCityTownCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("CountryState", "Property1", Parent.RK_RN_NKCountry));

				if (Parent.Country != null)
				{
					result.AdditionalFilter = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, Parent.RK_RN_NKCountry);
				}
				else
				{
					result.AdditionalFilter = ZQuery.NoResultQuery;
				}

				return result;
			}
		}
	}
}
