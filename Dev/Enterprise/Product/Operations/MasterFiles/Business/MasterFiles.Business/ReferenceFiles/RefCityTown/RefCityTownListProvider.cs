using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCityTownListProvider : RefListProvider
	{
		public RefCityTownListProvider(IBusinessObjectCollection collection)
				: base(collection)
		{
		}

		public RefCityTownListProvider(IBusinessObjectCollection collection, string countryCode, string postcode)
				: base(collection, countryCode, postcode)
		{
		}

		public override ZGuid PrimaryKeyFromCode(string code)
		{
			return GetPrimaryKeyFromCode<RefPostCode>(code, RefCityTownSchema.R9_RN_NKCountry, Helper.GetCityTownPKFromCode);
		}
	}
}
