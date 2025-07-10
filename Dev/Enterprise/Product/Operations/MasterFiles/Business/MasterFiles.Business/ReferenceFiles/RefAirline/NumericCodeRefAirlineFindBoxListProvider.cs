using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class NumericCodeRefAirlineFindBoxListProvider : FindBoxListProvider
	{
		public NumericCodeRefAirlineFindBoxListProvider(ActiveBusinessObjectCollection<NumericCodeRefAirline> collection)
			: base(collection)
		{
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			var bizObjs = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				ZQuery query = new ZQuery { IgnoreActiveFilter = true };
				AddCodeEqualsFilter(query, code);

				bizObjs = List.Factory.Load(GetTypeOfElements(code), query);
			}

			return bizObjs;
		}
	}
}
