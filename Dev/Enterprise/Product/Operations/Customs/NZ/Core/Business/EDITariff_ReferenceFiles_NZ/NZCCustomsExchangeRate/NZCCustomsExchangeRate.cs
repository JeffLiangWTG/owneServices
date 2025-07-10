using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCCustomsExchangeRate : AutoNZCCustomsExchangeRate
	{
		public NZCCustomsExchangeRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static ZDateTime GetMostRecentExchangeRateEndDate(BusinessObjectFactory factory)
		{
			var query = new ZQuery();
			query.OrderBy = NZCCustomsExchangeRateSchema.U7_DateActiveTo.Name + " desc";
			var myRow = factory.LoadTop1<NZCCustomsExchangeRate>(query);
			return myRow?.U7_DateActiveTo.Date ?? ZDateTime.Empty;
		}
	}
}
