using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ItineraryDataCollection : CusCodeDataCollection<ItineraryData>
	{
		public ItineraryDataCollection(BusinessObject parent) : base(parent, CusCodeDataTypeList.Codes.Itinerary)
		{
		}

		protected override bool AllowNewCore => base.AllowNewCore && Count < maxAllowed;

		const int maxAllowed = 99;
	}
}
