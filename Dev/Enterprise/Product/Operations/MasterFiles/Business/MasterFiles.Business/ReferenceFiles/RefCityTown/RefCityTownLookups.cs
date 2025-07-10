using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCityTownLookups : AutoRefCityTownLookups
	{
		public RefCityTownLookups(AutoRefCityTown parent) : base(parent)
		{
		}

		new RefCityTown Parent
		{
			get { return (RefCityTown)base.Parent; }
		}

		public override RefCountryStatesCollection States
		{
			get
			{
				var result = base.States;

				var country = Parent != null ? Parent.Country : null;
				if (country != null)
				{
					result.AdditionalFilter = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, country.RN_Code);
				}
				else
				{
					result.AdditionalFilter = ZQuery.NoResultQuery;
				}

				return result;
			}
		}

		public RefPostCodeCollection PostCodes
		{
			get
			{
				var result = new RefPostCodeCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country", "Property", Parent.R9_RN_NKCountry));

				if (Parent.Country != null)
				{
					result.AdditionalFilter = new ZQuery(RefPostCodeSchema.RK_RN_NKCountry, Parent.R9_RN_NKCountry);
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
