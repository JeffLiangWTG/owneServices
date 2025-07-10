using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class ViewLocationHelper
	{
		public static ViewLocation GetLocationFromString(BusinessObjectFactory factory, ZString locationCode, ZString locationTableCode)
		{
			var query = new ZQuery(ViewLocationSchema.VLO_Code, locationCode);
			query.AddToFilter(ViewLocationSchema.VLO_TableCode, locationTableCode);
			return factory.LoadTop1<ViewLocation>(query);
		}

		public static ViewLocation GetLocationFromPk(BusinessObjectFactory factory, ZGuid locationPk)
		{
			return factory.Load<ViewLocation>(locationPk);
		}
	}
}
