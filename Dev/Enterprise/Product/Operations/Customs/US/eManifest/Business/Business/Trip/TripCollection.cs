using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class TripCollection : ActiveBusinessObjectCollection<Trip>
	{
		public TripCollection(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public TripCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, GetApplicationFilter(filter))
		{
		}

		static ZQuery GetApplicationFilter(ZQuery filter)
		{
			var result = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.eManifest);
			if (filter != null)
			{
				result.AddToFilter(filter);
			}
			return result;
		}
	}
}
