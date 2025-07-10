using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefPostcodeListProvider : RefListProvider
	{
		public RefPostcodeListProvider(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		public RefPostcodeListProvider(IBusinessObjectCollection collection, string countryCode, string cityTown, string state)
			: base(collection, countryCode, cityTown, state)
		{
		}

		public override ZGuid PrimaryKeyFromCode(string code)
		{
			return GetPrimaryKeyFromCode<RefCityTown>(code, RefPostCodeSchema.RK_RN_NKCountry, Helper.GetPostcodePKFromCode);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			var bizObjs = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				var query = new ZQuery();
				AddCodeEqualsFilter(query, code);
				query.AddToFilter(List.CompleteFilter);
				query.AddToFilter(RefPostCodeSchema.RK_IsActive, true);
				bizObjs = List.Factory.Load<RefPostCode>(query);
			}

			return bizObjs;
		}
	}
}
